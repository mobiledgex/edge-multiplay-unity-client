/**
 *  Copyright 2018-2021 MobiledgeX, Inc. All rights and licenses reserved.
 *  MobiledgeX, Inc. 156 2nd Street #408, San Francisco, CA 94105
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
namespace EdgeMultiplay
{
    public class TemplateGenerator
    {
        [MenuItem("Assets/Create/EdgeMultiplay/GameManager", false, 1)]
        static void CreateGameManager()
        {
            string fileLocation = GetDirectoryPath() + "/GameManager.cs";
            if (File.Exists(fileLocation) == false)
            {
                using (StreamWriter outfile =
                    new StreamWriter(fileLocation))
                {
                    outfile.WriteLine("using UnityEngine;");
                    outfile.WriteLine("using EdgeMultiplay;");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("[RequireComponent(typeof(EdgeManager))]");
                    outfile.WriteLine("public class GameManager : EdgeMultiplayCallbacks {");
                    outfile.WriteLine(" ");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("    // Use this for initialization");
                    outfile.WriteLine("    void Start () {");
                    outfile.WriteLine("        ConnectToEdge(true,true);");
                    outfile.WriteLine("    }");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("    // Called once connected to your server deployed on Edge");
                    outfile.WriteLine("    public override void OnConnectionToEdge(){");
                    outfile.WriteLine("        print (\"Connected to server deployed on Edge\");");
                    outfile.WriteLine("    }");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("    // Called once the server registers the player right after the connection is established");
                    outfile.WriteLine("    public override void OnRegisterEvent(){");
                    outfile.WriteLine("        print (\"Game Session received from server\");");
                    outfile.WriteLine("        EdgeManager.JoinOrCreateRoom(\"playerName\", 0, 2);");
                    outfile.WriteLine("    }");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("    // Called once the JoinRoom request succeeded ");
                    outfile.WriteLine("    public override void OnRoomJoin(Room room){");
                    outfile.WriteLine("        print (\"Joined room\");");
                    outfile.WriteLine("        print (\"Maximum Players in the room :\"+ room.maxPlayersPerRoom); ");
                    outfile.WriteLine("        print (\"Count of Players in the room :\"+ room.roomMembers.Count); ");
                    outfile.WriteLine("    }");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("    // Called once the CreateRoom request succeeded ");
                    outfile.WriteLine("    public override void OnRoomCreated(Room room){");
                    outfile.WriteLine("        print (\"Created a room\");");
                    outfile.WriteLine("        print (\"Maximum Players in the room :\"+ room.maxPlayersPerRoom); ");
                    outfile.WriteLine("        print (\"Count of Players in the room :\"+ room.roomMembers.Count); ");
                    outfile.WriteLine("    }");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("    // Called once the Game start on the server");
                    outfile.WriteLine("    // The game starts on the server once the count of room members reachs the maximum players per room");
                    outfile.WriteLine("    public override void OnGameStart(){");
                    outfile.WriteLine("        print (\"Game Started\"); ");
                    outfile.WriteLine("    }");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("}");
                }
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/EdgeMultiplay/PlayerManager",false, 1)]
        static void CreatePlayerManager()
        {
            string fileLocation = GetDirectoryPath() + "/PlayerManager.cs";
            if (File.Exists(fileLocation) == false)
            {
                using (StreamWriter outfile =
                    new StreamWriter(fileLocation))
                {
                    outfile.WriteLine("using UnityEngine;");
                    outfile.WriteLine("using EdgeMultiplay;");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("public class PlayerManager : NetworkedPlayer {");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("    // Use this for initialization");
                    outfile.WriteLine("    void Start () {");
                    outfile.WriteLine("        ListenToMessages();");
                    outfile.WriteLine("    }");
                    outfile.WriteLine("    ");
                    outfile.WriteLine("    // Once the GameObject is destroyed");
                    outfile.WriteLine("    void OnDestroy () {");
                    outfile.WriteLine("        StopListening();");
                    outfile.WriteLine("    }");
                    outfile.WriteLine("    ");
                    outfile.WriteLine("    // Called once a GamePlay Event is received from the server");
                    outfile.WriteLine("    public override void OnMessageReceived(GamePlayEvent gamePlayEvent){");
                    outfile.WriteLine("        print (\"GamePlayEvent received from server, event name: \" + gamePlayEvent.eventName );");
                    outfile.WriteLine("    }");
                    outfile.WriteLine(" ");
                    outfile.WriteLine("}");
                }
            }
            AssetDatabase.Refresh();
        }

        static string GetDirectoryPath()
        {
            string path = "";
            var obj = Selection.activeObject;
            if (obj == null) path = "Assets";
            else path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (path.Length > 0)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
                else
                {
                    string[] pathFolders = path.Split('/');
                    path = "";
                    for (int i = 0; i < pathFolders.Length - 1; i++)
                    {
                        if (i == 0)
                        {
                            path += pathFolders[i];
                        }
                        else
                        {
                            path += "/" + pathFolders[i];
                        }
                    }
                    return path;
                }
                
            }
            else
            {
                throw new Exception("EdgeMultiplay: Can't create files outside the Assets folder");
            }

        }
    }
}