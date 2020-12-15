
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeMultiplay
{
    /// <summary>
    /// The player script should inherit from Networked Player
    /// NetworkedPlayer contains all the player data retrieved from the server
    /// </summary>
    [AddComponentMenu("EdgeMultiplay/NetworkedPlayer")]
    public class NetworkedPlayer : EdgeMultiplayCallbacks
    {
        #region NetworkedPlayer Variables

        /// <summary>
        /// the id of the room that the player is a member of
        /// </summary>
        public string roomId;
        /// <summary>
        /// the player order in the room, ex. the first player to join a room has playerIndex of 0
        /// </summary>
        public int playerIndex;
        /// <summary>
        /// The player Id that the server assigns to the player
        /// </summary>
        public string playerId;
        public string playerName;
        //public Player gamePlayer;
        public Action<GamePlayEvent> playerEvent;
        public bool isLocalPlayer;
        /// <summary>
        /// Can be used to set the player to be active or inactive although the player is still an active connection in the room
        /// </summary>
        public bool ActivePlayer = true;
        EdgeManager edgeManager;

        #endregion

        #region NetworkedPlayer Functions

        private void Awake()
        {
           edgeManager = FindObjectOfType<EdgeManager>();
        }

        /// <summary>
        /// Call ListenToMessages() to start listening to messages from the server
        /// </summary>
        public void ListenToMessages()
        {
            if (!isLocalPlayer)
            {
                playerEvent += OnMessageReceived;
                if (GetComponent<Rigidbody>())
                    Destroy(GetComponent<Rigidbody>());
                if (GetComponent<Rigidbody2D>())
                    Destroy(GetComponent<Rigidbody2D>());

            }
        }
        /// <summary>
        /// Add StopListening when you want to stop listening to messages from the server
        /// Recommended to have it under the Monobehaviour callback OnDestroy()
        /// </summary>
        public void StopListening()
        {
            if (!isLocalPlayer)
            {
                playerEvent -= OnMessageReceived;
            }
        }

        public override void OnWebSocketEventReceived(GamePlayEvent mobiledgexEvent)
        {
            if (!isLocalPlayer)
            {
                if (mobiledgexEvent.senderId == playerId)
                {
                    playerEvent(mobiledgexEvent);
                }
            }
        }

        public override void OnUDPEventReceived(GamePlayEvent udpEvent)
        { 
            if (!isLocalPlayer)
            {
                if (udpEvent.senderId == playerId)
                {
                   playerEvent(udpEvent);
                }
            }
        }

        /// <summary>
        /// Called once GamePlay Events Received from the server
        /// </summary>
        /// <param name="gamePlayEvent">the received gamePlayEvent</param>
        public virtual void OnMessageReceived(GamePlayEvent gamePlayEvent)
        {
            Debug.Log("GamePlayEvent Received");
        }

        public void SetUpPlayer(Player playerFromServer, string roomId, bool isLocalPlayer =false)
        {
            this.roomId = roomId;
            playerName = playerFromServer.playerName;
            playerIndex = playerFromServer.playerIndex;
            playerId = playerFromServer.playerId;
            this.isLocalPlayer = isLocalPlayer;
            ActivePlayer = true;
        }
        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(GamePlayEvent gamePlayEvent)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent()
            {
                eventName = gamePlayEvent.eventName,
                booleanData = gamePlayEvent.booleanData,
                stringData = gamePlayEvent.stringData,
                integerData = gamePlayEvent.integerData,
                floatData = gamePlayEvent.floatData,
            };
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, Vector3 position)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, position);
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, Quaternion rotation)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, rotation);
            edgeManager.SendGamePlayEvent(gameplayEvent) ;
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, Vector3 position, Quaternion rotation)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, position, rotation);
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, List<int> integerArray)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, integerArray);
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, List<float> floatArray)
        {
            GamePlayEvent gameplayEvent = new GamePlayEvent(eventName, floatArray);
            edgeManager.SendGamePlayEvent(gameplayEvent);
        }

        /// <summary>
        /// Sends an event to all players in the room except the sender
        /// </summary>
        public void BroadcastMessage(string eventName, string[] stringData = null , int[] commandInts = null ,
            float[] floatData = null,
            bool[] booleanData = null)
        {
            GamePlayEvent gamePlayEvent = new GamePlayEvent(this.roomId, playerId, eventName, stringData, commandInts, floatData, booleanData);
            edgeManager.SendGamePlayEvent(gamePlayEvent);
        }

        #endregion
    }
}
