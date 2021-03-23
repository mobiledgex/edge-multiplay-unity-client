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
        public List<Observable> observables;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            EdgeManager.observers.Add(this);
            networkedPlayer = GetComponent<NetworkedPlayer>();
            UpdateObservables();
        }

        private void OnValidate()
        {
            if (!GetComponent<NetworkedPlayer>())
            {
                Debug.LogError("EdgeMultiplayObserver requires PlayerManager or a class that inherits from NetworkedPlayer," +
                    "\nRemove EdgeMultiplayObserver Component from "+gameObject.name);
            }
            if (observables.Count > 0)
            {
                for (int i = 0; i < observables.Count; i++)
                {
                    observables[i].SetObservableIndex(i);
                    if (observables[i].observeredTransform)
                    {
                        observables[i].SetupObservable(networkedPlayer);
                    }
                }
            }
        }

        private void Update()
        {
            if (EdgeManager.gameStarted)
            {
                foreach (Observable observable in observables)
                {
                    if (RequiresUpdate(observable))
                    {
                       if (networkedPlayer && networkedPlayer.isLocalPlayer)
                       {
                           observable.SendDataToServer();
                       }
                    }
                }
            }
        }

        #endregion

        #region EdgeMultiplayObserver Functions
        /// <summary>
        ///UpdateObservers does the following:
        /// <para>1.Update the observables list indices.</para>
        /// <para>2.Updates/Assigns an Obserview View to the observed game objects.</para>
        /// <para>3.Disables the rigid body 2d or 3d on the observed game objects, so the physics can be simulated from the owner only.</para>
        /// <para>You should call UpdateObservables() at these situations:</para>
        /// <para>1.New observable added.</para>
        /// <para>2.Observable ownership change.</para>
        /// </summary>
        public void UpdateObservables()
        {
            for (int i = 0; i < observables.Count; i++)
            {
                ObservableView observerView;
                observables[i].SetObservableIndex(i);
                if (observables[i].observeredTransform.gameObject.GetComponent<ObservableView>())
                {
                    observerView = observables[i].observeredTransform.gameObject.GetComponent<ObservableView>();
                }
                else
                {
                    observerView = observables[i].observeredTransform.gameObject.AddComponent<ObservableView>();
                }
                observerView.SetupObserverView(networkedPlayer.playerId, i);
              
                if (observables[i].observeredTransform != null)
                {
                    observables[i].SetupObservable(networkedPlayer);
                }
                if (networkedPlayer && !networkedPlayer.isLocalPlayer && observables[i].owner == null)
                {
                    if (observables[i].observeredTransform && observables[i].observeredTransform.GetComponent<Rigidbody>())
                    {
                        observables[i].observeredTransform.GetComponent<Rigidbody>().isKinematic = true;
                    }
                    if (observables[i].observeredTransform && observables[i].observeredTransform.GetComponent<Rigidbody2D>())
                    {
                        observables[i].observeredTransform.GetComponent<Rigidbody2D>().isKinematic = true;
                    }
                }
                else
                {
                    if (observables[i].owner != null && !observables[i].owner.isLocalPlayer && observables[i].attachedToPlayer)
                    {
                        if (observables[i].observeredTransform.GetComponent<Rigidbody>())
                        {
                            observables[i].observeredTransform.GetComponent<Rigidbody>().isKinematic = true;
                        }
                        if (observables[i].observeredTransform.GetComponent<Rigidbody2D>())
                        {
                            observables[i].observeredTransform.GetComponent<Rigidbody2D>().isKinematic = true;
                        }
                    }
                    else
                    {
                        if (observables[i].owner != null && observables[i].owner.isLocalPlayer && observables[i].attachedToPlayer)
                        {
                            if (observables[i].observeredTransform.GetComponent<Rigidbody>())
                            {
                                observables[i].observeredTransform.GetComponent<Rigidbody>().isKinematic = false;
                            }
                            if (observables[i].observeredTransform.GetComponent<Rigidbody2D>())
                            {
                                observables[i].observeredTransform.GetComponent<Rigidbody2D>().isKinematic = false;
                            }
                        }
                    }
                }
            }
        }

        private bool RequiresUpdate(Observable observable)
        {
            switch (observable.syncOption)
            {
                case SyncOptions.SyncPosition:
                    if (observable.lastPosition != observable.observeredTransform.position)
                        return true;
                    else
                        return false;
                case SyncOptions.SyncRotation:
                    if (observable.lastRotation != observable.observeredTransform.rotation.eulerAngles)
                        return true;
                    else
                        return false;
                default:
                case SyncOptions.SyncPositionAndRotation:
                    if (observable.lastPosition != observable.observeredTransform.position || observable.lastRotation != observable.observeredTransform.rotation.eulerAngles)
                        return true;
                    else
                        return false;
            }
        }

        #endregion
    }
}
