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

namespace EdgeMultiplay {
    /// <summary>
    /// ObserverableView is added by default to Observered objects,holding the reference to the observerable owner and the observerable index in in the observer.observerable list
    /// </summary>
    [AddComponentMenu("EdgeMultiplay/ObserverableView")]
    public class ObserverableView : MonoBehaviour
    {
        public string ownerId;
        public int observerableIndex; //the observerable index in in the observer.observerable list

        public void SetupObserverView(string ownerId, int observerableIndex)
        {
            this.ownerId = ownerId;
            this.observerableIndex = observerableIndex;
        }

        public bool OwnerIsLocalPlayer()
        {
            if(EdgeManager.localPlayer.playerId == ownerId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
