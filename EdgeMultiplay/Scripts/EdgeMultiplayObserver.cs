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
    /// EdgeMultiplayObserver can be added to on an object to sync its position and/or rotation between all players
    /// You can sync a PlayerObject or Non PlayerObject
    /// </summary>
    [AddComponentMenu("EdgeMultiplay/EdgeMultiplayObserver")]
    public class EdgeMultiplayObserver : MonoBehaviour
    {
        #region EdgeMultiplayObserver Variables
        ///
        [HideInInspector]
        public NetworkedPlayer networkedPlayer;

        /// <summary>
        /// List of observed objects
        /// </summary>
        [Tooltip("Add all sync Transforms including the player transform")]
        public List<Observerable> observerables;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            EdgeManager.observers.Add(this);
            networkedPlayer = GetComponent<NetworkedPlayer>();
            UpdateObserverables();
        }

        private void OnValidate()
        {
            if (!GetComponent<NetworkedPlayer>())
            {
                Debug.LogError("EdgeMultiplayObserver requires PlayerManager or a class that inherits from NetworkedPlayer," +
                    "\nRemove EdgeMultiplayObserver Component from "+gameObject.name);
            }
            if (observerables.Count > 0)
            {
                for (int i = 0; i < observerables.Count; i++)
                {
                    observerables[i].SetObserverableIndex(i);
                    if (observerables[i].observeredTransform)
                    {
                        observerables[i].SetupObserverable(networkedPlayer);
                    }
                }
            }
        }

        private void Update()
        {
            if (EdgeManager.gameStarted)
            {
                foreach (Observerable observerable in observerables)
                {
                    if (RequiresUpdate(observerable))
                    {
                       if (networkedPlayer && networkedPlayer.isLocalPlayer)
                       {
                           observerable.SendDataToServer();
                       }
                    }
                }
            }
        }

        #endregion

        #region EdgeMultiplayObserver Functions
        /// <summary>
        ///UpdateObservers does the following:
        /// <para>1.Update the observerables list indices.</para>
        /// <para>2.Updates/Assigns an Obserview View to the observed game objects.</para>
        /// <para>3.Disables the rigid body 2d or 3d on the observed game objects, so the physics can be simulated from the owner only.</para>
        /// <para>You should call UpdateObserverables() at these situations:</para>
        /// <para>1.New observerable added.</para>
        /// <para>2.Observerable ownership change.</para>
        /// </summary>
        public void UpdateObserverables()
        {
            for (int i = 0; i < observerables.Count; i++)
            {
                ObserverableView observerView;
                observerables[i].SetObserverableIndex(i);
                if (observerables[i].observeredTransform.gameObject.GetComponent<ObserverableView>())
                {
                    observerView = observerables[i].observeredTransform.gameObject.GetComponent<ObserverableView>();
                }
                else
                {
                    observerView = observerables[i].observeredTransform.gameObject.AddComponent<ObserverableView>();
                }
                observerView.SetupObserverView(networkedPlayer.playerId, i);
              
                if (observerables[i].observeredTransform != null)
                {
                    observerables[i].SetupObserverable(networkedPlayer);
                }
                if (networkedPlayer && !networkedPlayer.isLocalPlayer && observerables[i].owner == null)
                {
                    if (observerables[i].observeredTransform && observerables[i].observeredTransform.GetComponent<Rigidbody>())
                    {
                        observerables[i].observeredTransform.GetComponent<Rigidbody>().isKinematic = true;
                    }
                    if (observerables[i].observeredTransform && observerables[i].observeredTransform.GetComponent<Rigidbody2D>())
                    {
                        observerables[i].observeredTransform.GetComponent<Rigidbody2D>().isKinematic = true;
                    }
                }
                else
                {
                    if (observerables[i].owner != null && !observerables[i].owner.isLocalPlayer && observerables[i].attachedToPlayer)
                    {
                        if (observerables[i].observeredTransform.GetComponent<Rigidbody>())
                        {
                            observerables[i].observeredTransform.GetComponent<Rigidbody>().isKinematic = true;
                        }
                        if (observerables[i].observeredTransform.GetComponent<Rigidbody2D>())
                        {
                            observerables[i].observeredTransform.GetComponent<Rigidbody2D>().isKinematic = true;
                        }
                    }
                    else
                    {
                        if (observerables[i].owner != null && observerables[i].owner.isLocalPlayer && observerables[i].attachedToPlayer)
                        {
                            if (observerables[i].observeredTransform.GetComponent<Rigidbody>())
                            {
                                observerables[i].observeredTransform.GetComponent<Rigidbody>().isKinematic = false;
                            }
                            if (observerables[i].observeredTransform.GetComponent<Rigidbody2D>())
                            {
                                observerables[i].observeredTransform.GetComponent<Rigidbody2D>().isKinematic = false;
                            }
                        }
                    }
                }
            }
        }

        private bool RequiresUpdate(Observerable observerable)
        {
            switch (observerable.syncOption)
            {
                case SyncOptions.SyncPosition:
                    if (observerable.lastPosition != observerable.observeredTransform.position)
                        return true;
                    else
                        return false;
                case SyncOptions.SyncRotation:
                    if (observerable.lastRotation != observerable.observeredTransform.rotation.eulerAngles)
                        return true;
                    else
                        return false;
                default:
                case SyncOptions.SyncPositionAndRotation:
                    if (observerable.lastPosition != observerable.observeredTransform.position || observerable.lastRotation != observerable.observeredTransform.rotation.eulerAngles)
                        return true;
                    else
                        return false;
            }
        }

        #endregion
    }
}
