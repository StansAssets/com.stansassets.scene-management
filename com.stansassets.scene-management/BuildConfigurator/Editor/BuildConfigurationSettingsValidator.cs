using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    public class BuildConfigurationSettingsValidator
    {
        public const string TAG = "[Build Configuration]";

        const string k_ScenesSyncDescription = "Current Editor Build Settings are our of sync " +
                                               "with the Scene Management build configuration.";
        
        static BuildConfigurationSettingsValidator() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorBuildSettings.sceneListChanged += EditorBuildSettingsOnSceneListChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state) {
            switch (state) {
                case PlayModeStateChange.ExitingEditMode:
                    PreventOfPlayingOutOfSync();
                    break;
            }
        }
        
        /// <summary>
        /// Check current Editor Build Settings with the Scene Management build configuration to prevent out of sync scenes.
        /// </summary>
        /// <returns>True - if scenes are out of sync</returns>
        internal static bool CompareScenesWithBuildSettings()
        {
            var needToSync = BuildConfigurationSettings.Instance.Configuration
                .CheckIntersectScenesWhBuildSettings(EditorUserBuildSettings.activeBuildTarget);

            return needToSync;
        }
        
        static void EditorBuildSettingsOnSceneListChanged()
        {
            if (!CompareScenesWithBuildSettings())
            {
                return;
            }

            BuildConfigurationMenu.OpenBuildSettings();
            Debug.LogError($"{k_ScenesSyncDescription} Scenes can be synchronized through the " +
                           $"'Scene Management -> Build Settings'.");
        }

        internal static bool HasMissingScenes()
        {
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration) return false;

            return BuildConfigurationSettings.Instance.Configuration
                       .DefaultScenes.Any(s => s != null && string.IsNullOrEmpty(s.Guid)) 
                   || BuildConfigurationSettings.Instance.Configuration
                       .Platforms.Any(p => p.Scenes.Any(s => s != null && string.IsNullOrEmpty(s.Guid)));
        }

        internal static bool HasScenesDuplicates()
        {
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration) return false;
            
            var hasDuplicates = BuildConfigurationSettings.Instance.Configuration
                .GetDuplicateScenes().Any();

            return hasDuplicates;
        }
        
        static void PreventOfPlayingOutOfSync()
        {
            if (!BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog)
            {
                return;
            }

            var outOfSync = CompareScenesWithBuildSettings();

            if (!outOfSync)
            {
                return;
            }
            
            var result = EditorUtility.DisplayDialogComplex(
                "Scenes Management",
                k_ScenesSyncDescription,
                "Skip",
                "Open Scene Management",
                "Don't show again");

            switch (result)
            {
                case 0:
                    break;
                case 1:
                    EditorApplication.isPlaying = false;
                    BuildConfigurationMenu.OpenBuildSettings();
                    break;
                case 2:
                    BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog = false;
                    break;
            }
        }
    }
}
