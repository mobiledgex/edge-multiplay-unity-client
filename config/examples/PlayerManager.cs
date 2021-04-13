using UnityEngine;
using EdgeMultiplay;
/** \@example PlayerManager.cs
* This is an example of how to use the EdgeMultiplay for your PlayerManager
* In this example how the player will communicate with other room members during game play, 
* Notice that for the PlayerManager to work we need to have using EdgeMultiplay; and and the class inherits from NetworkedPlayer
*/
public class PlayerManager : NetworkedPlayer {

  //! [ListenToMessages]
    void Start () {
      ListenToMessages();
    }
  //! [ListenToMessages]

//! [StopListening]
    void OnDestroy () {
      StopListening();
 }
    //! [StopListening]

    //! [OnMessageReceived]
    // Called once a GamePlay Event is received from the server
    public override void OnMessageReceived(GamePlayEvent gamePlayEvent){
      print ("GamePlayEvent received from server, event name: " + gamePlayEvent.eventName );
        switch (gamePlayEvent.eventName)
        {
            case "Shooting":
                print("Shooting");
                break;
            case "Score":
                //DoStuff();
                break;
            case "Collision":
                print("Collision");
                break;
        }
 }
    //! [OnMessageReceived]

    private void UpdateScore()
    {
        //! [BroadcastMessage]
        // send message to all other players
        // set the eventName to a unique name to differniate between your events OnMessageReceived()
        EdgeManager.MessageSender.BroadcastMessage(new GamePlayEvent()
        {
            eventName = "Score",
            floatData = new float[] { 1, 1 }
        });
        //! [BroadcastMessage]
    }

}
