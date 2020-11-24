using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    static class BuildConfigurationExtension
    {
        public static void InitializeBuildData(this BuildConfiguration configuration, BuildTarget buildTarget)
        {
            var addressableSceneNames =  new List<string>();
            var addressableSceneAssets = new List<AddressableSceneAsset>();
            var allSceneNames = new List<string>();

            var addressableSceneAssetsFromConfig = configuration.GetAddressableSceneAssets(buildTarget.ToBuildTargetRuntime());
            foreach (var scene in addressableSceneAssetsFromConfig) {
                string path = AssetDatabase.GUIDToAssetPath(scene.Guid);
                if (string.IsNullOrEmpty(path))
                    continue;

                string sceneName = Path.GetFileNameWithoutExtension(path);
                allSceneNames.Add(sceneName);
                addressableSceneNames.Add(sceneName);
                addressableSceneAssets.Add(scene);
            }
            
            var nonAddressableSceneAssetsFilenames = configuration.GetNonAddressableSceneAssets(buildTarget.ToBuildTargetRuntime())
                                                                  .Select(sa => AssetDatabase.GUIDToAssetPath(sa.Guid))
                                                                  .Where(path => !string.IsNullOrEmpty(path))
                                                                  .Select(Path.GetFileNameWithoutExtension);
            foreach (var scene in nonAddressableSceneAssetsFilenames) {
                allSceneNames.Add(scene);
            }

            Debug.Log("Addressable Scenes: " + addressableSceneNames.Count);
            Debug.Log($"Addressable Scenes List:\n{string.Join("\n", addressableSceneAssets.Select(asset => AssetDatabase.GUIDToAssetPath(asset.Guid)))}");
            configuration.SetScenesConfig(addressableSceneNames, addressableSceneAssets, allSceneNames);
        }

        public static bool IsActive(this BuildConfiguration configuration, PlatformsConfiguration platformsConfiguration) {
            BuildTargetRuntime buildTarget = EditorUserBuildSettings.activeBuildTarget.ToBuildTargetRuntime();
            return platformsConfiguration.BuildTargets.Contains(buildTarget);
        }

        public static int GetSceneIndex(this BuildConfiguration configuration, AddressableSceneAsset scene)
        {
            return configuration.GetAllScenesForPlatform(EditorUserBuildSettings.activeBuildTarget.ToBuildTargetRuntime())
                                .Where(sa=>sa != null)
                                .Select(sa=>sa.Guid)
                                .ToList()
                                .IndexOf(scene.Guid);
            int platformScenesCount = 0;
            foreach (var platformConfiguration in configuration.Platforms) {
                if (IsActive(configuration, platformConfiguration)) {
                    var platformIndex = platformConfiguration.Scenes.IndexOf(scene);
                    if (platformIndex >= 0) {
                        return configuration.DefaultScenesFirst ? configuration.DefaultScenes.Count + platformIndex : platformIndex;
                    }

                    platformScenesCount = platformConfiguration.Scenes.Count;
                }
            }

            var defaultIndex = configuration.DefaultScenes.IndexOf(scene);
            if (defaultIndex >= 0) {
                return configuration.DefaultScenesFirst ? defaultIndex : defaultIndex + platformScenesCount;
            }

            return -1;
        }

        public static void SetupBuildSettings(this BuildConfiguration configuration, BuildTarget buildTarget) {
            var buildSettingsScenes = EditorBuildSettings.scenes.ToList();
            var buildSettingsSceneGuids = new HashSet<string>(buildSettingsScenes.Select(s => s.guid.ToString()));

            bool shouldUpdateBuildSettings = false;
            var configurationSceneGuids = configuration.GetSceneAssetsToIncludeInBuild(buildTarget.ToBuildTargetRuntime()).Select(s => s.Guid);
            foreach (var sceneGuid in configurationSceneGuids) {
                if (buildSettingsSceneGuids.Contains(sceneGuid) == false) {
                    string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                    if (string.IsNullOrEmpty(scenePath)) {
                        Debug.LogWarning($"Scene with Guid: {sceneGuid} can't be added!");
                        continue;
                    }

                    buildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    Debug.Log($"{BuildConfigurationSettingsValidator.TAG} Automatically added scene: {scenePath}");
                    shouldUpdateBuildSettings = true;
                }
            }

            if (shouldUpdateBuildSettings) {
                EditorBuildSettings.scenes = buildSettingsScenes.ToArray();
            }
        }

    }
}
