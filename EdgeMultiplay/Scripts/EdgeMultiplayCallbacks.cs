/**
 *  Copyright 2018-2021 MobiledgeX, Inc. All rights and licenses reserved.
 *  MobiledgeX, Inc. 156 2nd Street #408, San Francisco, CA 94105
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
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
        public static Action leftRoom;
        public static Action gameStart;
        public static Action gameEnd;

        public void ConnectToEdge(bool testingMode = false,bool useFallBackLocation = false)
        {
            connectedToEdge += OnConnectionToEdge;
            failureToConnect += OnFaliureToConnect;
            registerEvent += OnRegisterEvent;
            notificationEvent += OnNotificationEvent;
            roomsList += OnRoomsListReceived;
            newRoomCreatedInLobby += OnNewRoomCreatedInLobby;
            roomCreated += OnRoomCreated;
            roomJoin += OnRoomJoin;
            playerRoomJoined += PlayerJoinedRoom;
            joinRoomFaliure += OnJoinRoomFailed;
            gameStart += OnGameStart;
            playerLeft += OnPlayerLeft;
            leftRoom += OnLeftRoom;
            eventReceived += OnWebSocketEventReceived;
            udpEventReceived += OnUDPEventReceived;
            StartCoroutine(ConnectToEdgeCoroutine( testingMode, useFallBackLocation));
        }

        IEnumerator ConnectToEdgeCoroutine(bool testingMode = false, bool useFallBackLocation = false)
        {
            EdgeManager edgeManager = FindObjectOfType<EdgeManager>();
            if (edgeManager.useLocalHostServer == false && (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android))
            {
                yield return StartCoroutine(MobiledgeX.LocationService.EnsureLocation());
            }
            else
            {
                yield return null;
            }
            edgeManager.ConnectToServer(testingMode, useFallBackLocation);
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
            Debug.Log("Notification Event From Server :"+ notification.notificationText);
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
        /// </summary>
        /// <param name="rooms"> Updated List of the rooms on the server </param>
        public virtual void OnNewRoomCreatedInLobby ()
        {
            Debug.Log("New Room Created On Server Event From Server, Call EdgeManager.GetRooms() to update rooms");
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
        /// </summary>
        /// <param name="RoomMemberLeft">Info about the player who left the room </param>
        public virtual void OnPlayerLeft(RoomMemberLeft playerLeft)
        {
            Debug.Log("Player Left Event From Server");
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
            Debug.Log("WebSocket Event Received Event From Server : " + gamePlayEvent.eventName);
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
            roomCreated -= OnRoomCreated;
            roomJoin -= OnRoomJoin;
            playerRoomJoined -= PlayerJoinedRoom;
            joinRoomFaliure -= OnJoinRoomFailed;
            gameStart -= OnGameStart;
            playerLeft -= OnPlayerLeft;
            eventReceived -= OnWebSocketEventReceived;
            udpEventReceived -= OnUDPEventReceived;
        }
    }
}
