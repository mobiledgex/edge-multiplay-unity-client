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
using UnityEngine;

namespace EdgeMultiplay
{
    /// <summary>
    /// Use for syncing GameObjects local position and/or local rotation
    /// </summary>
    public class EdgeMultiplayObserverLocal : EdgeMultiplayObserver
    {
        #region EdgeMultiplayObserverLocal Variables

        private Vector3 latestPosition;
        private Vector3 latestRotation;
        private bool LocalPlayeIsMaster;
        private NetworkedPlayer networkPlayer;
        private Vector3 targetPosInterpolated;
        private Vector3 targetRotInterpolated;

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
        private void Start()
        {
            if (!attachedToPlayer)
            {
                LocalPlayeIsMaster = EdgeManager.localPlayerIsMaster;
            }
            EdgeManager.observers.Add(this);
            networkPlayer = GetComponent<NetworkedPlayer>();
            positionFromServer = transform.localPosition;
            rotationFromServer = transform.localRotation.eulerAngles;
        }

        private void Update() 
        {
            if (EdgeManager.gameStarted)
            {
                if ((latestPosition != transform.localPosition && syncOption == SyncOptions.SyncPosition)
                || (latestRotation != transform.localRotation.eulerAngles && syncOption == SyncOptions.SyncRotation)
                || ((latestRotation != transform.localRotation.eulerAngles || latestPosition != transform.localPosition) && syncOption == SyncOptions.SyncPositionAndRotation))
                {
                    if (attachedToPlayer)
                    {
                        if (networkPlayer && networkPlayer.isLocalPlayer)
                        {
                            SendDataToServer(syncOption, true, networkPlayer.playerId);
                        }
                    }
                    else if (!attachedToPlayer && LocalPlayeIsMaster)
                    {
                        SendDataToServer(syncOption, false, eventId);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (attachedToPlayer)
            {
                if (networkPlayer && !networkPlayer.isLocalPlayer)
                {
                    ReflectServerData(syncOption);
                }
            }
            else if (!attachedToPlayer && !LocalPlayeIsMaster)
            {
                ReflectServerData(syncOption);
            }
        }

        private void OnDestroy()
        {
            eventId = null;
        }

        #endregion

        #region EdgeMultiplayObserver Private Functions
      
        void ReflectServerData(SyncOptions syncOption)
        {
            switch (syncOption)
            {
                case SyncOptions.SyncPosition:
                    if (InterpolatePosition)
                    {
                        targetPosInterpolated = Vector3.Lerp(transform.localPosition, positionFromServer, Time.deltaTime * InterpolationFactor);
                        SetLocalPostion(targetPosInterpolated);
                    }
                    else
                    {
                        SetLocalPostion(positionFromServer);
                    }   
                    break;

                case SyncOptions.SyncRotation:
                    if (InterpolateRotation)
                    {
                        targetRotInterpolated = Vector3.Lerp(transform.localRotation.eulerAngles, rotationFromServer, Time.deltaTime * InterpolationFactor);
                        SetLocalRotation(targetRotInterpolated);
                    }
                    else
                    {
                        SetLocalRotation(rotationFromServer);
                    }  
                    break;

                case SyncOptions.SyncPositionAndRotation:
                    if (InterpolatePosition)
                    {
                        targetPosInterpolated = Vector3.Lerp(transform.localPosition, positionFromServer, Time.deltaTime * InterpolationFactor);
                        SetLocalPostion(targetPosInterpolated);
                    }
                    else
                    {
                        SetLocalPostion(positionFromServer);
                    }
                        
                    if (InterpolateRotation)
                    {
                        targetRotInterpolated = Vector3.Lerp(transform.localRotation.eulerAngles, rotationFromServer, Time.deltaTime * InterpolationFactor);
                        SetLocalRotation(targetRotInterpolated);
                    }
                    else
                    {
                        SetLocalRotation(rotationFromServer);
                    }
                    break;
            }
        }

        void SendDataToServer(SyncOptions syncOption, bool attachedToPlayer, string observerId)
        {
            GamePlayEvent observerEvent = new GamePlayEvent();
            observerEvent.eventName = "EdgeMultiplayObserver";
            observerEvent.booleanData = new bool[1] { attachedToPlayer };
            observerEvent.stringData = new string[1] { observerId };
            observerEvent.integerData = new int[1] { (int)syncOption };
            switch (syncOption)
            {
                case SyncOptions.SyncPosition:
                    observerEvent.floatData = new float[3] { transform.localPosition.x, transform.localPosition.y, transform.localPosition.z };
                    latestPosition = transform.localPosition;
                    break;
                case SyncOptions.SyncRotation:
                    observerEvent.floatData = new float[3] { transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z };
                    latestRotation = transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncPositionAndRotation:
                    observerEvent.floatData = new float[6] { transform.localPosition.x, transform.localPosition.y, transform.localPosition.z
                        , transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z };
                    latestRotation = transform.localRotation.eulerAngles;
                    latestPosition = transform.localPosition;
                    break;
            }
            EdgeManager.SendUDPMessage(observerEvent);
        }


        void SetLocalPostion(Vector3 targetPosition)
        {
            var tempPosition = transform.localPosition;
            tempPosition.Set(targetPosition.x, targetPosition.y, targetPosition.z);
            transform.localPosition = tempPosition;
        }

        void SetLocalRotation(Vector3 targetRotationEulers)
        {
            Quaternion tempRotation = transform.localRotation;
            Quaternion rot = Quaternion.Euler(targetRotationEulers);
            tempRotation.Set(rot.x, rot.y, rot.z, rot.w);
            transform.localRotation = tempRotation;
        }

        #endregion
    }
}