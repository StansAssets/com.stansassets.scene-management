using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    static class BuildConfigurationExtension
    {
        public static List<SceneAsset> GetAddressableDefaultScenes(this BuildConfiguration configuration)
        {
            return configuration.DefaultScenes.Where(scene => scene.GetSceneAsset() != null && scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        public static List<SceneAsset> GetNonAddressableDefaultScenes(this BuildConfiguration configuration)
        {
            return configuration.DefaultScenes.Where(scene => scene.GetSceneAsset() != null && !scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        public static void InitializeBuildData(this BuildConfiguration configuration, BuildTarget buildTarget)
        {
            var addressableSceneNames =  new List<string>();
            var addressableSceneAssets = new List<AddressableSceneAsset>();
            var allSceneNames = new List<string>();

            var allSceneAssets = BuildScenesCollection(configuration, buildTarget, false);
            foreach (var scene in allSceneAssets) {
                string path = AssetDatabase.GUIDToAssetPath(scene.Guid);
                if (string.IsNullOrEmpty(path))
                    continue;

                string sceneName = Path.GetFileNameWithoutExtension(path);
                allSceneNames.Add(sceneName);
                if (scene.Addressable) {
                    addressableSceneNames.Add(sceneName);
                    addressableSceneAssets.Add(scene);
                }
            }

            Debug.Log("Addressable Scenes: " + addressableSceneNames.Count);
            Debug.Log($"Addressable Scenes List:\n{string.Join("\n", addressableSceneAssets.Select(asset => AssetDatabase.GUIDToAssetPath(asset.Guid)))}");
            configuration.SetScenesConfig(addressableSceneNames, addressableSceneAssets, allSceneNames);
        }

        public static List<AddressableSceneAsset> BuildScenesCollection(this BuildConfiguration configuration, BuildTarget builtTarget, bool stripAddressables) {
            var scenes = new List<AddressableSceneAsset>();

            List<AddressableSceneAsset> defaultSceneAssets = stripAddressables ? configuration.DefaultScenes.Where(s => !s.Addressable).ToList()
                                                                                      : configuration.DefaultScenes;

            if (configuration.DefaultScenesFirst)
            {
                ProcessPlatforms(ref scenes, builtTarget, configuration.Platforms, stripAddressables);
                InsertScenes(ref scenes, defaultSceneAssets);
            }
            else
            {
                InsertScenes(ref scenes, defaultSceneAssets);
                ProcessPlatforms(ref scenes, builtTarget, configuration.Platforms, stripAddressables);
            }

            return scenes;
        }

        public static bool IsActive(this BuildConfiguration configuration, PlatformsConfiguration platformsConfiguration) {
            BuildTargetRuntime buildTarget = (BuildTargetRuntime)(int)EditorUserBuildSettings.activeBuildTarget;
            return platformsConfiguration.BuildTargets.Contains(buildTarget);
        }

        public static int GetSceneIndex(this BuildConfiguration configuration, AddressableSceneAsset scene) {
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
            var configurationSceneGuids = configuration.BuildScenesCollection(buildTarget, false).Select(s => s.Guid);
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

        static void ProcessPlatforms(ref List<AddressableSceneAsset> scenes, BuildTarget buildTarget, List<PlatformsConfiguration> platforms, bool stripAddressable)
        {
            foreach (var platformsConfiguration in platforms)
            {
                var editorBuildTargets = platformsConfiguration.GetBuildTargetsEditor();
                if (editorBuildTargets.Contains(buildTarget)) {
                    var platformScenes = stripAddressable ? platformsConfiguration.GetNonAddressableScenes()
                                                                     : platformsConfiguration.Scenes;

                    InsertScenes(ref scenes, platformScenes);
                    break;
                }
            }
        }

        static void InsertScenes(ref List<AddressableSceneAsset> scenes, List<AddressableSceneAsset> sceneToInsert)
        {
            for (var index = 0; index < sceneToInsert.Count; index++)
            {
                var scene = sceneToInsert[index];
                if (string.IsNullOrEmpty(scene.Guid))
                    continue;

                if (scenes.Contains(scene))
                {
                    scenes.Remove(scene);
                }

                scenes.Insert(index, scene);
            }
        }
    }
}
