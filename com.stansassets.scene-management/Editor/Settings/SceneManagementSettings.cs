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

        const string k_UseCameraAndScenePersistenceKey = "_use-camera-and-scene-persistency";

        internal bool UseCameraAndScenePersistence
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetBool($"{k_UseCameraAndScenePersistenceKey}", false);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                EditorPrefs.SetBool($"{k_UseCameraAndScenePersistenceKey}", value);
#endif
            }
        }

#endif

    }
}
