using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    public class EditorBuildSettingsValidator
    {
        public const string ScenesSyncDescription = "Scenes in the Scene Management do not match the scenes " +
                                                    " in the Build Settings" +
                                                    ", they may not work correctly in the editor!";

        private const string k_HintDescription= "Scenes can be synchronized through the " +
                                                "'Scene Management -> Build Settings'.";

        public static bool CompareScenesWithBuildSettings()
        {
            var needToSync = BuildConfigurationSettings.Instance.Configuration
                .CheckIntersectScenesWhBuildSettings(EditorUserBuildSettings.activeBuildTarget);

            return needToSync;
        }

        static EditorBuildSettingsValidator()
        {
            EditorBuildSettings.sceneListChanged += EditorBuildSettingsOnSceneListChanged;
        }

        private static void EditorBuildSettingsOnSceneListChanged()
        {
            if (!CompareScenesWithBuildSettings())
            {
                return;
            }

            BuildConfigurationMenu.OpenBuildSettings();
            Debug.LogError($"{ScenesSyncDescription}\n* {k_HintDescription}");
        }
    }
}