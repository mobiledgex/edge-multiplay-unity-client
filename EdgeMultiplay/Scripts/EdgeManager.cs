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
        public static bool localPlayerIsMaster;
        /// <summary>
        /// Indicates that the game have already started
        /// </summary>
        public static bool gameStarted;
        public static List<EdgeMultiplayObserver> observers = new List<EdgeMultiplayObserver>();

        public const int defaultEdgeMultiplayServerUDPPort = 5000;
        public const int defaultEdgeMultiplayServerTCPPort = 3000;

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
        public List<GameObject> SpawnPrefabs;

        /// <summary>
        /// List of Spawn Info, Spawn Info consists of Position and the Rotation Eulers
        /// </summary>
        [Tooltip("Positions for spawning player prefabs, the order is based on who connects first")]
        public List<PositionAndRotationEulers> SpawnInfo;

        [Header("Configuration", order = 10)]
        /// <summary>
        /// Set to true if you have EdgeMultiplay Server running on your machine
        /// </summary>
        [Tooltip("Set to true if you have EdgeMultiplay Server running on your machine")]
        public bool useLocalHostServer;

        /// <summary>
        /// Use 127.0.0.1 as your IP Address for testing on you computer only
        /// Or use the Host IP Address for testing between your Computer and devices connected to the same WifiNetwork
        /// For Mac : Open Terminal and type "ifconfig" and copy the "en0" address
        /// For Windows : Open CMD and type "Ipconfig /all" and copy the "IPV4" address
        /// </summary>
        [Tooltip("Use 127.0.0.1 as your IP Address for testing on you computer only,\n" +
            "Or use the Host IP Address for testing between your Computer and devices connected to the same WifiNetwork\n" +
            "For Mac : Open Terminal and type \"ifconfig\" and copy the \"en0\" address \n" +
            "For Windows : Open CMD and type \"Ipconfig /all \" and copy the \"IPV4\" address ")]
        public string hostIPAddress;

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
        public async Task ConnectToServer(bool useAnyCarrierNetwork = true, bool useFallBackLocation = false)
        {
            if (useLocalHostServer)
            {
                try
                {
                    gameSession = new Session();
                    wsClient = new MobiledgeXWebSocketClient();
                    Uri uri = new Uri("ws://" + hostIPAddress + ":" + defaultEdgeMultiplayServerTCPPort);
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
                    integration = new MobiledgeXIntegration();
                    integration.UseWifiOnly(useAnyCarrierNetwork);
                    integration.useFallbackLocation = useFallBackLocation;
                    wsClient = new MobiledgeXWebSocketClient();
                    try
                    {
                        await integration.RegisterAndFindCloudlet();
                    }
                    catch (LocationException)
                    {
                        MobiledgeXIntegration.LocationFromIPAddress location = await MobiledgeXIntegration.GetLocationFromIP();
                        integration.SetFallbackLocation(location.longitude, location.latitude);
                        await integration.RegisterAndFindCloudlet();
                    }
                    integration.GetAppPort(LProto.L_PROTO_TCP);
                    string url = integration.GetUrl("ws");
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
        /// Get Player based on playerId
        /// </summary>
        /// <param name="playerId"> playerId is a unique Id assigned to player during OnRegister() and saved into EdgeManager.gameSession.playerId</param>
        /// <returns></returns>
        public static NetworkedPlayer GetPlayer(string playerId)
        {
            return currentRoomPlayers.Find(player => player.playerId == playerId);
        }

        /// <summary>
        /// Get Player based on playerId
        /// </summary>
        /// <param name="playerIndex"> playerIndex is an id assigned to player based on the precedence of joining the room </param>
        /// <returns></returns>
        public static NetworkedPlayer GetPlayer(int playerIndex)
        {
            return currentRoomPlayers.Find(player => player.playerIndex == playerIndex);
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
        public static void JoinOrCreateRoom(string playerName, int playerAvatar, int maxPlayersPerRoom)
        {
            if(maxPlayersPerRoom < 2)
            {
                Debug.LogError("EdgeMultiplay : maxPlayersPerRoom must be greater than 1");
                return;
            }
            JoinOrCreateRoomRequest createOrJoinRoomRequest = new JoinOrCreateRoomRequest(playerName, playerAvatar, maxPlayersPerRoom);
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
        public static void CreateRoom(string playerName, int playerAvatar, int maxPlayersPerRoom)
        {
            if (maxPlayersPerRoom < 2)
            {
                Debug.LogError("EdgeMultiplay : maxPlayersPerRoom must be greater than 1");
                return;
            }
            // Assure Player is not already a member of another room  
            if (gameSession.roomId == "")
            {
                CreateRoomRequest createRoomRequest = new CreateRoomRequest(playerName, playerAvatar, maxPlayersPerRoom);
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
        public static void JoinRoom(string roomId, string playerName, int playerAvatar)
        {
            if (gameSession.roomId == "")
            {
                JoinRoomRequest joinRoomRequest = new JoinRoomRequest(roomId, playerName, playerAvatar);
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
            print("WS Msg " + msg);
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
                    break;

                case "playerJoinedRoom":
                    PlayerJoinedRoom playerJoinedRoom = Messaging<PlayerJoinedRoom>.Deserialize(message);
                    EdgeMultiplayCallbacks.playerRoomJoined(playerJoinedRoom.room);
                    break;

                case "gameStart":
                    GameStart gameStart = Messaging<GameStart>.Deserialize(message);
                    gameSession.currentPlayers = gameStart.room.roomMembers.ToArray();
                    foreach (Player player in gameStart.room.roomMembers)
                    {
                        if (player.playerId == gameSession.playerId)
                        {
                            gameSession.playerIndex = player.playerIndex;
                        }
                    }
                    CreatePlayers(gameSession.currentPlayers);
                    gameStarted = true;
                    EdgeMultiplayCallbacks.gameStart();
                    if (useLocalHostServer)
                    {
                        udpClient = new MobiledgeXUDPClient(hostIPAddress, defaultEdgeMultiplayServerUDPPort);
                    }
                    else
                    {
                        udpClient = new MobiledgeXUDPClient(integration.GetHost(), integration.GetAppPort(LProto.L_PROTO_UDP).public_port);
                    }
                    SendUDPMessage(new GamePlayEvent(){eventName = "Start"});
                    break;

                case "GamePlayEvent":
                    GamePlayEvent gamePlayEvent = Messaging<GamePlayEvent>.Deserialize(message);
                    if (gamePlayEvent.eventName == "NewObserverCreated")
                    {
                        CreateObserver(gamePlayEvent);
                        break;
                    }
                    ReflectEvent(gamePlayEvent);
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

        void CreateObserver(GamePlayEvent newObserverEvent)
        {
            if(localPlayer.playerId == newObserverEvent.stringData[0])
            {
                return;
            }
            NetworkedPlayer playerCreatedObserver = GetPlayer(newObserverEvent.stringData[0]);
            playerCreatedObserver.CreateObserverObject(
                prefabName: newObserverEvent.stringData[1],
                startPosition: new Vector3 ( newObserverEvent.floatData[0], newObserverEvent.floatData[1] , newObserverEvent.floatData[2] ),
                startRotation: Quaternion.Euler(new Vector3(newObserverEvent.floatData[3], newObserverEvent.floatData[4], newObserverEvent.floatData[5])),
                syncOption: (SyncOptions)Enum.ToObject(typeof(SyncOptions), newObserverEvent.integerData[0]),
                interpolatePosition: newObserverEvent.booleanData[0],
                interpolateRotation: newObserverEvent.booleanData[1],
                interpolationFactor: newObserverEvent.floatData[6]);
        }

        void SyncObject(GamePlayEvent receivedEvent)
        {
            if (receivedEvent.senderId == gameSession.playerId)
            {
                return;
            }
            EdgeMultiplayObserver edgeMultiplayObserver;
            //if (receivedEvent.booleanData[0]) // player object
            //{
                edgeMultiplayObserver = observers.Find(observer => observer.GetComponent<NetworkedPlayer>()
                && observer.GetComponent<NetworkedPlayer>().playerId == receivedEvent.stringData[0]);
            //}
            //else
            //{
                //edgeMultiplayObserver = observers.Find(observer => observer.eventId == receivedEvent.stringData[0]);
            //}
            int observerId = receivedEvent.integerData[1];
            switch (receivedEvent.integerData[0])
            {
                case (int)SyncOptions.SyncPosition:
                    edgeMultiplayObserver.observers[observerId].positionFromServer = new Vector3(receivedEvent.floatData[0], receivedEvent.floatData[1], receivedEvent.floatData[2]);
                    break;
                case (int)SyncOptions.SyncRotation:
                    edgeMultiplayObserver.observers[observerId].rotationFromServer = new Vector3(receivedEvent.floatData[0], receivedEvent.floatData[1], receivedEvent.floatData[2]);
                    break;
                case (int)SyncOptions.SyncPositionAndRotation:
                    edgeMultiplayObserver.observers[observerId].positionFromServer = new Vector3(receivedEvent.floatData[0], receivedEvent.floatData[1], receivedEvent.floatData[2]);
                    edgeMultiplayObserver.observers[observerId].rotationFromServer = new Vector3(receivedEvent.floatData[3], receivedEvent.floatData[4], receivedEvent.floatData[5]);
                    break;
            }
        }

        void ReflectEvent(GamePlayEvent receivedEvent)
        {
            EdgeMultiplayCallbacks.eventReceived(receivedEvent);
        }

        void CreatePlayers(Player[] gamePlayers)
        {
            if (currentRoomPlayers.Count < 1 && gamePlayers.Length > 1)
            {
                try
                {
                    foreach (Player player in gamePlayers)
                    {
                        GameObject playerObj = SpawnPrefabs[player.playerAvatar];
                        playerObj.GetComponent<NetworkedPlayer>().SetUpPlayer(player, gameSession.roomId, player.playerId == gameSession.playerId);

                        GameObject playerCreated;
                        if (WorldOriginTransform != null)
                        {
                            playerCreated = Instantiate(playerObj, WorldOriginTransform);
                            var position = playerCreated.transform.localPosition;
                            position.Set(SpawnInfo[player.playerIndex].position.x, SpawnInfo[player.playerIndex].position.y, SpawnInfo[player.playerIndex].position.z);
                            playerCreated.transform.localPosition = position;

                            var rotation = playerCreated.transform.localRotation;
                            Quaternion tempRotation = Quaternion.Euler(SpawnInfo[player.playerIndex].rotation);
                            rotation.Set(tempRotation.x, tempRotation.y, tempRotation.z, tempRotation.w);
                            playerCreated.transform.localRotation = rotation;
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
                        currentRoomPlayers.Add(networkedPlayer);
                        EdgeMultiplayCallbacks.eventReceived += networkedPlayer.OnWebSocketEventReceived;
                        EdgeMultiplayCallbacks.udpEventReceived += networkedPlayer.OnUDPEventReceived;
                        if (player.playerId == gameSession.playerId)
                        {
                            localPlayer = MessageSender = networkedPlayer;
                            localPlayerIsMaster = player.playerIndex == 0;
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    throw new Exception("EdgeMultiplay: Error in creating players, Make sure all your EdgeManager Spawn prefabs have a script that inherits from NetworkedPlayer ");
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new Exception("EdgeMultiplay: Error in creating players, Make sure Size of EdgeManager Spawn Info equal or greater than number of players in the room ");
                }
                catch (Exception)
                {
                    throw new Exception("EdgeMultiplay: Error in creating players");
                }
            }
        }

        #endregion

    }
}