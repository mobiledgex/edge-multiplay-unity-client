/** \@example GameManager.cs
* This is an example of how to use the EdgeMultiplay for your GameManager
* In this example we connect to the server in the start function and we use EdgeMultiplayCallbacks such as OnConnectionToEdge() and others to control the game flow
* Notice that for the GameManager to work we need to have using EdgeMultiplay;, require EdgeManager Component and the class inherits from EdgeMultiplayCallbacks
*/
//! [requirements]
using UnityEngine;
using EdgeMultiplay;

[RequireComponent(typeof(EdgeManager))]
public class GameManager : EdgeMultiplayCallbacks {
    //! [requirements]
    // Use this for initialization
    //! [connecttoedge]
    void Start () {
      ConnectToEdge();
 }
    //! [connecttoedge]

    //! [OnConnectionToEdge]
    // Called once connected to your server deployed on Edge
    public override void OnConnectionToEdge(){
      print ("Connected to server deployed on Edge");
 }
    //! [OnConnectionToEdge]

    //! [OnRegisterEvent]
    // Called once the server registers the player right after the connection is established
    public override void OnRegisterEvent(){
      print ("Game Session received from server");
        //! [JoinOrCreateRoom]
        EdgeManager.JoinOrCreateRoom(playerName: "John Doe", playerAvatar: 0, maxPlayersPerRoom: 2);
        //! [JoinOrCreateRoom]
    }
    //! [OnRegisterEvent]

    //! [OnRoomJoin]
    // Called once the JoinRoom request succeeded 
    public override void OnRoomJoin(Room room){
      print ("Joined room");
      print ("Maximum Players in the room :"+ room.maxPlayersPerRoom); 
      print ("Count of Players in the room :"+ room.roomMembers.Count); 
    }
    //! [OnRoomJoin]
    //! [OnRoomCreated]
    // Called once the CreateRoom request succeeded 
    public override void OnRoomCreated(Room room){
      print ("Created a room");
      print ("Maximum Players in the room :"+ room.maxPlayersPerRoom); 
      print ("Count of Players in the room :"+ room.roomMembers.Count); 
 }
 //! [OnRoomCreated]

  //! [OnGameStart]
 // Called once the Game start on the server
 // The game starts on the server once the count of room members reachs the maximum players per room
 public override void OnGameStart(){
      print ("Game Started"); 
 }
    //! [OnGameStart]

    void Examples()
    {
        //! [JoinRoom]
        EdgeManager.JoinRoom(roomId:"room-1-1-2" ,playerName: "John Doe", playerAvatar: 0);
        //! [JoinRoom]
        //! [GetRooms]
        EdgeManager.GetRooms();
        //! [GetRooms]

        //! [CreateRoom]
        EdgeManager.CreateRoom(playerName:"John Doe",playerAvatar:0,maxPlayersPerRoom:2);
        //! [CreateRoom]

        //! [ExitRoom]
        EdgeManager.ExitRoom();
        //! [ExitRoom]

        //! [Disconnect]
        EdgeManager.Disconnect();
        //! [Disconnect]
    }
}
