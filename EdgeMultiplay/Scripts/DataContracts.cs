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
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DistributedMatchEngine;

namespace EdgeMultiplay
{
    #region EdgeMultiplay Data Structures

    /// <summary>
    /// Synchronization Options
    /// </summary>
    public enum SyncOptions
    {
        SyncPosition,
        SyncRotation,
        SyncPositionAndRotation,
        SyncLocalPosition,
        SyncLocalRotation,
        SyncLocalPositionAndRotation
    }

    /// <summary>
    /// Called once a notification is received from the server
    /// Example of Notifications are new room created on server 
    /// </summary>
    [DataContract]
    public class Notification
    {
        [DataMember]
        public string type = "notification";

        [DataMember]
        public string notificationText;
    }

    /// <summary>
    /// Register Event is being emitted from the server once the connection starts
    /// </summary>
    [DataContract]
    public class Register
    {
        [DataMember]
        public string type = "register";

        [DataMember]
        public string sessionId;

        [DataMember]
        public string playerId;

    }

    /// <summary>
    /// Indicates that server created a room but still waiting for more players to Join
    /// </summary>
    [DataContract]
    public class RoomCreated
    {
        [DataMember]
        public string type = "roomCreated";
        [DataMember]
        public Room room;
    }


    /// <summary>
    /// Indicates that player joined a room but still waiting for more players to Join
    /// </summary>
    [DataContract]
    public class RoomJoin
    {
        [DataMember]
        public string type = "roomJoin";
        [DataMember]
        public Room room;

    }

    /// <summary>
    /// Event is received as the response for EdgeManager.GetRooms()
    /// </summary>
    [DataContract]
    public class RoomsList
    {
        [DataMember]
        public string type = "roomsList";
        [DataMember]
        public List<Room> rooms;
    }

    /// <summary>
    /// Event is received as the response for EdgeManager.GetAvailableRooms()
    /// </summary>
    [DataContract]
    public class AvailableRoomsList
    {
        [DataMember]
        public string type = "availableRoomsList";
        [DataMember]
        public List<Room> availableRooms;

    }

    /// <summary>
    /// Event is received once a new member joins a room that the local player is a member of
    /// the room member has the new updated list of room members
    /// </summary>
    [DataContract]
    public class PlayerJoinedRoom
    {
        [DataMember]
        public string type = "playerJoinedRoom";
        [DataMember]
        public Room room;
    }

    /// <summary>
    /// Event is received once a room member leaves a room that the local player is a member of
    /// </summary>
    [DataContract]
    public class RoomMemberLeft
    {
        [DataMember]
        public string type = "memberLeft";

        [DataMember]
        public string idOfPlayerLeft;

    }


    /// <summary>
    /// Indicates the start of the Game
    /// Once the number of players in a room reaches the room maximum player limit the game starts
    /// (Ex. If a room have maximum players (3), once the server assigns the third player to the room, the server will send GameStartEvent to all the room members)
    /// </summary>
    [DataContract]
    public class GameStart
    {
        [DataMember]
        public string type = "gameStart";

        [DataMember]
        public Room room;
    }

    /// <summary>
    /// GamePlayEvent is the main event for GamePlayEvents
    /// </summary>
    [DataContract]
    public class GamePlayEvent
    {
        [DataMember(IsRequired = true)]
        public string type;
        [DataMember (IsRequired = true)]
        public string roomId;
        [DataMember (IsRequired = true)]
        public string senderId;
        [DataMember (IsRequired = true)]
        public string eventName;
        [DataMember (EmitDefaultValue = false)]
        public string[] stringData;
        [DataMember (EmitDefaultValue = false)]
        public int[] integerData;
        [DataMember (EmitDefaultValue = false)]
        public float[] floatData;
        [DataMember (EmitDefaultValue = false)]
        public bool[] booleanData;

        public GamePlayEvent()
        {
            type = "GamePlayEvent";
        }

        public GamePlayEvent(string eventName, Vector3 position)
        {
            type = "GamePlayEvent";
            this.eventName = eventName;
            floatData = Util.ConvertVector3ToFloatArray(position);
        }

        public GamePlayEvent(string eventName, Quaternion rotation)
        {
            type = "GamePlayEvent";
            this.eventName = eventName;
            floatData = Util.ConvertVector3ToFloatArray(rotation.eulerAngles);
        }

        public GamePlayEvent(string eventName, Vector3 position, Quaternion rotation)
        {
            type = "GamePlayEvent";
            this.eventName = eventName;

            floatData = new float[6]
            {
                position.x, position.y, position.z,rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z
            };
        }

        public GamePlayEvent(string eventName, List<int> scoreArray)
        {
            type = "GamePlayEvent";
            this.eventName = eventName;
            integerData = scoreArray.ToArray();
        }

        public GamePlayEvent(string eventName, List<float> scoreArray)
        {
            type = "GamePlayEvent";
            this.eventName = eventName;
            floatData = scoreArray.ToArray();
        }

        public GamePlayEvent(string roomId, string playerId,
            string eventName, string[] stringData, int[] integerData, float[] floatData,
            bool[] booleanData)
        {
            type = "GamePlayEvent";
            this.roomId = roomId;
            senderId = playerId;
            this.eventName = eventName;
            this.stringData = stringData;
            this.integerData = integerData;
            this.floatData = floatData;
            this.booleanData = booleanData;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this); ;
        }
    }


    [Serializable]
    public class Observable
    {
        /// <summary>
        /// The Transform you want to Sync its position and/or rotation
        /// </summary>
        public Transform observeredTransform;
        /// <summary>
        /// Synchronization Option
        /// </summary>
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
        public int observerIndex;
        [HideInInspector]
        public Vector3 lastPosition;
        [HideInInspector]
        public Vector3 lastRotation;
        [HideInInspector]
        public NetworkedPlayer owner;
        [HideInInspector]
        public bool attachedToPlayer;
        /// <summary>
        /// Observable Constructor
        /// </summary>
        /// <param name="targetTransform">The Transform you want to Sync its position and/or rotation</param>
        /// <param name="syncOption">Synchronization Option</param>
        /// <param name="interpolatePosition">Set to true if you want to smoothen the tracked rotation if there is network lag</param>
        /// <param name="interpolateRotation">Set to true if you want to smoothen the tracked rotation if there is network lag</param>
        /// <param name="interpolationFactor">Set Interpolation factor value between 0.1 and 1</param>
        /// <param name="observerIndex">Observable index in observer.observables list</param>
        public Observable(Transform targetTransform, SyncOptions syncOption, bool interpolatePosition, bool interpolateRotation, float interpolationFactor, int observerIndex = 0)
        {
            this.observeredTransform = targetTransform;
            this.syncOption = syncOption;
            InterpolatePosition = interpolatePosition;
            InterpolateRotation = interpolateRotation;
            InterpolationFactor = interpolationFactor;
            this.observerIndex = observerIndex;
            
        }
        /// <summary>
        /// Observable Constructor
        /// </summary>
        /// <param name="targetTransform">The Transform you want to Sync its position and/or rotation</param>
        /// <param name="syncOption">Synchronization Option</param>
        /// <param name="interpolatePosition">Set to true if you want to smoothen the tracked rotation if there is network lag</param>
        /// <param name="interpolateRotation">Set to true if you want to smoothen the tracked rotation if there is network lag</param>
        /// <param name="interpolationFactor">Set Interpolation factor value between 0.1 and 1</param>
        public Observable(Transform targetTransform, SyncOptions syncOption, bool interpolatePosition, bool interpolateRotation, float interpolationFactor)
        {
            this.observeredTransform = targetTransform;
            this.syncOption = syncOption;
            InterpolatePosition = interpolatePosition;
            InterpolateRotation = interpolateRotation;
            InterpolationFactor = interpolationFactor;
        }

        public void SetObservableIndex(int index)
        {
            observerIndex = index;
        }

        public void SetupObservable(NetworkedPlayer observerOwner)
        {
            owner = observerOwner;
            switch (syncOption)
            {
                case SyncOptions.SyncPosition:
                    lastPosition = observeredTransform.transform.position;
                    break;
                case SyncOptions.SyncRotation:
                    lastRotation = observeredTransform.transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncPositionAndRotation:
                    lastPosition = observeredTransform.transform.position;
                    lastRotation = observeredTransform.transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncLocalPosition:
                    lastPosition = observeredTransform.transform.localPosition;
                    break;
                case SyncOptions.SyncLocalRotation:
                    lastRotation = observeredTransform.transform.localRotation.eulerAngles;
                    break;
                case SyncOptions.SyncLocalPositionAndRotation:
                    lastPosition = observeredTransform.transform.localPosition;
                    lastRotation = observeredTransform.transform.localRotation.eulerAngles;
                    break;
            }
        }

        public void SendDataToServer()
        {
            GamePlayEvent observerEvent = new GamePlayEvent();
            observerEvent.eventName = "EdgeMultiplayObserver";
            observerEvent.integerData = new int[2] { (int)syncOption, observerIndex };
            switch (syncOption)
            {
                case SyncOptions.SyncPosition:
                    observerEvent.floatData = Util.GetPositionData(observeredTransform.transform);
                    lastPosition = observeredTransform.transform.position;
                    break;
                case SyncOptions.SyncRotation:
                    observerEvent.floatData = Util.GetRotationEulerData(observeredTransform.transform);
                    lastRotation = observeredTransform.transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncPositionAndRotation:
                    observerEvent.floatData = Util.GetPositionAndRotationData(observeredTransform.transform);
                    lastRotation = observeredTransform.transform.rotation.eulerAngles;
                    lastPosition = observeredTransform.transform.position;
                    break;
                case SyncOptions.SyncLocalPosition:
                    observerEvent.floatData = Util.GetLocalPositionData(observeredTransform.transform);
                    lastPosition = observeredTransform.localPosition;
                    break;
                case SyncOptions.SyncLocalRotation:
                    observerEvent.floatData = Util.GetLocalRotationData(observeredTransform.transform);
                    lastRotation = observeredTransform.transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncLocalPositionAndRotation:
                    observerEvent.floatData = Util.GetLocalPositionAndRotationData(observeredTransform.transform);
                    lastPosition = observeredTransform.transform.localRotation.eulerAngles;
                    lastRotation = observeredTransform.transform.localPosition;
                    break;
            }
            EdgeManager.SendUDPMessage(observerEvent);
        }

        public void SetOwnership(string ownerId)
        {
            owner = EdgeManager.GetPlayer(ownerId);
            if(owner == null)
            {
                throw new Exception("EdgeMultiplay: Couldn't find player with id: " + ownerId);
            }

            if (owner.observer == null)
            {
                owner.gameObject.AddComponent<EdgeMultiplayObserver>();
            }
            owner.observer.observables.Add(this);
            owner.observer.UpdateObservables();
        }
        /// <summary>
        /// Changes the owner of an Observable object
        /// <para>ChangeOwnership() will change the owner in the local player's world and </para>
        /// <para>update all room members about Ownership change.</para>
        /// </summary>
        /// <param name="newOwnerId">The new owner player id</param>
        public void ChangeOwnership(string newOwnerId)
        {
            if (owner != null)
            {
                if (newOwnerId == owner.playerId)
                {
                    return;// ownership already sat
                }
                if (owner.observer != null)
                {
                    owner.observer.observables.Remove(this);
                }
                owner = EdgeManager.GetPlayer(newOwnerId);
                if(owner == null)
                {
                    throw new Exception("EdgeMultiplay: Couldn't find player with id: " + newOwnerId);
                }
                if (owner.observer == null)
                {
                    owner.gameObject.AddComponent<EdgeMultiplayObserver>();
                }
                owner.observer.observables.Add(this);
                owner.observer.UpdateObservables();
                GamePlayEvent changeOwnershipEvent = new GamePlayEvent()
                {
                    eventName = "ObservableOwnershipChange",
                    stringData = new string[1] { newOwnerId },
                    integerData = new int[1] { observerIndex }
                };
                EdgeManager.MessageSender.BroadcastMessage(changeOwnershipEvent);
            }
            else
            {
                Debug.LogError("EdgeMultiplay: Observer has no owner, Use observer.SetOwnership() first");
                return;
            }
        }
    }

    #endregion
    #region Requests

    /// <summary>
    /// CreateRoomRequest is sent to the server to create a room
    /// </summary>
    [DataContract]
    public class CreateRoomRequest
    {
        [DataMember]
        public string type = "CreateRoom";
        [DataMember]
        public string playerId;
        [DataMember]
        public string playerName;
        [DataMember]
        public int playerAvatar;
        [DataMember]
        public int maxPlayersPerRoom;
        [DataMember (EmitDefaultValue = false)]
        public Hashtable playerTags;
        public CreateRoomRequest(string PlayerName, int PlayerAvatar, int MaxPlayersPerRoom , Hashtable playerTags = null)
        {
            type = "CreateRoom";
            playerId = EdgeManager.gameSession.playerId;
            playerName = PlayerName;
            playerAvatar = PlayerAvatar;
            maxPlayersPerRoom = MaxPlayersPerRoom;
            this.playerTags = playerTags;
        }
    }

    /// <summary>
    /// JoinOrCreateRoomRequest is sent to the server to join a room or create a new room if there is no available rooms
    /// </summary>
    [DataContract]
    public class JoinOrCreateRoomRequest
    {
        [DataMember]
        public string type = "JoinOrCreateRoom";
        [DataMember]
        public string playerId;
        [DataMember]
        public string playerName;
        [DataMember]
        public int playerAvatar;
        [DataMember]
        public int maxPlayersPerRoom;
        [DataMember (EmitDefaultValue = false)]
        public Hashtable playerTags;

        public JoinOrCreateRoomRequest(string PlayerName, int PlayerAvatar, int MaxPlayersPerRoom, Hashtable playerTags = null)
        {
            type = "JoinOrCreateRoom";
            playerId = EdgeManager.gameSession.playerId;
            playerName = PlayerName;
            playerAvatar = PlayerAvatar;
            maxPlayersPerRoom = MaxPlayersPerRoom;
            this.playerTags = playerTags;
        }
    }

    /// <summary>
    /// JoinRoomRequest is sent to the server to join a specific room
    /// </summary>
    [DataContract]
    public class JoinRoomRequest
    {
        [DataMember]
        public string type = "JoinRoom";
        [DataMember]
        public string playerId;
        [DataMember]
        public string playerName;
        [DataMember]
        public int playerAvatar;
        [DataMember]
        public string roomId;
        [DataMember (EmitDefaultValue = false)]
        public Hashtable playerTags;

        public JoinRoomRequest(string RoomId, string PlayerName, int PlayerAvatar, Hashtable playerTags = null)
        {
            type = "JoinRoom";
            roomId = RoomId;
            playerId = EdgeManager.gameSession.playerId;
            playerName = PlayerName;
            playerAvatar = PlayerAvatar;
            this.playerTags = playerTags;
        }
    }

    /// <summary>
    /// GetRoomsRequest is sent to the server to get a list of the rooms on the server
    /// </summary>
    [DataContract]
    public class GetRoomsRequest
    {
        [DataMember]
        public string type = "GetRooms";
        public GetRoomsRequest()
        {
            type = "GetRooms";
        }
    }

    /// <summary>
    /// GetAvailableRooms is sent to the server to get a list of the availble rooms on the server
    /// available room is a room that is not full
    /// room x is available if room x have 2 members and the maximum players per room is 3
    /// </summary>
    [DataContract]
    public class GetAvailableRoomsRequest
    {
        [DataMember]
        public string type = "GetAvailableRooms";
        public GetAvailableRoomsRequest()
        {
            type = "GetAvailableRooms";
        }
    }

    [DataContract]
    public class ExitRoomRequest
    {
        [DataMember]
        public string type = "ExitRoom";
        [DataMember]
        public string playerId;
        [DataMember]
        public string roomId;

        public ExitRoomRequest()
        {
            type = "ExitRoom";
            playerId = EdgeManager.gameSession.playerId;
            roomId = EdgeManager.gameSession.roomId;
        }
    }

    #endregion
    #region EdgeMultiplay Helper Classes

    /// <summary>
    /// Wrapper class to hold game session data received from the server
    /// </summary>
    public class Session
    {
        public string sessionId;
        public string roomId = "";
        public string playerId;
        public string playerName;
        public string playerAvatar;
        public int playerIndex;
        public Player[] currentPlayers;
    }

    /// <summary>
    /// Wrapper class for room info received the server
    /// </summary>
    [DataContract]
    public class Room
    {
        /// <summary>
        /// Room Id assigned on ther server
        /// </summary>
        [DataMember]
        public string roomId;

        /// <summary>
        /// Generic List of players in the room
        /// </summary>
        [DataMember]
        public List<Player> roomMembers;

        /// <summary>
        /// Maximum Number of players per room
        /// </summary>
        [DataMember]
        public int maxPlayersPerRoom;
    }

    /// <summary>
    /// Wrapper class for the player info received from the server
    /// </summary>
    [DataContract]
    public class Player
    {
        /// <summary>
        /// Unique ID assigned to player once the connection is established with the server, assigned on RegisterEvent
        /// </summary>
        [HideInInspector]
        [DataMember]
        public string playerId;

        [DataMember]
        public string playerName;

        /// <summary>
        /// the player order in the room, ex. the first player to join a room has playerIndex of 0
        /// </summary>
        [DataMember]
        public int playerIndex;
        /// <summary>
        /// the player Avatar will be selected from EdgeManager SpawnPrefabs Array by index
        /// (ex. if playerAvatar=3 then the player will take the Avatar from EdgeManager.SpawnPrefabs[3]
        /// </summary>
        [DataMember]
        public int playerAvatar = 0;

        /// <summary>
        /// helper instance variable for serializing and deserializing playerTags
        /// </summary>
        [DataMember (EmitDefaultValue = false)]
        internal Hashtable playerTags;

        /// <summary>
        /// Dictionary<string,string> custom data associated with the player
        /// </summary>
        public Dictionary<string, string> playerTagsDict
        {
            get
            {
                if(playerTags != null)
                {
                    return Tag.HashtableToDictionary(playerTags);
                }
                else
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// Wrapper class to hold the return of GamePlayEvent.GetPositionAndRotation()
    /// </summary>
    public class PositionAndRotation
    {
        public Vector3 position;
        public Quaternion rotation;

        public PositionAndRotation(Vector3 vector3, Quaternion quaternion)
        {
            position = vector3;
            rotation = quaternion;
        }
    }

    /// <summary>
    /// Wrapper class for Position And Rotation Eulers  Used for EdgeManager Spawn Info
    /// </summary>
    [Serializable]
    public class PositionAndRotationEulers
    {
        public Vector3 position;
        /// <summary>
        /// Rotation Eulers similar as to the Rotation in the inspector
        /// </summary>
        public Vector3 rotation;
    }

    #endregion
    #region Serialization Helpers

    [DataContract]
    class MessageWrapper
    {
        [DataMember]
        public string type = "utf8";
        [DataMember]
        public string utf8Data;
        public static MessageWrapper WrapTextMessage(string jsonStr)
        {
            var wrapper = new MessageWrapper();
            wrapper.utf8Data = jsonStr;
            return wrapper;
        }
        public static MessageWrapper UnWrapMessage(string wrappedJsonStr)
        {
            var wrapper = Messaging<MessageWrapper>.Deserialize(wrappedJsonStr);
            return wrapper;
        }
    }

    static class Messaging<T>
    {
        private static string StreamToString(Stream s)
        {
            s.Position = 0;
            StreamReader reader = new StreamReader(s);
            string jsonStr = reader.ReadToEnd();
            return jsonStr;
        }

        public static string Serialize(T t)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();

            serializer.WriteObject(ms, t);
            string jsonStr = StreamToString(ms);

            return jsonStr;
        }

        public static T Deserialize(string jsonString)
        {
            using (MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString ?? "")))
            {
                return Deserialize(memStream);
            }
        }

        public static T Deserialize(Stream stream)
        {
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));
            T t = (T)deserializer.ReadObject(stream);
            return t;
        }
    }

    #endregion
}
