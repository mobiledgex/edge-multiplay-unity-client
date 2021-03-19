using UnityEngine;

namespace EdgeMultiplay {
    /// <summary>
    /// ObserverableView is added by default to Observers, holding the reference to the observer owner and the observer id
    /// </summary>
    [AddComponentMenu("EdgeMultiplay/ObserverableView")]
    public class ObserverableView : MonoBehaviour
    {
        public string ownerId;
        public int observerableIndex;

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
