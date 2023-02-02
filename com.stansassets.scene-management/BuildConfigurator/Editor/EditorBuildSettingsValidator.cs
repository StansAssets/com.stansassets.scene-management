using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    public class EditorBuildSettingsValidator
    {
        public const string ScenesSyncDescription = "Current Editor Build Settings are our of sync " +
                                                    "with the Scene Management build configuration.";

        const string k_HintDescription= "Scenes can be synchronized through the " +
                                        "'Scene Management -> Build Settings'.";

        public static readonly Color OutOfSyncColor = new Color(0.93f, 0.39f, 0.32f);

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