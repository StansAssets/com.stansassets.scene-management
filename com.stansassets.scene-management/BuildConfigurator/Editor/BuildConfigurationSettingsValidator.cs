using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    public class BuildConfigurationSettingsValidator
    {
        public class ValidationReport
        {
            public Dictionary<BuildConfiguration, ConfigurationValidationData> ConfigurationReports = new Dictionary<BuildConfiguration, ConfigurationValidationData>();
        }

        public class ConfigurationValidationData
        {
            public List<SceneAssetInfo> Duplicates = new List<SceneAssetInfo>();
            public int MissingScenesCounter;
        }

        public const string TAG = "[Build Configuration]";

        static BuildConfigurationSettingsValidator() {
            ValidateConfiguration();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state) {
            switch (state) {
                case PlayModeStateChange.EnteredPlayMode:
                    BuildConfigurationSettings.Instance.Configuration.SetupBuildSettings(EditorUserBuildSettings.activeBuildTarget);
                    break;
            }
        }

        static void ValidateConfiguration(bool printWarnings = true) {
            if (BuildConfigurationSettings.Instance == null) {
                throw new DataException("BuildConfigurationSettings is null!");
            }

            if (BuildConfigurationSettings.Instance.HasValidConfiguration) {
                foreach (var configuration in BuildConfigurationSettings.Instance.BuildConfigurations) {
                    HashSet<string> sceneGuids = new HashSet<string>();
                    foreach (var platformConfiguration in configuration.Platforms) {
                        ValidateScenes(platformConfiguration.Scenes, configuration, printWarnings, sceneGuids);
                    }

                    ValidateScenes(configuration.DefaultScenes, configuration, printWarnings, sceneGuids);
                }

                BuildConfigurationSettings.Instance.Configuration.SetupBuildSettings(EditorUserBuildSettings.activeBuildTarget);
            }
        }

        static void ValidateScenes(IEnumerable<SceneAssetInfo> scenes, BuildConfiguration configuration, bool printWarnings, HashSet<string> sceneGuids) {
            foreach (var scene in scenes) {
                var sceneAsset = scene.GetSceneAsset();
                if (sceneAsset == null) {
                    if(printWarnings)
                        Debug.LogWarning($"{TAG} Scene is missing in configuration: {configuration.Name}");
                }
                else if (sceneGuids.Contains(scene.Guid)) {
                    if(printWarnings)
                        Debug.LogWarning($"{TAG} Scene: {sceneAsset.name} duplicated in configuration: {configuration.Name}");
                }
                else {
                    sceneGuids.Add(scene.Guid);
                }
            }
        }

        ConfigurationValidationData GetValidationDataFor(BuildConfiguration configuration, ValidationReport report) {
            if (report.ConfigurationReports.TryGetValue(configuration, out var data) == false) {
                data = new ConfigurationValidationData();
                report.ConfigurationReports[configuration] = data;
            }

            return data;
        }
    }
}
