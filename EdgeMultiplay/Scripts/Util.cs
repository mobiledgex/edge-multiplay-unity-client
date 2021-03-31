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
    public class Util
    {
        /// <summary>
        /// Extracts position data as float array [3] from a transform component
        /// </summary>
        /// <param name="transformComponent"> Transform Component </param>
        /// <returns> float array of size 3 containing transform.position data </returns>
        public static float[] GetPositionData(Transform transformComponent)
        {
            return new float[3] { transformComponent.position.x, transformComponent.position.y, transformComponent.position.z };
        }

        /// <summary>
        /// Extracts rotation euler's angles as float array [3] from a transform component
        /// </summary>
        /// <param name="transformComponent"> Transform Component </param>
        /// <returns> float array of size 3  containing transform.rotation.eulerAngles data </returns>
        public static float[] GetRotationEulerData(Transform transformComponent)
        {
            return new float[3] { transformComponent.rotation.eulerAngles.x, transformComponent.rotation.eulerAngles.y, transformComponent.rotation.eulerAngles.z };
        }

        /// <summary>
        /// Extracts position and rotation euler's angles as float array [6] from a transform component
        /// </summary>
        /// <param name="transformComponent"> Transform Component </param>
        /// <returns> float array of size 6 containing transform.position data[3] and transform.rotation.eulerAngles[3] data respectively </returns>
        public static float[] GetPositionAndRotationData(Transform transformComponent)
        {
            return new float[6] {transformComponent.position.x, transformComponent.position.y, transformComponent.position.z,
                transformComponent.rotation.eulerAngles.x, transformComponent.rotation.eulerAngles.y, transformComponent.rotation.eulerAngles.z };
        }

        /// <summary>
        /// Extracts local position data as float array [3] from a transform component
        /// </summary>
        /// <param name="transformComponent"> Transform Component </param>
        /// <returns> float array of size 3  containing transform.localPosition data </returns>
        public static float[] GetLocalPositionData(Transform transformComponent)
        {
            return new float[3] { transformComponent.localPosition.x, transformComponent.localPosition.y, transformComponent.localPosition.z };
        }

        /// <summary>
        /// Extracts local rotation euler's angles as float array [3] from a transform component
        /// </summary>
        /// <param name="transformComponent"> Transform Component </param>
        /// <returns> float array of size 3  containing transform.localRotation data </returns>
        public static float[] GetLocalRotationData(Transform transformComponent)
        {
            return new float[3] { transformComponent.localRotation.eulerAngles.x, transformComponent.localRotation.eulerAngles.y, transformComponent.localRotation.eulerAngles.z };
        }

        /// <summary>
        /// Extracts local position and local rotation euler's angles as float array [6] from a transform component
        /// </summary>
        /// <param name="transformComponent"> Transform Component </param>
        /// <returns> float array of size 6 containing transform.localPosition data[3] and transform.localRotation.eulerAngles[3] data respectively </returns>
        public static float[] GetLocalPositionAndRotationData(Transform transformComponent)
        {
            return new float[6] {transformComponent.localPosition.x, transformComponent.localPosition.y, transformComponent.localPosition.z,
                transformComponent.localRotation.eulerAngles.x, transformComponent.localRotation.eulerAngles.y, transformComponent.localRotation.eulerAngles.z };
        }

        /// <summary>
        /// Sets the local position of the supplied transform component to the supplied targetLocalPositon 
        /// </summary>
        /// <param name="transformComponent"> Transform Component </param>
        /// <param name="targetPosition"> Vector3 object representing local position </param>
        public static void SetLocalPostion(Transform transformComponent, Vector3 targetLocalPosition)
        {
            var tempPosition = transformComponent.localPosition;
            tempPosition.Set(targetLocalPosition.x, targetLocalPosition.y, targetLocalPosition.z);
            transformComponent.localPosition = tempPosition;
        }

        /// <summary>
        /// Sets the local rotation of the supplied transform component to the supplied targetLocalRotation euler's angles
        /// </summary>
        /// <param name="transformComponent"> Transform Component </param>
        /// <param name="targetRotationEulers"> Vector3 object representing local rotation euler's angles </param>
        public static void SetLocalRotation(Transform transformComponent, Vector3 targetRotationEulers)
        {
            Quaternion tempRotation = transformComponent.localRotation;
            Quaternion rot = Quaternion.Euler(targetRotationEulers);
            tempRotation.Set(rot.x, rot.y, rot.z, rot.w);
            transformComponent.localRotation = tempRotation;
        }

        /// <summary>
        /// Converts a Vector3 to a float array of size 3
        /// </summary>
        /// <param name="vector3"> Vector3 object </param>
        /// <returns> float array of size 3 </returns>
        public static float[] ConvertVector3ToFloatArray(Vector3 vector3)
        {
            return new float[3] { vector3.x, vector3.y, vector3.z };
        }

        /// <summary>
        /// Converts a quaternion to a float array of size 4 -> (x,y,z,w)
        /// </summary>
        /// <param name="quaternion"> Quaternion object </param>
        /// <returns> float array of size 4 </returns>
        public static float[] ConvertQuaternionToFloatArray(Quaternion quaternion)
        {
            return new float[4] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        }

        /// <summary>
        /// Returns a Vector 3 from the supplied floatArray starting from the supplied startIndex, your floatArray size must be larger than 2
        /// </summary>
        /// <param name="startIndex"> the start index of the Vector3 in the float array </param>
        /// <returns>Vector3 Object</returns>
        public static Vector3 ConvertFloatArrayToVector3(float[] floatArray, int startIndex = 0)
        {
            if (floatArray.Length > (startIndex + 2))
            {
                return new Vector3(floatArray[startIndex], floatArray[startIndex + 1], floatArray[startIndex + 2]);
            }
            else
            {
                throw new Exception("floatArray starting from the start index doesn't qualify to create a Vector3");
            }
        }

        /// <summary>
        /// Returns a Quaternion from the supplied floatArray starting from the supplied startIndex, your floatArray size must be larger than 3
        /// </summary>
        /// <param name="startIndex"> the start index of the Quaternion in the float array </param>
        /// <returns>Quaternion Object</returns>
        public static Quaternion ConvertFloatArrayToQuaternion(float[] floatArray, int startIndex = 0)
        {
            if (floatArray.Length > (startIndex + 2))
            {
                return new Quaternion(floatArray[startIndex], floatArray[startIndex + 1], floatArray[startIndex + 2], floatArray[startIndex+3]);
            }
            else
            {
                throw new Exception("floatArray starting from the start index doesn't qualify to create a Vector3");
            }
        }
    }
}
