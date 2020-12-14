
using System;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeMultiplay
{
    #region GameFlowEvents
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
    /// the room member have the new updated list of room members
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

        /// <summary>
        /// Returns a Vector 3 from the GamePlayEvent floatData
        /// </summary>
        /// <param name="startIndex"></param>
        /// <returns>Vector3 Object</returns>
        public Vector3 GetVector3(int startIndex = 0)
        {
            if (floatData.Length > (startIndex+2))
            {
                return new Vector3(floatData[startIndex], floatData[startIndex + 1], floatData[startIndex + 2]);
            }
            else
            {
                throw new Exception("Float Data starting from the start index doesn't qualify to create a Vector3"); 
            }
        }

        /// <summary>
        /// Returns a Quaternion from the GamePlayEvent floatData
        /// </summary>
        /// <param name="startIndex"></param>
        /// <returns>Quaternion Object represents Rotation</returns>
        public Quaternion GetQuaternion(int startIndex = 0)
        {
            if (floatData.Length > (startIndex + 2))
            {
                Vector3 eulers = GetVector3(startIndex);
                return Quaternion.Euler(eulers);
            }
            else
            {
                throw new Exception("Float Data starting from the start index doesn't qualify to create a Quaternion");
            }
        
        }

        /// <summary>
        /// Returns a PositionAndRotation from the GamePlayEvent floatData
        /// </summary>
        /// <param name="startIndex"></param>
        /// <returns>PositionAndRotation Object</returns>
        public PositionAndRotation GetPositionAndRotation(int startIndex = 0)
        {
            if (floatData.Length > (startIndex + 5))
            {
                Vector3 eulers = GetVector3(startIndex+3);
                return new PositionAndRotation(GetVector3(startIndex), Quaternion.Euler(eulers));
            }
            else
            {
                throw new Exception("Float Data starting from the start index doesn't qualify to create a PositionAndRotation");
            }
        }

        public GamePlayEvent()
        {
            type = "GamePlayEvent";
        }

        public GamePlayEvent(string eventName, Vector3 position)
        {
            type = "GamePlayEvent";
            this.eventName = eventName;
            floatData = new float[3] { position.x, position.y, position.z };
        }

        public GamePlayEvent(string eventName, Quaternion rotation)
        {
            type = "GamePlayEvent";
            this.eventName = eventName;
            floatData = new float[3] { rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z };
        }

        public GamePlayEvent(string eventName, Vector3 position, Quaternion rotation)
        {
            type = "GamePlayEvent";
            this.eventName = eventName;
            floatData = new float[6] { position.x, position.y, position.z ,
                rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z };
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

        public override string ToString()
        { 
            return type+"$" + roomId + "$" + senderId + "$" + eventName + "$" + string.Join(",", stringData) + "$" +string.Join(",", integerData) + "$" + string.Join(",", floatData) + "$" + string.Join(",", booleanData);
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

        public CreateRoomRequest(string PlayerName, int PlayerAvatar, int MaxPlayersPerRoom)
        {
            type = "CreateRoom";
            playerId = EdgeManager.gameSession.playerId;
            playerName = PlayerName;
            playerAvatar = PlayerAvatar;
            maxPlayersPerRoom = MaxPlayersPerRoom;
        }
    }

        /// <summary>
        /// JoinOrCreateRoomRequest is sent to the server to join a room or create a new room if it there is no available rooms
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

        public JoinOrCreateRoomRequest(string PlayerName, int PlayerAvatar, int MaxPlayersPerRoom)
        {
            type = "JoinOrCreateRoom";
            playerId = EdgeManager.gameSession.playerId;
            playerName = PlayerName;
            playerAvatar = PlayerAvatar;
            maxPlayersPerRoom = MaxPlayersPerRoom;
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

        public JoinRoomRequest(string RoomId, string PlayerName, int PlayerAvatar)
        {
            type = "JoinRoom";
            roomId = RoomId;
            playerId = EdgeManager.gameSession.playerId;
            playerName = PlayerName;
            playerAvatar = PlayerAvatar;
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
        public int udpReceivePort;
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
        /// UDP receive port for the player to communicate with the server, assigned by the server on GameStartEvent
        /// </summary>
        [HideInInspector]
        [DataMember]
        public int udpPort;
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