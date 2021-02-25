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
using static EdgeMultiplay.EdgeMultiplayObserver;

namespace EdgeMultiplay
{
    /// <summary>
    /// EdgeMultiplayObserver can be added to on an object to sync its position and/or rotation between all players
    /// You can sync a PlayerObject or Non PlayerObject
    /// </summary>
    [AddComponentMenu("EdgeMultiplay/EdgeMultiplayObserver")]
    public class EdgeMultiplayObserver : MonoBehaviour
    {
        #region EdgeMultiplayObserver Variables

        [HideInInspector]
        public string eventId;
        private bool isLocalPlayerMaster;
        private NetworkedPlayer networkedPlayer;

        #endregion

        #region EdgeMultiplayObserver Editor exposed variables

        /// <summary>
        /// Set to true if the Component is attached a player that have a scripts that inherits from NetwokedPlayer
        /// </summary>
        [Tooltip("Check if the Observer is attached to a player object, otherwise leave unchecked.")]
        public bool attachedToPlayer;

        /// <summary>
        /// List of GameObjects that you want to sync its position and/or rotation
        /// </summary>
        [Tooltip("GameObjects you want to sync its position and/or rotation")]
        public List<Observer> observers;

        public enum SyncOptions
        {
            SyncPosition,
            SyncRotation,
            SyncPositionAndRotation
        }

        #endregion

        #region MonoBehaviour Callbacks

#if UNITY_EDITOR

        //Reset is a Monobehaviour callback and is called once a Component is added to a GameObject
        [ExecuteAlways]
        void Reset()
        {
            eventId = Guid.NewGuid().ToString("N") + gameObject.GetInstanceID().ToString();
        }

#endif
        private void Awake()
        {
            if (!attachedToPlayer)
            {
                isLocalPlayerMaster = EdgeManager.localPlayerIsMaster;
            }
            else
            {
                isLocalPlayerMaster = false;
            }
            EdgeManager.observers.Add(this);
            foreach(Observer observer in observers)
            {
                observer.positionFromServer = observer.gameObject.transform.position;
                observer.rotationFromServer = observer.gameObject.transform.rotation.eulerAngles;
            }
            networkedPlayer = GetComponent<NetworkedPlayer>();
        }

        private void OnValidate()
        {
            for(int i =0; i< observers.Count; i++)
            {
                observers[i].observerId = i;
            }
        }

        private void Update()
        {
            if (EdgeManager.gameStarted)
            {
                foreach (Observer observer in observers)
                {
                    if (RequiresUpdate(observer))
                    {
                        if (attachedToPlayer)
                        {
                            if (networkedPlayer && networkedPlayer.isLocalPlayer)
                            {
                                SendDataToServer(observer, true, networkedPlayer.playerId);
                            }
                        }
                        else if (!attachedToPlayer && isLocalPlayerMaster)
                        {
                            SendDataToServer(observer, false, eventId);
                        }
                    }
                }
            }
        }

        private void LateUpdate()
        {
            foreach (Observer observer in observers)
            {
                if (attachedToPlayer)
                {
                    if (networkedPlayer && !networkedPlayer.isLocalPlayer)
                    {
                        ReflectServerData(observer);
                    }
                }
                else if (!attachedToPlayer && !isLocalPlayerMaster)
                {
                    ReflectServerData(observer);
                }
            }
        }

        private void OnDestroy()
        {
            eventId = null;
        }

        #endregion

        #region EdgeMultiplayObserver Private Functions

        private bool RequiresUpdate(Observer observer)
        {
            switch (observer.syncOption)
            {
                case SyncOptions.SyncPosition:
                    if (observer.lastPosition != observer.gameObject.transform.position)
                        return true;
                    else
                        return false;
                case SyncOptions.SyncRotation:
                    if (observer.lastRotation != observer.gameObject.transform.rotation.eulerAngles)
                        return true;
                    else
                        return false;
                default:
                case SyncOptions.SyncPositionAndRotation:
                    if (observer.lastPosition != observer.gameObject.transform.position || observer.lastRotation != observer.gameObject.transform.rotation.eulerAngles)
                        return true;
                    else
                        return false;
            }
        }

        void ReflectServerData(Observer obs)
        {
            switch (obs.syncOption)
            {
                case SyncOptions.SyncPosition:
                    if (obs.InterpolatePosition)
                        obs.gameObject.transform.position = Vector3.Lerp(obs.gameObject.transform.position, obs.positionFromServer, Time.deltaTime * obs.InterpolationFactor);
                    else
                        obs.gameObject.transform.position = obs.positionFromServer;
                    break;

                case SyncOptions.SyncRotation:
                    if (obs.InterpolateRotation)
                        obs.gameObject.transform.rotation = Quaternion.Lerp(obs.gameObject.transform.rotation, Quaternion.Euler(obs.rotationFromServer), Time.deltaTime * obs.InterpolationFactor);
                    else
                        obs.gameObject.transform.rotation = Quaternion.Euler(obs.rotationFromServer);
                    break;

                case SyncOptions.SyncPositionAndRotation:
                    if (obs.InterpolatePosition)
                        obs.gameObject.transform.position = Vector3.Lerp(obs.gameObject.transform.position, obs.positionFromServer, Time.deltaTime * obs.InterpolationFactor);
                    else
                        obs.gameObject.transform.position = obs.positionFromServer;

                    if (obs.InterpolateRotation)
                        obs.gameObject.transform.rotation = Quaternion.Lerp(obs.gameObject.transform.rotation, Quaternion.Euler(obs.rotationFromServer), Time.deltaTime * obs.InterpolationFactor);
                    else
                        obs.gameObject.transform.rotation = Quaternion.Euler(obs.rotationFromServer);
                    break;
            }
        }

        void SendDataToServer(Observer observer, bool attachedToPlayer, string playerId)
        {
            GamePlayEvent observerEvent = new GamePlayEvent();
            observerEvent.eventName = "EdgeMultiplayObserver";
            observerEvent.booleanData = new bool[1] { attachedToPlayer };
            observerEvent.stringData = new string[1] { playerId };
            observerEvent.integerData = new int[2] { (int)observer.syncOption, observer.observerId };
            switch (observer.syncOption)
            {
                case SyncOptions.SyncPosition:
                    observerEvent.floatData = new float[3] { observer.gameObject.transform.position.x, observer.gameObject.transform.position.y, observer.gameObject.transform.position.z };
                    observer.lastPosition = observer.gameObject.transform.position;
                    break;
                case SyncOptions.SyncRotation:
                    observerEvent.floatData = new float[3] { observer.gameObject.transform.rotation.eulerAngles.x, observer.gameObject.transform.rotation.eulerAngles.y, observer.gameObject.transform.rotation.eulerAngles.z };
                    observer.lastRotation = observer.gameObject.transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncPositionAndRotation:
                    observerEvent.floatData = new float[6] { observer.gameObject.transform.position.x, observer.gameObject.transform.position.y, observer.gameObject.transform.position.z
                        , observer.gameObject.transform.rotation.eulerAngles.x, observer.gameObject.transform.rotation.eulerAngles.y, observer.gameObject.transform.rotation.eulerAngles.z };
                    observer.lastRotation = observer.gameObject.transform.rotation.eulerAngles;
                    observer.lastPosition = observer.gameObject.transform.position;
                    break;
            }
            EdgeManager.SendUDPMessage(observerEvent);
        }

        #endregion
    }

    [Serializable]
    public class Observer
    {
        /// <summary>
        /// The GameObject you want to Sync its position and/or rotation
        /// </summary>
        public GameObject gameObject;
        public SyncOptions syncOption;
        /// <summary>
        /// Set to true if you want to smoothen the tracked position if you have network lag
        /// </summary>
        [Tooltip("Check if you want to smoothen the tracked position if you have network lag")]
        public bool InterpolatePosition;
        /// <summary>
        /// Set to true if you want to smoothen the tracked rotation if you have network lag
        /// </summary>
        [Tooltip("Check if you want to smoothen the tracked rotation if you have network lag")]
        public bool InterpolateRotation;
        /// <summary>
        /// Set Interpolation factor between 0.1 and 1
        /// </summary>
        [Tooltip("Set Interpolation factor between 0.1 and 1")]
        [Range(0.1f, 1f)]
        public float InterpolationFactor;
        [HideInInspector]
        public int observerId;
        [HideInInspector]
        public Vector3 lastPosition;
        [HideInInspector]
        public Vector3 lastRotation;
        [HideInInspector]
        public Vector3 positionFromServer;
        [HideInInspector]
        public Vector3 rotationFromServer;

    }
}