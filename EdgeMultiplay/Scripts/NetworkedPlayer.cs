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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeMultiplay
{
    /// <summary>
    /// The player script should inherit from Networked Player
    /// NetworkedPlayer contains all the player data retrieved from the server
    /// </summary>
    [AddComponentMenu("EdgeMultiplay/NetworkedPlayer")]
    public class NetworkedPlayer : EdgeMultiplayCallbacks
    {
        #region NetworkedPlayer Variables

        /// <summary>
        /// the id of the room that the player is a member of
        /// </summary>
        public string roomId;
        /// <summary>
        /// the player order in the room, ex. the first player to join a room has playerIndex of 0
        /// </summary>
        public int playerIndex;
        /// <summary>
        /// The player Id that the server assigns to the player
        /// </summary>
        public string playerId;
        public string playerName;
        //public Player gamePlayer;
        public Action<GamePlayEvent> playerEvent;
        public bool isLocalPlayer;
        /// <summary>
        /// Can be used to set the player to be active or inactive although the player is still an active connection in the room
        /// </summary>
        public bool ActivePlayer = true;
        EdgeManager edgeManager;
        /// <summary>
        /// The observer responsible for tracking the observable transforms
        /// </summary>
        public EdgeMultiplayObserver observer
        {
            get
            {
                if (GetComponent<EdgeMultiplayObserver>())
                {
                    return GetComponent<EdgeMultiplayObserver>();
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        #endregion

        #region NetworkedPlayer Functions

        private void Awake()
        {
           edgeManager = FindObjectOfType<EdgeManager>();
        }

        /// <summary>
        /// Call ListenToMessages() to start listening to messages from the server
        /// </summary>
        public void ListenToMessages()
        {
            if (!isLocalPlayer)
            {
                playerEvent += OnMessageReceived;
            }
        }
        /// <summary>
        /// Add StopListening when you want to stop listening to messages from the server
        /// Recommended to have it under the Monobehaviour callback OnDestroy()
        /// </summary>
        public void StopListening()
        {
            if (!isLocalPlayer)
            {
                playerEvent -= OnMessageReceived;
            }
        }

        public override void OnWebSocketEventReceived(GamePlayEvent mobiledgexEvent)
        {
            if (!isLocalPlayer)
            {
                if (mobiledgexEvent.senderId == playerId)
                {
                    playerEvent(mobiledgexEvent);
                }
            }
        }

        public override void OnUDPEventReceived(GamePlayEvent udpEvent)
        { 
            if (!isLocalPlayer)
            {
                if (udpEvent.senderId == playerId)
                {
                   playerEvent(udpEvent);
                }
            }
        }

        /// <summary>
        /// Called once GamePlay Events Received from the server
        /// </summary>
        /// <param name="gamePlayEvent">the received gamePlayEvent</param>
        public virtual void OnMessageReceived(GamePlayEvent gamePlayEvent)
        {
            Debug.Log("GamePlayEvent Received");
        }

        public void SetUpPlayer(Player playerFromServer, string roomId, bool isLocalPlayer =false)
        {
            this.roomId = roomId;
            playerName = playerFromServer.playerName;
            playerIndex = playerFromServer.playerIndex;
            playerId = playerFromServer.playerId;
            this.isLocalPlayer = isLocalPlayer;
            ActivePlayer = true;
        }
        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(GamePlayEvent gamePlayEvent)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent()
            {
                eventName = gamePlayEvent.eventName,
                booleanData = gamePlayEvent.booleanData,
                stringData = gamePlayEvent.stringData,
                integerData = gamePlayEvent.integerData,
                floatData = gamePlayEvent.floatData,
            };
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, Vector3 position)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, position);
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, Quaternion rotation)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, rotation);
            edgeManager.SendGamePlayEvent(gameplayEvent) ;
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, Vector3 position, Quaternion rotation)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, position, rotation);
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, List<int> integerArray)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, integerArray);
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, List<float> floatArray)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, floatArray);
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, string[] stringData = null , int[] commandInts = null ,
            float[] floatData = null,
            bool[] booleanData = null)
        {
            GamePlayEvent gamePlayEvent = new GamePlayEvent(this.roomId, playerId, eventName, stringData, commandInts, floatData, booleanData);
            edgeManager.SendGamePlayEvent(gamePlayEvent);
        }

        /// <summary>
        /// Creates synced GameObject in runtime in the local player's world
        /// and sends an event to all players in the room to create the observable in their worlds
        /// </summary>
        /// <param name="prefabName">The name of your prefab (Game Object) stored in Resources Folder without extensions ex. 'Ball' </param>
        /// <param name="startPosition"> the inital position of the spawning </param>
        /// <param name="startRotation">  the inital rotation of the spawning </param>
        /// <param name="syncOption"> Which Synchronization option will be applied </param>
        /// <param name="interpolatePosition"> Set to true if you want to smoothen the tracked position if you have network lag </param>
        /// <param name="interpolateRotation"> Set to true if you want to smoothen the tracked rotation if you have network lag</param>
        /// <param name="interpolationFactor"> Set Interpolation factor between 0.1 and 1 </param>
        /// <returns> The created Observer object </returns>
        public Observable CreateObservableObject(string prefabName, Vector3 startPosition, Quaternion startRotation, SyncOptions syncOption, bool interpolatePosition = false, bool interpolateRotation = false, float interpolationFactor = 0)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            GameObject syncedObject;
            syncedObject = Instantiate(prefab, startPosition, startRotation);
            if (!observer)
            {
                observer =  gameObject.AddComponent<EdgeMultiplayObserver>();
            }
            Observable newObservable = new Observable(syncedObject.transform, syncOption, interpolatePosition, interpolateRotation, Mathf.Clamp(interpolationFactor, 0.1f, 1f), observer.observables.Count);
            observer.observables.Add(newObservable);
            newObservable.SetupObservable(this);
            observer.UpdateObservables();
            if (isLocalPlayer)
            {
                GamePlayEvent newObserverEvent = new GamePlayEvent
                {
                    eventName = "NewObservableCreated",
                    booleanData = new bool[2] { interpolatePosition, interpolateRotation },
                    stringData = new string[2] { playerId, prefabName },
                    integerData = new int[1] { (int)syncOption },
                    floatData = new float[7] { startPosition.x, startPosition.y, startPosition.z, startRotation.eulerAngles.x, startRotation.eulerAngles.y, startRotation.eulerAngles.z, interpolationFactor },
                };
                edgeManager.SendGamePlayEvent(newObserverEvent);
            }
            return newObservable;
        }

        #endregion
    }
}
