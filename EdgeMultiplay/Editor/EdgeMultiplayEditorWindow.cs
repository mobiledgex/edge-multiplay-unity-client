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
using System.IO;
using UnityEditor;
using UnityEngine;


namespace EdgeMultiplay
{
    [ExecuteInEditMode]
    public class EdgeMultiplayEditorWindow : EditorWindow
    {

        #region  EdgeMultiplay ToolBar Menu items

        [MenuItem("EdgeMultiplay/Examples/PingPong", false, 0)]
        public static void ImportPingPongExample()
        {
            string assetsFolder = Path.GetFullPath(Application.dataPath);
            AssetDatabase.ImportPackage(Path.Combine(assetsFolder, "EdgeMultiplay/Examples/PingPongExample.unitypackage"), true);
        }

        [MenuItem("EdgeMultiplay/Docs/Getting Started", false, 20)]
        public static void OpenGettingStartedURL()
        {
            Application.OpenURL("https://mobiledgex.github.io/edge-multiplay-unity-client/getting_started.html");
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
            Application.OpenURL("https://discord.gg/k22WcfMFZ3");
        }

        [MenuItem("EdgeMultiplay/Report a bug", false, 60)]
        public static void ReportBug()
        {
            Application.OpenURL("https://github.com/mobiledgex/edge-multiplay-unity-client/issues/new/choose");
        }
    }
}

#endregion