
using System;
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

        [HideInInspector]
        public Vector3 positionFromServer;
        [HideInInspector]
        public Vector3 rotationFromServer;
        [HideInInspector]
        public string eventId;
        private Vector3 lastPosition;
        private Vector3 lastRotation;
        private bool isLocalPlayerMaster;
        public enum SyncOptions
        {
            SyncPosition,
            SyncRotation,
            SyncPositionAndRotation
        }
        #endregion

        #region EdgeMultiplayObserver Editor exposed variables

        /// <summary>
        /// Set to true if the Component is attached a player that have a scripts that inherits from NetwokedPlayer
        /// </summary>
        [Tooltip("Check if the Observer is attached to a player object, otherwise leave unchecked.")]
        public bool attachedToPlayer;
        [Header("Sync Options", order = 1)]
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
        private void Awake()
        {
            if (!attachedToPlayer)
            {
                isLocalPlayerMaster = EdgeManager.localPlayerIsMaster;
            }
            else
            {
                isLocalPlayerMaster = false;
                 
            }
            EdgeManager.observers.Add(this);
            positionFromServer = transform.position;
            rotationFromServer = transform.rotation.eulerAngles;
        }

        private void Update()
        {
            if (EdgeManager.gameStarted) {
                if ((lastPosition != transform.position && syncOption == SyncOptions.SyncPosition)
                    || (lastRotation != transform.rotation.eulerAngles && syncOption == SyncOptions.SyncRotation)
                    || ((lastRotation != transform.rotation.eulerAngles || lastPosition != transform.position)
                         && syncOption == SyncOptions.SyncPositionAndRotation))
                {
                    if (attachedToPlayer)
                    {
                        if (GetComponent<NetworkedPlayer>() && GetComponent<NetworkedPlayer>().isLocalPlayer)
                        {
                                SendDataToServer(syncOption, true, GetComponent<NetworkedPlayer>().playerId);
                        }
                    }
                    else if (!attachedToPlayer && isLocalPlayerMaster)
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
                if (GetComponent<NetworkedPlayer>() && !GetComponent<NetworkedPlayer>().isLocalPlayer)
                {
                    ReflectServerData(syncOption);
                }
            }
            else if (!attachedToPlayer && !isLocalPlayerMaster)
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
                        transform.position = Vector3.Lerp(transform.position, positionFromServer, Time.deltaTime * InterpolationFactor);
                    else
                        transform.position = positionFromServer;
                    break;

                case SyncOptions.SyncRotation:
                    if (InterpolateRotation)
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotationFromServer), Time.deltaTime * InterpolationFactor);
                    else
                        transform.rotation = Quaternion.Euler(rotationFromServer);
                    break;

                case SyncOptions.SyncPositionAndRotation:
                    if (InterpolatePosition)
                        transform.position = Vector3.Lerp(transform.position, positionFromServer, Time.deltaTime * InterpolationFactor);
                    else
                        transform.position = positionFromServer;

                    if (InterpolateRotation)
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotationFromServer), Time.deltaTime * InterpolationFactor);
                    else
                        transform.rotation = Quaternion.Euler(rotationFromServer);
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
                    observerEvent.floatData = new float[3] { transform.position.x, transform.position.y, transform.position.z };
                    lastPosition = transform.position;
                    break;
                case SyncOptions.SyncRotation:
                    observerEvent.floatData = new float[3] { transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z };
                    lastRotation = transform.rotation.eulerAngles;
                    break;
                case SyncOptions.SyncPositionAndRotation:
                    observerEvent.floatData = new float[6] { transform.position.x, transform.position.y, transform.position.z
                        , transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z };
                    lastRotation = transform.rotation.eulerAngles;
                    lastPosition = transform.position;
                    break;
            }
            EdgeManager.SendUDPMessage(observerEvent);
        }

        #endregion
    }
}