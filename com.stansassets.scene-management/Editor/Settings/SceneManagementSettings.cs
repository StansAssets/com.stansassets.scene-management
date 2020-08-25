using System.Collections.Generic;
using StansAssets.Plugins;
using UnityEditor;

namespace StansAssets.SceneManagement
{
    public class SceneManagementSettings : PackageScriptableSettingsSingleton<SceneManagementSettings>
    {
        protected override bool IsEditorOnly => true;
        public override string PackageName => SceneManagementPackage.PackageName;
        
#if UNITY_2019_4_OR_NEWER
        public SceneAsset LandingScene;
        internal List<SceneStateInfo> OpenScenesBeforeLandingStart;
        internal int LastActiveSceneIndex;
        internal SceneViewInfo LastSceneView;
#endif
    }
}
