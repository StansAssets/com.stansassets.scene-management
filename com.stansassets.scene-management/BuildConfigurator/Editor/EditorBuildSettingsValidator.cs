using System.Linq;
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
        
        public const string ScenesDuplicatesWarningDescription = "Repetitive scenes found. Resolve it manually!";

        public const string ScenesDuplicatesOkDescription = "There are no repetitive scenes.";

        public static readonly Color DuplicateColor = new Color(1f, 0.78f, 1f);

        const string k_HintDescription = "Scenes can be synchronized through the " +
                                         "'Scene Management -> Build Settings'.";

        /// <summary>
        /// Check current Editor Build Settings with the Scene Management build configuration to prevent out of sync scenes.
        /// </summary>
        /// <returns>True - if scenes are out of sync</returns>
        public static bool CompareScenesWithBuildSettings()
        {
            var needToSync = BuildConfigurationSettings.Instance.Configuration
                .CheckIntersectScenesWhBuildSettings(EditorUserBuildSettings.activeBuildTarget);

            return needToSync;
        }

        public static bool HasScenesDuplicates()
        {
            var hasDuplicates = BuildConfigurationSettings.Instance.Configuration
                .GetDuplicateScenes(EditorUserBuildSettings.activeBuildTarget).Any();

            return hasDuplicates;
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