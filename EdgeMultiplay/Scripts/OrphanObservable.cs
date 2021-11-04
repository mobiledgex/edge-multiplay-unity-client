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

namespace EdgeMultiplay
{
  /// <summary>
  /// Add OrphanObservable to GameObjects that don't have an owner at the time of creation (Ex. created in Unity Editor)
  /// the default owner of this observable is the first player in the room, You can change this in SetOwnerShip() below
  /// </summary>
  [AddComponentMenu("EdgeMultiplay/OrphanObservable")]
  public class OrphanObservable : MonoBehaviour
  {
    public Observable observable;

    void Update()
    {
      if (observable != null && observable.owner == null)
      {
        if (EdgeManager.gameStarted)
        {
          observable.SetOwnership(EdgeManager.GetPlayer(0).playerId);//owner is first player in the room
        }
      }
    }
  }
}