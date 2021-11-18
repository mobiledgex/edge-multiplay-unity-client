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

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EdgeMultiplay
{
  public class EdgeMultiplayCallbacks : MonoBehaviour
  {
    public static Action connectedToEdge;
    public static Action<string> failureToConnect;
    public static Action registerEvent;
    public static Action<Notification> notificationEvent;
    public static Action<GamePlayEvent> eventReceived;
    public static Action<GamePlayEvent> udpEventReceived;
    public static Action<RoomMemberLeft> playerLeft;
    public static Action<List<Room>> roomsList;
    public static Action<Room> roomCreated;
    public static Action<Room> roomJoin;
    public static Action<Room> playerRoomJoined;
    public static Action joinRoomFaliure;
    public static Action newRoomCreatedInLobby;
    public static Action roomRemovedFromLobby;
    public static Action roomsUpdated;
    public static Action leftRoom;
    public static Action gameStart;
    public static Action gameEnd;
    public static Action<Observable> newObservableCreated;

    /// <summary>
    /// Starts the connection to your Edge server, server discovery is based on GPS location and the telecommunication carrier
    /// </summary>
    /// <param name="useAnyCarrierNetwork">True by default, set to false to connect to a specific carrier, set carrier name using EdgeManager.integration.carrierName </param>
    /// <param name="useFallBackLocation">False by default, location is acquired from user GPS location, if you are using location blind device like Oculus, use EdgeManager.integration.SetFallbackLocation()</param>
    /// <param name="path"> You can specify a path for your connection to be verified on the server side </param>
    public void ConnectToEdge(bool useAnyCarrierNetwork = true, bool useFallBackLocation = false, string path = "")
    {
      connectedToEdge += OnConnectionToEdge;
      failureToConnect += OnFaliureToConnect;
      registerEvent += OnRegisterEvent;
      notificationEvent += OnNotificationEvent;
      roomsList += OnRoomsListReceived;
      newRoomCreatedInLobby += OnNewRoomCreatedInLobby;
      roomRemovedFromLobby += OnRoomRemovedFromLobby;
      roomCreated += OnRoomCreated;
      roomJoin += OnRoomJoin;
      playerRoomJoined += PlayerJoinedRoom;
      joinRoomFaliure += OnJoinRoomFailed;
      gameStart += OnGameStart;
      roomsUpdated += OnRoomsUpdated;
      playerLeft += OnPlayerLeft;
      leftRoom += OnLeftRoom;
      eventReceived += OnWebSocketEventReceived;
      udpEventReceived += OnUDPEventReceived;
      newObservableCreated += OnNewObservableCreated;
      StartCoroutine(ConnectToEdgeCoroutine(useAnyCarrierNetwork, useFallBackLocation, path));
    }

    IEnumerator ConnectToEdgeCoroutine(bool useAnyCarrierNetwork = true, bool useFallBackLocation = false, string path = "")
    {
      EdgeManager edgeManager = FindObjectOfType<EdgeManager>();
      if (Configs.clientSettings.useLocalHostServer == false)
      {
        if (SystemInfo.supportsLocationService)
        {
          yield return StartCoroutine(MobiledgeX.LocationService.EnsureLocation());
        }
        else
        {
          useFallBackLocation = true;
          yield return null;
        }
      }
      edgeManager.ConnectToServer(useAnyCarrierNetwork, useFallBackLocation, path).ConfigureAwait(false);
    }

    /// <summary>
    /// Called once the ConnectToEdge Request succeed 
    /// </summary>
    public virtual void OnConnectionToEdge()
    {
      Debug.Log("Connected to Edge");
    }

    /// <summary>
    /// Called once the ConnectToEdge Request fails
    /// </summary>
    /// <param name="reason"> Reason of connection faliure </param>
    public virtual void OnFaliureToConnect(string reason)
    {
      Debug.Log("Edge Connection Falied because " + reason);
    }

    /// <summary>
    /// Called once the server assigned a playerId to the user right after the connection is established
    /// </summary>
    public virtual void OnRegisterEvent()
    {
      Debug.Log("Register Event From Server");
    }

    /// <summary>
    /// Called once a notification is received from the server
    /// </summary>
    /// <param name="notification"> the notification received from the server</param>
    public virtual void OnNotificationEvent(Notification notification)
    {
      Debug.Log("Notification Event From Server :" + notification.notificationText);
    }

    /// <summary>
    /// The response for EdgeManager.GetRooms()
    /// returns list of rooms on the server
    /// </summary>
    /// <param name="rooms">List of the rooms on the server </param>
    public virtual void OnRoomsListReceived(List<Room> rooms)
    {
      Debug.Log("Rooms List Received from the server");
    }

    /// <summary>
    /// Called automatically once a new room is created on the server
    /// You can use this callback to call EdgeManager.GetRooms() to get the updated list of the rooms in the lobby
    /// </summary>
    public virtual void OnNewRoomCreatedInLobby()
    {
      Debug.Log("New Room Created In the Lobby, Call EdgeManager.GetRooms() to get the updated rooms list");
    }

    /// <summary>
    /// Called automatically once a room is removed from the lobby
    /// You can use this callback to call EdgeManager.GetRooms() to get the updated list of the rooms in the lobby
    /// </summary>
    public virtual void OnRoomRemovedFromLobby()
    {
      Debug.Log("A Room have been removed from the Lobby, Call EdgeManager.GetRooms() to get the updated rooms list");
    }

    /// <summary>
    /// Called once the local player creates a room,
    /// The response of EdgeManagre.CreateRoom() and might be the response of EdgeManager.JoinOrCreateRoom() in case of room creation
    /// </summary>
    /// <param name="room">Created Room Info</param>
    public virtual void OnRoomCreated(Room room)
    {
      Debug.Log("Room Created Event From Server");
    }

    /// <summary>
    /// Called once the local player joins a room,
    /// The response of EdgeManagre.JoinRoom() and might be the response of EdgeManager.JoinOrCreateRoom() in case of room join
    /// </summary>
    /// <param name="room">Joined Room Info</param>
    public virtual void OnRoomJoin(Room room)
    {
      Debug.Log("Room Join Event From Server");
    }

    /// <summary>
    /// Called automatically once a player joins the local player room 
    /// </summary>
    /// <param name="room">Updated Room Info</param>
    public virtual void PlayerJoinedRoom(Room room)
    {
      Debug.Log("PlayerJoinedRoom Event From Server");
    }

    /// <summary>
    /// Called once a JoinRoom Request fails on the server
    /// </summary>
    public virtual void OnJoinRoomFailed()
    {
      Debug.Log("Join Room Failed Event From Server");
    }

    /// <summary>
    /// Called once the game starts,
    /// the game starts on the server once the number of players == the maximum players per room
    /// </summary>
    public virtual void OnGameStart()
    {
      Debug.Log("Game Start Event From Server");
    }

    /// <summary>
    /// Called once a player in the same room as the local player leaves the room
    /// <para>If the player who left was tracking any transforms
    /// this callback will be where you should transfer observables ownership to another player if the game is still running.</para>
    /// </summary>
    /// <param name="RoomMemberLeft">Info about the player who left the room </param>
    public virtual void OnPlayerLeft(RoomMemberLeft playerLeft)
    {
      Debug.Log("Player Left Event From Server");
    }

    /// <summary>
    /// Called once a player joins or leaves a room in the lobby
    /// this callback can be useful to update rooms status, Call EdgeManager.GetRooms() to get the latest rooms updates
    /// </summary>
    public virtual void OnRoomsUpdated()
    {
      Debug.Log("RoomsUpdate Event From Server");
    }

    /// <summary>
    /// Called once ExitRoom request in(EdgeManager.ExitRoom()) succeed
    /// </summary>
    public virtual void OnLeftRoom()
    {
      Debug.Log("Left Room Event From Server");
    }

    /// <summary>
    /// During GamePlay once a event received from another player (Websocket)
    /// </summary>
    /// <param name="gamePlayEvent">received GamePlayEvent</param>
    public virtual void OnWebSocketEventReceived(GamePlayEvent gamePlayEvent)
    {
      Debug.Log("WebSocket Event Received From Server : " + gamePlayEvent.eventName);
    }

    /// <summary>
    /// Server Callback when a new Observable is created by one of the players
    /// </summary>
    /// <param name="observable"> The created Observable object </param>
    public virtual void OnNewObservableCreated(Observable observable)
    {
      Debug.Log("New Observable created, owner name : " + observable.owner.playerName);
    }

    /// <summary>
    /// During GamePlay once a event received from another player (UDP)
    /// </summary>
    /// <param name="gamePlayEvent">received GamePlayEvent</param>
    public virtual void OnUDPEventReceived(GamePlayEvent gamePlayEvent)
    {
      //Debug.Log("UDP Msg Received Event From Server : " + gamePlayEvent.eventName);
    }

    private void OnDestroy()
    {
      connectedToEdge -= OnConnectionToEdge;
      failureToConnect -= OnFaliureToConnect;
      registerEvent -= OnRegisterEvent;
      notificationEvent -= OnNotificationEvent;
      leftRoom += OnLeftRoom;
      roomsList -= OnRoomsListReceived;
      newRoomCreatedInLobby -= OnNewRoomCreatedInLobby;
      roomRemovedFromLobby -= OnRoomRemovedFromLobby;
      roomCreated -= OnRoomCreated;
      roomJoin -= OnRoomJoin;
      playerRoomJoined -= PlayerJoinedRoom;
      joinRoomFaliure -= OnJoinRoomFailed;
      gameStart -= OnGameStart;
      playerLeft -= OnPlayerLeft;
      roomsUpdated -= OnRoomsUpdated;
      eventReceived -= OnWebSocketEventReceived;
      udpEventReceived -= OnUDPEventReceived;
      newObservableCreated -= OnNewObservableCreated;
    }
  }
}
