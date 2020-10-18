using System.Collections.Generic;
using StansAssets.Plugins;
using UnityEditor;

namespace StansAssets.SceneManagement
{
    /// <summary>
    /// Scene Management Settings scriptable object.
    /// You can modify this settings using C# or Scene Management Editor Window.
    /// </summary>
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
