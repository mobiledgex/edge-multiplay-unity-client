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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace EdgeMultiplay
{
    [ExecuteInEditMode]
    public class EdgeMultiplayEditorWindow : EditorWindow
    {

        #region  EdgeMultiplay ToolBar Menu items

        [MenuItem("EdgeMultiplay/Getting Started", false, 0)]
        public static void OpenGettingStartedURL()
        {
            Application.OpenURL("https://www.youtube.com/playlist?list=PLwUZZfaECSv18E5d0ooDR7S8416pImW8W");
        }

        [MenuItem("EdgeMultiplay/Examples/PingPong", false, 40)]
        public static void ImportPingPongExample()
        {
            string assetsFolder = Path.GetFullPath(Application.dataPath);
            AssetDatabase.ImportPackage(Path.Combine(assetsFolder, "EdgeMultiplay/Examples/PingPongExample.unitypackage"), true);
        }

        [MenuItem("EdgeMultiplay/Examples/Chat", false, 40)]
        public static void ImportChatExample()
        {
            string assetsFolder = Path.GetFullPath(Application.dataPath);
            AssetDatabase.ImportPackage(Path.Combine(assetsFolder, "EdgeMultiplay/Examples/ChatExample.unitypackage"), true);
        }

        [MenuItem("EdgeMultiplay/Examples/OwnershipExamples", false, 40)]
        public static void ImportOwnershipExamples()
        {
            string assetsFolder = Path.GetFullPath(Application.dataPath);
            AssetDatabase.ImportPackage(Path.Combine(assetsFolder, "EdgeMultiplay/Examples/OwnershipExamples.unitypackage"), true);
        }

        [MenuItem("EdgeMultiplay/Docs/How It Works?", false, 20)]
        public static void OpenHowItWorksURL()
        {
            Application.OpenURL("https://mobiledgex.github.io/edge-multiplay-unity-client/how_it_works.html");
        }

        [MenuItem("EdgeMultiplay/Docs/Documentation", false, 20)]
        public static void OpenDocumentationURL()
        {
            Application.OpenURL("https://mobiledgex.github.io/edge-multiplay-unity-client/");
        }

        [MenuItem("EdgeMultiplay/Join the Community", false, 20)]
        public static void JoinTheCommunity()
        {
            Application.OpenURL("https://discord.gg/CHCWfgrxh6");
        }

        [MenuItem("EdgeMultiplay/Report a bug", false, 60)]
        public static void ReportBug()
        {
            Application.OpenURL("https://github.com/mobiledgex/edge-multiplay-unity-client/issues/new/choose");
        }

        [MenuItem("EdgeMultiplay/Version 1.1", false, 80)]
        public static void Version()
        {
            //placeholder for version number
        }
    }
}

#endregion