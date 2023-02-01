using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    public class EditorBuildSettingsValidator
    {
        public const string ScenesSyncWarningDescription = "Current Editor Build Settings are our of sync " +
                                                           "with the Scene Management build configuration.";

        public const string ScenesSyncOkDescription = "Editor Build Settings are synced " +
                                                      "with the Scene Management build configuration.";

        public static readonly Color OutOfSyncColor = new Color(0.93f, 0.39f, 0.32f);

        const string k_HintDescription = "Scenes can be synchronized through the " +
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

        static void EditorBuildSettingsOnSceneListChanged()
        {
            if (!CompareScenesWithBuildSettings())
            {
                return;
            }

            BuildConfigurationMenu.OpenBuildSettings();
            Debug.LogError($"{ScenesSyncWarningDescription}\n* {k_HintDescription}");
        }
    }
}