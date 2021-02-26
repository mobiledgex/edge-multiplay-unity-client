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
using System.Collections;
using UnityEngine;
using static EdgeMultiplay.EdgeMultiplayObserver;


// fixme two problems
// Syncing non player objects like PingPongBall
// Creating synced object at run time (non player and player)


// Requirement for synced object across players:
// 1. Unique id consistent across all players.
// 2. who owns the GOs? (One with the best connection) - (One geographically closer to the server)

// Creating synced object at run time (non player and player)
// OnObservers list update add item to list

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

        private NetworkedPlayer networkedPlayer;

        #endregion

        #region EdgeMultiplayObserver Editor exposed variables
        
        public List<Observer> observers;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {

            EdgeManager.observers.Add(this);
            
            networkedPlayer = GetComponent<NetworkedPlayer>();
            UpdateObservers();

        }


        public void UpdateObservers()
        {
            for (int i = 0; i < observers.Count; i++)
            {
                observers[i].SetObserverId(i);
                if (observers[i].gameObject)
                {
                    observers[i].SetupObserver();
                }
                if (!networkedPlayer.isLocalPlayer)
                {
                    if (observers[i].gameObject.GetComponent<Rigidbody>())
                        GetComponent<Rigidbody>().isKinematic = true;
                    if (observers[i].gameObject.GetComponent<Rigidbody2D>())
                        GetComponent<Rigidbody2D>().isKinematic = true;
                }
            }
        }
        private void OnValidate()
        {
            if (observers.Count > 0)
            {
                for (int i = 0; i < observers.Count; i++)
                {
                    observers[i].SetObserverId(i);
                    if (observers[i].gameObject)
                    {
                        observers[i].SetupObserver();
                    }
                }
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
                       if (networkedPlayer && networkedPlayer.isLocalPlayer)
                       {
                           observer.SendDataToServer(true,networkedPlayer.playerId);
                       }
                    }
                }
            }
        }

        private void LateUpdate()
        {
            foreach (Observer observer in observers)
            {
               if (networkedPlayer && !networkedPlayer.isLocalPlayer)
               {
                   observer.ReflectDataFromServer();
               }
            }
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

        #endregion
    }

    [Serializable]
    public class ObserverList<Observer>:List<Observer>
    {
        //public List<Observer> observers;

         event EventHandler OnAdd;

        public new void Add(Observer item)
        {
            if (null != OnAdd)
            {
                OnAdd(this, null);
            }
            base.Add(item);
        }
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

        public Observer(GameObject gameObject, SyncOptions syncOption, bool interpolatePosition, bool interpolateRotation, float interpolationFactor, int observerId)
        {
            this.gameObject = gameObject;
            this.syncOption = syncOption;
            InterpolatePosition = interpolatePosition;
            InterpolateRotation = interpolateRotation;
            InterpolationFactor = interpolationFactor;
            this.observerId = observerId;
        }

        public void SetLocalPostion(Vector3 targetPosition)
        {
            var tempPosition = gameObject.transform.localPosition;
            tempPosition.Set(targetPosition.x, targetPosition.y, targetPosition.z);
            gameObject.transform.localPosition = tempPosition;
        }

        public void SetLocalRotation(Vector3 targetRotationEulers)
        {
            Quaternion tempRotation = gameObject.transform.localRotation;
            Quaternion rot = Quaternion.Euler(targetRotationEulers);
            tempRotation.Set(rot.x, rot.y, rot.z, rot.w);
            gameObject.transform.localRotation = tempRotation;
        }

        public void SetObserverId(int index)
        {
            observerId = index;
        }

        public void SetupObserver()
        {
            switch (syncOption)
            {
                case SyncOptions.SyncPosition:
                    lastPosition = gameObject.transform.position;
                    positionFromServer = gameObject.transform.position;
                    break;
                case SyncOptions.SyncRotation:
                    lastRotation = gameObject.transform.rotation.eulerAngles;
                    rotationFromServer = gameObject.transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncPositionAndRotation:
                    lastPosition = gameObject.transform.position;
                    positionFromServer = gameObject.transform.position;
                    lastRotation = gameObject.transform.rotation.eulerAngles;
                    rotationFromServer = gameObject.transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncLocalPosition:
                    lastPosition = gameObject.transform.localPosition;
                    positionFromServer = gameObject.transform.localPosition;
                    break;
                case SyncOptions.SyncLocalRotation:
                    lastRotation = gameObject.transform.localRotation.eulerAngles;
                    rotationFromServer = gameObject.transform.localRotation.eulerAngles;
                    break;
                case SyncOptions.SyncLocalPositionAndRotation:
                    lastPosition = gameObject.transform.localPosition;
                    positionFromServer = gameObject.transform.localPosition;
                    lastRotation = gameObject.transform.localRotation.eulerAngles;
                    rotationFromServer = gameObject.transform.localRotation.eulerAngles;
                    break;
            }
        }

        public void ReflectDataFromServer()
        {
            switch (syncOption)
            {
                case SyncOptions.SyncPosition:
                    if (InterpolatePosition)
                       gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, positionFromServer, Time.deltaTime * InterpolationFactor);
                    else
                        gameObject.transform.position = positionFromServer;
                    break;

                case SyncOptions.SyncRotation:
                    if (InterpolateRotation)
                        gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, Quaternion.Euler(rotationFromServer), Time.deltaTime * InterpolationFactor);
                    else
                        gameObject.transform.rotation = Quaternion.Euler(rotationFromServer);
                    break;

                case SyncOptions.SyncPositionAndRotation:
                    if (InterpolatePosition)
                        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, positionFromServer, Time.deltaTime * InterpolationFactor);
                    else
                        gameObject.transform.position = positionFromServer;

                    if (InterpolateRotation)
                        gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, Quaternion.Euler(rotationFromServer), Time.deltaTime * InterpolationFactor);
                    else
                        gameObject.transform.rotation = Quaternion.Euler(rotationFromServer);
                    break;
                case SyncOptions.SyncLocalPosition:
                    if (InterpolatePosition)
                        SetLocalPostion(Vector3.Lerp(gameObject.transform.position, positionFromServer, Time.deltaTime * InterpolationFactor));
                    else
                        SetLocalPostion(positionFromServer);
                    break;

                case SyncOptions.SyncLocalRotation:
                    if (InterpolateRotation)
                        SetLocalRotation(Vector3.Lerp(gameObject.transform.localRotation.eulerAngles, rotationFromServer, Time.deltaTime * InterpolationFactor));
                    else
                        SetLocalRotation(rotationFromServer);
                    break;

                case SyncOptions.SyncLocalPositionAndRotation:
                    if (InterpolatePosition)
                        SetLocalPostion(Vector3.Lerp(gameObject.transform.position, positionFromServer, Time.deltaTime * InterpolationFactor));
                    else
                        SetLocalPostion(positionFromServer);

                    if (InterpolateRotation)
                        SetLocalRotation(Vector3.Lerp(gameObject.transform.localRotation.eulerAngles, rotationFromServer, Time.deltaTime * InterpolationFactor));
                    else
                        SetLocalRotation(rotationFromServer);
                    break;
            }
        }

        public void SendDataToServer(bool attachedToPlayer, string playerId)
        {
            GamePlayEvent observerEvent = new GamePlayEvent();
            observerEvent.eventName = "EdgeMultiplayObserver";
            observerEvent.booleanData = new bool[1] { attachedToPlayer };
            observerEvent.stringData = new string[1] { playerId };
            observerEvent.integerData = new int[2] { (int)syncOption, observerId };
            switch (syncOption)
            {
                case SyncOptions.SyncPosition:
                    observerEvent.floatData = GetPositionData(gameObject.transform);
                    lastPosition = gameObject.transform.position;
                    break;
                case SyncOptions.SyncRotation:
                    observerEvent.floatData = GetRotationData(gameObject.transform);
                    lastRotation = gameObject.transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncPositionAndRotation:
                    observerEvent.floatData = GetPositionAndRotationData(gameObject.transform);
                    lastRotation = gameObject.transform.rotation.eulerAngles;
                    lastPosition = gameObject.transform.position;
                    break;
                case SyncOptions.SyncLocalPosition:
                    observerEvent.floatData = GetLocalPositionData(gameObject.transform);
                    lastPosition = gameObject.transform.localPosition;
                    break;
                case SyncOptions.SyncLocalRotation:
                    observerEvent.floatData = GetLocalRotationData(gameObject.transform);
                    lastRotation = gameObject.transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncLocalPositionAndRotation:
                    observerEvent.floatData = GetLocalPositionAndRotationData(gameObject.transform);
                    lastPosition = gameObject.transform.localRotation.eulerAngles;
                    lastRotation = gameObject.transform.localPosition;
                    break;
            }
            EdgeManager.SendUDPMessage(observerEvent);
        }

        public float[] GetPositionData(Transform transformComponent)
        {
            return new float[3] { transformComponent.position.x, transformComponent.position.y, transformComponent.position.z };
        }

        public float[] GetRotationData(Transform transformComponent)
        {
            return new float[3] { transformComponent.rotation.eulerAngles.x, transformComponent.rotation.eulerAngles.x, transformComponent.rotation.eulerAngles.x };
        }
        public float[] GetPositionAndRotationData(Transform transformComponent)
        {
            return new float[6] {transformComponent.position.x, transformComponent.position.y, transformComponent.position.z,
                transformComponent.rotation.eulerAngles.x, transformComponent.rotation.eulerAngles.x, transformComponent.rotation.eulerAngles.x };
        }

        public float[] GetLocalPositionData(Transform transformComponent)
        {
            return new float[3] { transformComponent.localPosition.x, transformComponent.localPosition.y, transformComponent.localPosition.z };
        }

        public float[] GetLocalRotationData(Transform transformComponent)
        {
            return new float[3] { transformComponent.localRotation.eulerAngles.x, transformComponent.localRotation.eulerAngles.y, transformComponent.localRotation.eulerAngles.z };
        }

        public float[] GetLocalPositionAndRotationData(Transform transformComponent)
        {
            return new float[6] {transformComponent.localPosition.x, transformComponent.localPosition.y, transformComponent.localPosition.z,
                transformComponent.localRotation.eulerAngles.x, transformComponent.localRotation.eulerAngles.y, transformComponent.localRotation.eulerAngles.z };
        }

        public float[] GetLocalScaleData(Transform transformComponent)
        {
            return new float[3] { transformComponent.localScale.x, transformComponent.localScale.y, transformComponent.localScale.z };
        }


    }

    public class NonPlayerObserver : MonoBehaviour
    {
        public enum Owner
        {
            FirstPlayerInRoom,
            ClosestPlayerToServer
        }
        public Owner owner;
        public SyncOptions syncOption;
        public Observer observer;

        private void Awake()
        {
          if(owner == Owner.FirstPlayerInRoom)
          {
             NetworkedPlayer observerOwner = EdgeManager.GetPlayer(0);
             EdgeMultiplayObserver edgeMultiplayObserver = observerOwner.GetComponent<EdgeMultiplayObserver>();
             if (!edgeMultiplayObserver)
             {
                    edgeMultiplayObserver = observerOwner.gameObject.AddComponent<EdgeMultiplayObserver>();
             }
             edgeMultiplayObserver.observers.Add(observer);
             edgeMultiplayObserver.UpdateObservers();
          }
        }

    }
}