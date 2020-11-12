/**
 * Copyright 2018-2020 MobiledgeX, Inc. All rights and licenses reserved.
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
using EnhancementManager;

namespace EdgeMultiplay
{
    [ExecuteInEditMode]
    public class EdgeMultiplayEditorWindow : EditorWindow
    {
        #region  Mobiledgex ToolBar Menu items

        [MenuItem("EdgeMultiplay/Examples/PingPong", false, 0)]
        public static void ImportPingPongExamples()
        {
            string assetsFolder = Path.GetFullPath(Application.dataPath);
            AssetDatabase.ImportPackage(Path.Combine(assetsFolder, "EdgeMultiplay/Examples/PingPong.unitypackage"), true);
            Enhancement.EdgeMultiplayPingPongImported();
        }

        [MenuItem("EdgeMultiplay/Docs/Getting Started", false, 20)]
        public static void OpenGettingStartedURL()
        {
            Application.OpenURL("https://mobiledgex.github.io/EdgeMultiplay-UnityClient/getting_started.html");
            Enhancement.EdgeMultiplayDocsOpened();
        }

        [MenuItem("EdgeMultiplay/Docs/How It Works?", false, 20)]
        public static void OpenHowItWorksURL()
        {
            Application.OpenURL("https://mobiledgex.github.io/EdgeMultiplay-UnityClient/how_it_works.html");
            Enhancement.EdgeMultiplayDocsOpened();
        }

        [MenuItem("EdgeMultiplay/Docs/Documentation", false, 20)]
        public static void OpenDocumentationURL()
        {
            Application.OpenURL("https://mobiledgex.github.io/EdgeMultiplay-UnityClient");
            Enhancement.EdgeMultiplayDocsOpened();
        }

        [MenuItem("EdgeMultiplay/Report a bug", false, 60)]
        public static void ReportBug()
        {
            Application.OpenURL("https://github.com/mobiledgex/EdgeMultiplay-UnityClient/issues/new/choose");
        }
    }
}

#endregion