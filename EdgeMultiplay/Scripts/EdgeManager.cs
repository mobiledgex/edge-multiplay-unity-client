/**
 * Copyright 2018-2021 MobiledgeX, Inc. All rights and licenses reserved.
 * MobiledgeX, Inc. 156 2nd Street #408, San Francisco, CA 94105
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using MobiledgeX;
using DistributedMatchEngine;
using System.Threading.Tasks;

namespace EdgeMultiplay
{
  /// <summary>
  /// EdgeManager is the class responsible for Connection to the server, Handling Server Events and storing game session
  /// EdgeManager requires LocationService if you are using a MobiledgeX Server
  /// EdgeManager have many static variables, your app/game should have only one EdgeManager
  /// </summary>
  [RequireComponent(typeof(MobiledgeX.LocationService))]
  [AddComponentMenu("EdgeMultiplay/EdgeManager")]
  public class EdgeManager : MonoBehaviour
  {
    #region  EdgeManager static variables and constants

    public static MobiledgeXIntegration integration;
    static MobiledgeXWebSocketClient wsClient;
    static MobiledgeXUDPClient udpClient;
    /// <summary>
    /// The Game Session stored after the player is registered on the server
    /// </summary>
    public static Session gameSession;
    /// <summary>
    /// list of the current players in the same room as the local player
    /// </summary>
    public static List<NetworkedPlayer> currentRoomPlayers = new List<NetworkedPlayer>();
    /// <summary>
    /// Represents the local player, to send messages to the server use EdgeManager.MessageSender.BroadcastMessage()
    /// </summary>
    public static NetworkedPlayer MessageSender;
    public static NetworkedPlayer localPlayer;
    /// <summary>
    /// Indicates that the game have already started
    /// </summary>
    public static bool gameStarted;
    public static List<EdgeMultiplayObserver> observers = new List<EdgeMultiplayObserver>();

    /// <summary>
    /// If you want to have a different World Origin, Players will be spawned relative to the Transform specified
    /// </summary>
    public Transform WorldOriginTransform;
    #endregion

    #region private variables

    byte[] udpMsg;
    string wsMsg;

    #endregion

    #region EdgeManager Editor Exposed Variables

    /// <summary>
    /// List of GameObjects to be used as player avatar
    /// </summary>
    [Header("Players", order = 0)]
    [Tooltip("Player Avatars to be instatiated in once the Game starts")]
    public List<NetworkedPlayer> SpawnPrefabs;

    /// <summary>
    /// List of Spawn Info, Spawn Info consists of Position and the Rotation Eulers
    /// </summary>
    [Tooltip("Positions for spawning player prefabs, the order is based on who connects first")]
    public List<PositionAndRotationEulers> SpawnInfo;
    private static EdgeManager instance;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
      DontDestroyOnLoad(this);
      if (instance == null)
      {
        instance = this;
      }
      else
      {
        Destroy(gameObject);
      }
      integration = new MobiledgeXIntegration();
    }

    void Update()
    {
      if (wsClient == null)
      {
        return;
      }
      //websocket receive queue
      var ws_queue = wsClient.receiveQueue;
      while (ws_queue.TryPeek(out wsMsg))
      {
        ws_queue.TryDequeue(out wsMsg);
        HandleWebSocketMessage(wsMsg);
        wsMsg = null;
      }
      if (udpClient == null)
      {
        return;
      }
      //udp receive queue
      while (udpClient.receiveQueue.TryPeek(out udpMsg))
      {
        udpClient.receiveQueue.TryDequeue(out udpMsg);
        HandleUDPMessage(Encoding.UTF8.GetString(udpMsg));
        udpMsg = null;
      }
    }

    void OnDestroy()
    {
      observers = null;
      currentRoomPlayers = null;
      if (wsClient != null)
      {
        wsClient.tokenSource.Cancel();
      }
      if (udpClient != null)
      {
        udpClient.Dispose();
      }
      if (Application.platform == RuntimePlatform.OSXPlayer
         || Application.platform == RuntimePlatform.WindowsPlayer
         || Application.platform == RuntimePlatform.LinuxPlayer)
      {
        Environment.Exit(0);
      }
    }

    #endregion

    #region Static EdgeManager functions
    /// <summary>
    /// Connect to your EdgeMultiplay server based on location and carrier info
    /// <para>
    /// use <b>public override void OnConnectionToEdge()</b>  to get the server response
    /// </para>
    /// </summary>
    /// <param name="useAnyCarrierNetwork"> set to true for connection based on location info only </param>
    /// <param name="useFallBackLocation"> set to true to use overloaded location sat in setFallbackLocation()</param>
    /// <param name="path"> You can specify a path for your connection to be verified on the server side </param>
    /// <returns> Connection Task, use OnConnectionToEdge() to listen to the async task result </returns>
    public async Task ConnectToServer(bool useAnyCarrierNetwork = true, bool useFallBackLocation = false, string path = "")
    {
      if (Configs.clientSettings.useLocalHostServer)
      {
        try
        {
          gameSession = new Session();
          wsClient = new MobiledgeXWebSocketClient();
          Uri uri = new Uri("ws://" + Configs.clientSettings.hostIPAddress + ":" + Configs.clientSettings.WebSocketPort + path);
          if (wsClient.isOpen())
          {
            wsClient.Dispose();
            wsClient = new MobiledgeXWebSocketClient();
          }
          await wsClient.Connect(@uri);
          EdgeMultiplayCallbacks.connectedToEdge();
        }
        catch (Exception e)
        {
          EdgeMultiplayCallbacks.failureToConnect(e.Message);
          Debug.LogError("EdgeMultiplay: Failed to connect to your Local Host, Check console for more details");
        }
      }
      else
      {
        try
        {
          gameSession = new Session();
          integration.UseWifiOnly(useAnyCarrierNetwork);
          integration.useFallbackLocation = useFallBackLocation;
          wsClient = new MobiledgeXWebSocketClient();
          await integration.RegisterAndFindCloudlet();
          integration.GetAppPort(LProto.L_PROTO_TCP, Configs.clientSettings.WebSocketPort);
          string url = integration.GetUrl("ws") + path;
          Uri uri = new Uri(url);
          if (wsClient.isOpen())
          {
            wsClient.Dispose();
            wsClient = new MobiledgeXWebSocketClient();
          }
          await wsClient.Connect(@uri);
          EdgeMultiplayCallbacks.connectedToEdge();
        }
        catch (Exception e)
        {
          EdgeMultiplayCallbacks.failureToConnect(e.Message);
          Debug.LogError("EdgeMultiplay: Failed to connect to Edge, Check console for more details");
        }
      }
    }

    /// <summary>
    /// Get Player using the player id
    /// </summary>
    /// <param name="playerId"> playerId is a unique Id assigned to player during OnRegister() and saved into EdgeManager.gameSession.playerId</param>
    /// <returns> The NetworkedPlayer of the supplied playerId </returns>
    public static NetworkedPlayer GetPlayer(string playerId)
    {
      NetworkedPlayer playerRequested;
      playerRequested = currentRoomPlayers.Find(player => player.playerId == playerId);
      if (playerRequested == null)
      {
        Debug.LogError("Error finding player with id: " + playerId);
      }
      return playerRequested;
    }

    /// <summary>
    /// Get Player  using the player index in the room
    /// </summary>
    /// <param name="playerIndex"> playerIndex is an id assigned to player based on the precedence of joining the room </param>
    /// <returns> The NetworkedPlayer of the supplied playerIndex </returns>
    public static NetworkedPlayer GetPlayer(int playerIndex)
    {
      NetworkedPlayer playerRequested;
      playerRequested = currentRoomPlayers.Find(player => player.playerIndex == playerIndex);
      if (playerRequested == null)
      {
        Debug.LogError("Error finding player with index: " + playerIndex);
      }
      return playerRequested;
    }

    /// <summary>
    /// Sends Join Or Create Room request to the server, the server will try to match a player with any available room
    /// if the server didn't find an available room, the server will create a room for the player
    /// <para>
    /// use <b>public override void OnRoomCreated</b> or <b>public override void OnRoomJoin</b> to get the server response
    /// </para>
    /// </summary>
    /// <param name="playerName"> player name to be assigned to player</param>
    /// <param name="playerAvatar">(integer value) Avatar Index from EdgeManager Spawn Prefabs</param>
    /// <param name="maxPlayersPerRoom">In case of room creation, the maximum players allowed in the room</param>
    /// <param name="minPlayersPerRoom">In case of room creation, the minimum players threshold to start a game, if less than 2, minPlayersPerRoom == maxPlayersPerRoom </param>
    /// <param name="playerTags">Dictionary<string,string> custom data associated with the player</param> 
    public static void JoinOrCreateRoom(string playerName, int playerAvatar, int maxPlayersPerRoom, Dictionary<string, string> playerTags = null, int minPlayersToStartGame = 0)
    {
      if (maxPlayersPerRoom < 2)
      {
        Debug.LogError("EdgeMultiplay : maxPlayersPerRoom must be greater than 1");
        return;
      }
      Hashtable playertagsHashtable;
      if (playerTags != null)
      {
        playertagsHashtable = Tag.DictionaryToHashtable(playerTags);
      }
      else
      {
        playertagsHashtable = null;
      }
      JoinOrCreateRoomRequest createOrJoinRoomRequest = new JoinOrCreateRoomRequest(playerName, playerAvatar, maxPlayersPerRoom, playertagsHashtable, minPlayersToStartGame);
      wsClient.Send(Messaging<JoinOrCreateRoomRequest>.Serialize(createOrJoinRoomRequest));
    }

    /// <summary>
    /// Sends a request to the server to get a full list of rooms on the servers
    /// <para>
    /// use <b>public override void OnRoomsListReceived</b> to get the server response
    /// </para>
    /// </summary>
    public static void GetRooms()
    {
      GetRoomsRequest getRoomsRequest = new GetRoomsRequest();
      wsClient.Send(Messaging<GetRoomsRequest>.Serialize(getRoomsRequest));
    }

    /// <summary>
    /// Sends a request to the server to get a full list of rooms on the servers
    /// <para>
    /// use <b>public override void OnRoomsListReceived</b> to get the server response
    /// </para>
    /// </summary>
    public static void GetAvailableRooms()
    {
      GetAvailableRoomsRequest getAvailableRoomsRequest = new GetAvailableRoomsRequest();
      wsClient.Send(Messaging<GetAvailableRoomsRequest>.Serialize(getAvailableRoomsRequest));
    }

    /// <summary>
    /// Sends a request to the server to create a room
    /// <para>
    /// use <b>public override void OnRoomCreated</b>  to get the server response
    /// </para>
    /// </summary>
    /// <param name="playerName">player name to be assigned to player</param>
    /// <param name="playerAvatar">(integer value) Avatar Index from EdgeManager Spawn Prefabs</param>
    /// <param name="maxPlayersPerRoom">The maximum players allowed in the room</param>
    /// <param name="minPlayersPerRoom">In case of room creation, the minimum players threshold to start a game, if less than 2, minPlayersPerRoom == maxPlayersPerRoom </param>
    /// <param name="playerTags">Dictionary<string,string> custom data associated with the player</param>
    public static void CreateRoom(string playerName, int playerAvatar, int maxPlayersPerRoom, Dictionary<string, string> playerTags = null, int minPlayersToStartGame = 0)
    {
      if (maxPlayersPerRoom < 2)
      {
        Debug.LogError("EdgeMultiplay : maxPlayersPerRoom must be greater than 1");
        return;
      }
      // Assure Player is not already a member of another room  
      if (gameSession.roomId == "")
      {
        Hashtable playertagsHashtable;
        if (playerTags != null)
        {
          playertagsHashtable = Tag.DictionaryToHashtable(playerTags);
        }
        else
        {
          playertagsHashtable = null;
        }
        CreateRoomRequest createRoomRequest = new CreateRoomRequest(playerName, playerAvatar, maxPlayersPerRoom, playertagsHashtable, minPlayersToStartGame);
        wsClient.Send(Messaging<CreateRoomRequest>.Serialize(createRoomRequest));
      }
      else
      {
        Debug.LogError("EdgeMultiplay : Player is already a member in another room");
      }
    }

    /// <summary>
    /// Sends a request to the server to join a room
    /// <para>
    /// use <b>public override void OnRoomJoin</b>  to get the server response
    /// </para>
    /// </summary>
    /// <param name="roomId">Id of the room intended to join</param>
    /// <param name="playerAvatar">(integer value) Avatar Index from EdgeManager Spawn Prefabs</param>
    /// <param name="playerTags">Dictionary<string,string> custom data associated with the player</param>
    public static void JoinRoom(string roomId, string playerName, int playerAvatar, Dictionary<string, string> playerTags = null)
    {
      if (gameSession.roomId == "")
      {
        Hashtable playertagsHashtable;
        if (playerTags != null)
        {
          playertagsHashtable = Tag.DictionaryToHashtable(playerTags);
        }
        else
        {
          playertagsHashtable = null;
        }
        JoinRoomRequest joinRoomRequest = new JoinRoomRequest(roomId, playerName, playerAvatar, playertagsHashtable);
        wsClient.Send(Messaging<JoinRoomRequest>.Serialize(joinRoomRequest));
      }
      else
      {
        if (gameSession.roomId == roomId)
        {
          Debug.LogError("EdgeMultiplay : Player is already a member in this room");
        }
        else
        {
          Debug.LogError("EdgeMultiplay : Player is already a member in another room");
        }
      }
    }

    /// <summary>
    /// Sends a UDP Message from local player to other room members, can be used only after the game starts (OnGameStart())
    /// <para>
    /// on the player manager that inherits from NetworkedPlayer use
    /// <b>public override void OnMessageReceived</b>  to get the forwarded message
    /// </para>
    /// <para>
    /// <b>UDP Messages are limited to 508 bytes</b> if you want to exceed that make sure you slice and reassemble your buffer
    /// </para>
    /// </summary>
    /// <param name="gameplayEvent">the GamePlay Event to be forwarded to other room members</param>
    public static void SendUDPMessage(GamePlayEvent gameplayEvent)
    {
      gameplayEvent.roomId = gameSession.roomId;
      gameplayEvent.senderId = gameSession.playerId;
      if (udpClient.run)
      {
        udpClient.Send(gameplayEvent.ToJson());
      }
      else
      {
        Debug.LogError("EdgeMultiplay: Error in sending UDP Message");
      }
    }

    /// <summary>
    /// Kill the connection to your EdgeMultiplay server
    /// </summary>
    public static void Disconnect()
    {
      wsClient = null;
      udpClient = null;
      foreach (NetworkedPlayer player in currentRoomPlayers)
      {
        Destroy(player.gameObject);
      }
      currentRoomPlayers = new List<NetworkedPlayer>();
      gameSession = new Session();
      gameStarted = false;
      MessageSender = null;
      localPlayer = null;
      observers = new List<EdgeMultiplayObserver>();
    }

    /// <summary>
    /// Exit the current room you are in
    /// </summary>
    public static void ExitRoom()
    {
      ExitRoomRequest exitRoomRequest = new ExitRoomRequest();
      wsClient.Send(Messaging<ExitRoomRequest>.Serialize(exitRoomRequest));
    }

    #endregion

    #region EdgeManager Functions

    public void SendGamePlayEvent(GamePlayEvent mobiledgexEvent)
    {
      mobiledgexEvent.roomId = gameSession.roomId;
      mobiledgexEvent.senderId = gameSession.playerId;
      wsClient.Send(Messaging<GamePlayEvent>.Serialize(mobiledgexEvent));
    }

    void HandleWebSocketMessage(string message)
    {
      var msg = MessageWrapper.UnWrapMessage(message);
      switch (msg.type)
      {
        case "register":
          Register register = Messaging<Register>.Deserialize(message);
          gameSession.sessionId = register.sessionId;
          gameSession.playerId = register.playerId;
          EdgeMultiplayCallbacks.registerEvent();
          break;

        case "notification":
          Notification notification = Messaging<Notification>.Deserialize(message);
          switch (notification.notificationText)
          {
            case "left-room":
              gameSession.roomId = "";
              EdgeMultiplayCallbacks.leftRoom();
              break;
            case "join-room-faliure":
              EdgeMultiplayCallbacks.joinRoomFaliure();
              break;
            case "new-room-created-in-lobby":
              EdgeMultiplayCallbacks.newRoomCreatedInLobby();
              break;
            case "room-removed-from-lobby":
              EdgeMultiplayCallbacks.roomRemovedFromLobby();
              break;
            case "rooms-updated":
              EdgeMultiplayCallbacks.roomsUpdated();
              break;
          }
          EdgeMultiplayCallbacks.notificationEvent(notification);
          break;

        case "roomsList":
          RoomsList roomsList = Messaging<RoomsList>.Deserialize(message);
          EdgeMultiplayCallbacks.roomsList(roomsList.rooms);
          break;

        case "roomCreated":
          RoomCreated roomCreated = Messaging<RoomCreated>.Deserialize(message);
          gameSession.roomId = roomCreated.room.roomId;
          EdgeMultiplayCallbacks.roomCreated(roomCreated.room);
          break;

        case "roomJoin":
          RoomJoin roomJoin = Messaging<RoomJoin>.Deserialize(message);
          gameSession.roomId = roomJoin.room.roomId;
          EdgeMultiplayCallbacks.roomJoin(roomJoin.room);
          int roomMembersCount = roomJoin.room.roomMembers.Count;
          if (roomMembersCount >= roomJoin.room.minPlayersToStartGame) //game already started
          {
            StartGame(roomJoin.room);
          }
          break;

        case "playerJoinedRoom":
          PlayerJoinedRoom playerJoinedRoom = Messaging<PlayerJoinedRoom>.Deserialize(message);
          EdgeMultiplayCallbacks.playerRoomJoined(playerJoinedRoom.room);
          if (playerJoinedRoom.room.roomMembers[playerJoinedRoom.room.roomMembers.Count - 1].playerId == gameSession.playerId)
          {
            break;
          }
          else
          {
            if (gameStarted)
            {
              NetworkedPlayer networkedPlayer = SpawnPlayer(playerJoinedRoom.room.roomMembers[playerJoinedRoom.room.roomMembers.Count - 1]);
              currentRoomPlayers.Add(networkedPlayer);
              EdgeMultiplayCallbacks.eventReceived += networkedPlayer.OnWebSocketEventReceived;
              EdgeMultiplayCallbacks.udpEventReceived += networkedPlayer.OnUDPEventReceived;
            }
            else if (playerJoinedRoom.room.roomMembers.Count == playerJoinedRoom.room.minPlayersToStartGame)
            {
              StartGame(playerJoinedRoom.room);
            }
          }
          break;

        case "gameStart":
          GameStart gameStart = Messaging<GameStart>.Deserialize(message);
          EdgeMultiplayCallbacks.gameStart();
          break;

        case "GamePlayEvent":
          GamePlayEvent gamePlayEvent = Messaging<GamePlayEvent>.Deserialize(message);
          switch (gamePlayEvent.eventName)
          {
            case "NewObservableCreated":
              CreateObservableObject(gamePlayEvent);
              break;
            case "ObservableOwnershipChange":
              UpdateObserverOwnership(gamePlayEvent);
              break;
            case "TakeOverObservable":
              TakeOverObservable(gamePlayEvent);
              break;
            case "OwnershipRequest":
              OwnershipRequestReceived(gamePlayEvent);
              break;
            default:
              ReflectEvent(gamePlayEvent);
              break;
          }
          break;

        case "memberLeft":
          RoomMemberLeft playerLeft = Messaging<RoomMemberLeft>.Deserialize(message);
          EdgeMultiplayCallbacks.playerLeft(playerLeft);
          break;

        default:
          Debug.LogError("Unknown WebSocket message arrived: " + msg.type + ", message: " + message);
          break;
      }
    }

    void StartGame(Room room)
    {
      gameSession.currentPlayers = room.roomMembers.ToArray();
      foreach (Player player in room.roomMembers)
      {
        if (player.playerId == gameSession.playerId)
        {
          gameSession.playerIndex = player.playerIndex;
        }
      }
      CreatePlayers(gameSession.currentPlayers);
      gameStarted = true;
      if (Configs.clientSettings.useLocalHostServer)
      {
        udpClient = new MobiledgeXUDPClient(Configs.clientSettings.hostIPAddress, Configs.clientSettings.UDPPort);
      }
      else
      {
        udpClient = new MobiledgeXUDPClient(integration.GetHost(), integration.GetAppPort(LProto.L_PROTO_UDP, Configs.clientSettings.UDPPort).public_port);
      }
      SendUDPMessage(new GamePlayEvent() { eventName = "Start" });
    }

    void HandleUDPMessage(string message)
    {
      GamePlayEvent gamePlayEvent = JsonUtility.FromJson<GamePlayEvent>(message);
      switch (gamePlayEvent.type)
      {
        case "GamePlayEvent":
          if (gamePlayEvent.eventName == "EdgeMultiplayObserver")
          {
            SyncObject(gamePlayEvent);
          }
          else
          {
            // if used for other than that Syncing GameObjects Position & Rotation
            // it wil trigger OnUDPMessagesReceived()
            EdgeMultiplayCallbacks.udpEventReceived(gamePlayEvent);
          }
          break;
        default:
          Debug.LogError("Unknown UDP message arrived: " + message);
          break;
      }
    }

    void CreateObservableObject(GamePlayEvent newObservableEvent)
    {
      if (localPlayer.playerId == newObservableEvent.stringData[0])
      {
        return;
      }
      NetworkedPlayer playerCreatedObserver = GetPlayer(newObservableEvent.stringData[0]);
      Observable observable = playerCreatedObserver.CreateObservableObject(
          prefabName: newObservableEvent.stringData[1],
          startPosition: Util.ConvertFloatArrayToVector3(newObservableEvent.floatData, 0),
          startRotation: Quaternion.Euler(Util.ConvertFloatArrayToVector3(newObservableEvent.floatData, 3)),
          syncOption: (SyncOptions)Enum.ToObject(typeof(SyncOptions), newObservableEvent.integerData[0]),
          interpolatePosition: newObservableEvent.booleanData[0],
          interpolateRotation: newObservableEvent.booleanData[1],
          interpolationFactor: newObservableEvent.floatData[6]);
      EdgeMultiplayCallbacks.newObservableCreated(observable);

    }

    /// <summary>
    /// When an observable object change its owner, OwnershipChangeEvent is sent from the owner to other players
    /// Changing ownership must occur at the owner world
    /// </summary>
    /// <param name="ownershipChangeEvent">OwnershipChangeEvent contains (oldOwnerId, the observable Index and the newOwnerId)</param>
    void UpdateObserverOwnership(GamePlayEvent ownershipChangeEvent)
    {
      if (localPlayer.playerId == ownershipChangeEvent.senderId)
      {
        return;
      }
      NetworkedPlayer oldOwner = GetPlayer(ownershipChangeEvent.senderId);
      Observable observer;
      if (oldOwner.observer != null)
      {
        observer = oldOwner.observer.observables[ownershipChangeEvent.integerData[0]];
      }
      else
      {
        Debug.LogError("Couldn't find old owner.observer");
        return;
      }
      NetworkedPlayer newOwner = GetPlayer(ownershipChangeEvent.stringData[0]);
      if (newOwner.observer == null)
      {
        newOwner.gameObject.AddComponent<EdgeMultiplayObserver>();
      }
      newOwner.observer.observables.Add(observer);
      newOwner.observer.UpdateObservables();
      oldOwner.observer.observables.RemoveAt(ownershipChangeEvent.integerData[0]);
    }

    void TakeOverObservable(GamePlayEvent gamePlayEvent)
    {
      // check if the observable owner is the local player
      if (gamePlayEvent.stringData[0] == localPlayer.playerId)
      {
        //get the observable
        Observable observable = localPlayer.observer.observables
            .Find(obs => obs.observableIndex == gamePlayEvent.integerData[0]);
        if (observable == null)
        {
          Debug.LogWarning("EdgeMultiplay: couldn't find the observable");
          return;
        }
        // change the observable ownership from the current owner to the sender
        observable.ChangeOwnership(gamePlayEvent.senderId);
      }
    }

    void OwnershipRequestReceived(GamePlayEvent gamePlayEvent)
    {
      // check if the observable owner is the local player
      if (gamePlayEvent.stringData[0] == localPlayer.playerId)
      {
        //get the observable
        Observable observable = localPlayer.observer.observables
            .Find(obs => obs.observableIndex == gamePlayEvent.integerData[0]);
        if (observable == null)
        {
          Debug.LogWarning("EdgeMultiplay: couldn't find the observable");
          return;
        }
        NetworkedPlayer requestee = GetPlayer(gamePlayEvent.senderId);

        // Inform the current owner about the ownership request
        // Triggers OnOwnershipRequestReceived() callback
        localPlayer.ownershipRequested(requestee, observable);
      }
    }

    /// <summary>
    /// If the LocalPlayer is observing any transforms, once there is any update to the observed transform
    /// the local player will send the updated transfrom to its clones in the other players' world.
    /// </summary>
    /// <param name="receivedEvent"> the received gameplay event contains (observable owner id, observable index, syncOption, updated transform data) </param>
    void SyncObject(GamePlayEvent receivedEvent)
    {
      if (receivedEvent.senderId == localPlayer.playerId)
      {
        return;
      }
      NetworkedPlayer sourcePlayer = GetPlayer(receivedEvent.senderId);
      if (sourcePlayer.isLocalPlayer)
      {
        return;
      }
      Observable observableObj;
      int observableIndex = receivedEvent.integerData[1];
      observableObj = sourcePlayer.observer.observables.Find(observer => observer.observableIndex == observableIndex);
      if (observableObj == null)
      {
        Debug.LogError("No observer found with this id " + receivedEvent.integerData[1]);
        return;
      }
      switch (receivedEvent.integerData[0])
      {
        case (int)SyncOptions.SyncPosition:
          observableObj.observeredTransform.transform.position = Util.ConvertFloatArrayToVector3(receivedEvent.floatData, 0);
          break;
        case (int)SyncOptions.SyncRotation:
          observableObj.observeredTransform.transform.rotation = Quaternion.Euler(Util.ConvertFloatArrayToVector3(receivedEvent.floatData, 0));
          break;
        case (int)SyncOptions.SyncPositionAndRotation:
          observableObj.observeredTransform.transform.position = Util.ConvertFloatArrayToVector3(receivedEvent.floatData, 0);
          observableObj.observeredTransform.transform.rotation = Quaternion.Euler(Util.ConvertFloatArrayToVector3(receivedEvent.floatData, 3));
          break;
        case (int)SyncOptions.SyncLocalPosition:
          Util.SetLocalPostion(observableObj.observeredTransform.transform, Util.ConvertFloatArrayToVector3(receivedEvent.floatData, 0));
          break;
        case (int)SyncOptions.SyncLocalRotation:
          Util.SetLocalRotation(observableObj.observeredTransform.transform, Util.ConvertFloatArrayToVector3(receivedEvent.floatData, 0));
          break;
        case (int)SyncOptions.SyncLocalPositionAndRotation:
          Util.SetLocalPostion(observableObj.observeredTransform.transform, Util.ConvertFloatArrayToVector3(receivedEvent.floatData, 0));
          Util.SetLocalRotation(observableObj.observeredTransform.transform, Util.ConvertFloatArrayToVector3(receivedEvent.floatData, 3));
          break;
      }
    }

    void ReflectEvent(GamePlayEvent receivedEvent)
    {
      EdgeMultiplayCallbacks.eventReceived(receivedEvent);
    }

    void CreatePlayers(Player[] gamePlayers)
    {
      try
      {
        foreach (Player player in gamePlayers)
        {
          NetworkedPlayer networkedPlayer = SpawnPlayer(player);
          currentRoomPlayers.Add(networkedPlayer);
          EdgeMultiplayCallbacks.eventReceived += networkedPlayer.OnWebSocketEventReceived;
          EdgeMultiplayCallbacks.udpEventReceived += networkedPlayer.OnUDPEventReceived;
          if (player.playerId == gameSession.playerId)
          {
            print("XXXX LOCATED LOCAL PLAYER");
            localPlayer = MessageSender = networkedPlayer;
          }
        }
      }
      catch (NullReferenceException)
      {
        throw new Exception("EdgeMultiplay: Error in creating players, Make sure to attach your Prefabs to EdgeManager.SpawnInfo in the inspector");
      }
      catch (ArgumentOutOfRangeException)
      {
        throw new Exception("EdgeMultiplay: Error in creating players, Make sure Size of EdgeManager Spawn Info equal or greater than number of players in the room");
      }
      catch (Exception)
      {
        throw new Exception("EdgeMultiplay: Error in creating players");
      }
    }
    NetworkedPlayer SpawnPlayer(Player player)
    {
      GameObject playerObj = SpawnPrefabs[player.playerAvatar].gameObject;
      playerObj.GetComponent<NetworkedPlayer>().SetUpPlayer(player, gameSession.roomId, player.playerId == gameSession.playerId);

      GameObject playerCreated;
      if (WorldOriginTransform != null)
      {
        playerCreated = Instantiate(playerObj, WorldOriginTransform);
        Util.SetLocalPostion(playerCreated.transform, SpawnInfo[player.playerIndex].position);
        Util.SetLocalRotation(playerCreated.transform, SpawnInfo[player.playerIndex].rotation);
      }
      else
      {
        playerCreated = Instantiate(playerObj, SpawnInfo[player.playerIndex].position, Quaternion.Euler(SpawnInfo[player.playerIndex].rotation));
      }
      NetworkedPlayer networkedPlayer = playerCreated.GetComponent<NetworkedPlayer>();
      if (player.playerName == "")
      {
        playerCreated.name = networkedPlayer.playerName = "Player " + (player.playerIndex + 1);
      }
      else
      {
        playerCreated.name = player.playerName;
      }

      return networkedPlayer;
    }
    #endregion

  }
}
