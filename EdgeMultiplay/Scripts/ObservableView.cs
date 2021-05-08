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
    /// ObservableView is added by default to Observered objects,holding the reference to the observable owner and the observable index in in the observer.observable list
    /// </summary>
    [AddComponentMenu("EdgeMultiplay/ObservableView")]
    public class ObservableView : MonoBehaviour
    {
        public string ownerId;
        public int observableIndex; //the observable index in in the observer.observable list

        public void SetupObservableView(string ownerId, int observableIndex)
        {
            this.ownerId = ownerId;
            this.observableIndex = observableIndex;
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
