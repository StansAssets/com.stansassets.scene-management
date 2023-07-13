using System.Collections.Generic;
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
        
        internal static (IEnumerable<string> confScenes, IEnumerable<string> buildScenes) GetScenesCollections()
        {
            var configurationScenes = BuildConfigurationSettings.Instance.Configuration
                .BuildScenesCollection(new BuildScenesParams(EditorUserBuildSettings.activeBuildTarget, false, true))
                .Select(s=>s.Guid);

            var buildSettingsScenes = EditorBuildSettings.scenes.Select(s=>s.guid.ToString());
            
            return (configurationScenes, buildSettingsScenes);
        }
        
        static void EditorBuildSettingsOnSceneListChanged()
        {
            if (!CompareScenesWithBuildSettings())
            {
                BuildConfigurationMenu.UpdateBuildSettingsWindowStatus();
                return;
            }

            BuildConfigurationMenu.OpenBuildSettings();
            BuildConfigurationMenu.UpdateBuildSettingsWindowStatus();

            Debug.LogError($"{k_ScenesSyncDescription} Scenes can be synchronized through the " +
                           $"'Scene Management -> Build Settings'.");
        }

        internal static bool HasMissingScenes()
        {
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration) return false;

            return BuildConfigurationSettings.Instance.Configuration.DefaultSceneConfigurations.Any(conf => conf.Scenes.Any(s => s != null && string.IsNullOrEmpty(s.Guid))) 
                || BuildConfigurationSettings.Instance.Configuration
                    .Platforms.Any(p => p.Scenes.Any(s => s != null && string.IsNullOrEmpty(s.Guid)));
        }

        internal static bool HasScenesDuplicates()
        {
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration) return false;

            var conf = BuildConfigurationSettings.Instance.Configuration;

            var platformsDuplicates = conf.GetConfigurationRepetitiveScenes();
            if (platformsDuplicates.Any(s => s.Value.Any()))
            {
                return true;
            }

            var defaultInPlatform = conf.DefaultSceneConfigurations.Any(defConf => conf.GetDefaultInPlatformsDuplicateScenes(defConf.BuildTargetGroup).Any());
            if (defaultInPlatform) return true;
            
            var inConfig = conf.GetConfigurationRepetitiveScenes(EditorUserBuildSettings.activeBuildTarget).Any();
            if (inConfig) return true;

            var buildTarget = conf.GetBuildTargetDuplicateScenes(EditorUserBuildSettings.activeBuildTarget).Any();
            if (buildTarget) return true;

            return false;
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

        internal static bool HasAnyScene()
        {
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration) return false;

            return BuildConfigurationSettings.Instance.Configuration.DefaultSceneConfigurations.Any(conf => conf.Scenes.Any(i => i != null && !string.IsNullOrEmpty(i.Guid)))
                   || BuildConfigurationSettings.Instance.Configuration
                       .Platforms.Any(p => p.Scenes.Any(i => i != null && !string.IsNullOrEmpty(i.Guid)));
        }

        internal static bool HasBuildTargetsDuplicates()
        {
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration) return false;

            var conf = BuildConfigurationSettings.Instance.Configuration;
            var buildTargets = conf.Platforms
                .SelectMany(x => x.BuildTargets)
                .ToList();
            
            buildTargets.RemoveAll(runtime => runtime == BuildTargetRuntime.NoTarget);
            
            var duplicates = buildTargets
                .GroupBy(x => x)
                .Where(y => y.Count() > 1)
                .Select(z => z.Key);
            
            return duplicates.Any();
        }
    }
}
