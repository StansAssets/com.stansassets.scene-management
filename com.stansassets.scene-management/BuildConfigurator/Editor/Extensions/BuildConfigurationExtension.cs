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
        public static List<SceneAsset> GetAddressableDefaultScenes(this BuildConfiguration configuration)
        {
            return configuration.DefaultScenes.Where(scene => scene.GetSceneAsset() != null && scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        public static List<SceneAsset> GetNonAddressableDefaultScenes(this BuildConfiguration configuration)
        {
            return configuration.DefaultScenes.Where(scene => scene.GetSceneAsset() != null && !scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        public static void InitializeBuildData(this BuildConfiguration buildConfiguration, BuildTarget buildTarget)
        {
            // TODO here we may want to create a runtime config and save some info
            // but from now let's just make sure we always have valid names

            buildConfiguration.UpdateSceneNames();
        }

        public static void UpdateSceneNames(this BuildConfiguration buildConfiguration)
        {
            foreach (var scene in buildConfiguration.DefaultScenes)
            {
                if(scene == null)
                    continue;

                var path = AssetDatabase.GUIDToAssetPath(scene.Guid);
                scene.Name = Path.GetFileNameWithoutExtension(path);
            }

            foreach (var platform in buildConfiguration.Platforms)
            {
                foreach (var scene in platform.Scenes)
                {
                    if(scene == null)
                        continue;
                        }
            }
        }

        public static bool IsActive(this BuildConfiguration configuration, PlatformsConfiguration platformsConfiguration) {
            BuildTargetRuntime buildTarget = EditorUserBuildSettings.activeBuildTarget.ToBuildTargetRuntime();
            return platformsConfiguration.BuildTargets.Contains(buildTarget);
        }

        public static int GetSceneIndex(this BuildConfiguration configuration, SceneAssetInfo scene)
        {
            return configuration.GetAllScenesForPlatform(EditorUserBuildSettings.activeBuildTarget.ToBuildTargetRuntime())
                                .Where(sa=>sa != null)
                                .Select(sa=>sa.Guid)
                                .ToList()
                                .IndexOf(scene.Guid);
        }

        public static void SetupBuildSettings(this BuildConfiguration configuration, BuildTarget buildTarget)
        {
            var buildSettingsScenes = EditorBuildSettings.scenes.ToList();
            var buildSettingsSceneGuids = new HashSet<string>(buildSettingsScenes.Select(s => s.guid.ToString()));

            bool shouldUpdateBuildSettings = false;
            var configurationSceneGuids = configuration.GetSceneAssetsToIncludeInBuild(buildTarget.ToBuildTargetRuntime()).Select(s => s.Guid);
            foreach (var sceneGuid in configurationSceneGuids) {
                if (buildSettingsSceneGuids.Contains(sceneGuid) == false) {
                    string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                    if (string.IsNullOrEmpty(scenePath))
                    {
                        Debug.LogWarning($"Scene with Guid: {sceneGuid} can't be added!");
                        continue;
                    }

                    buildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    Debug.Log($"{BuildConfigurationSettingsValidator.TAG} Automatically added scene: {scenePath}");
                    shouldUpdateBuildSettings = true;
                }
            }

            if (shouldUpdateBuildSettings)
            {
                EditorBuildSettings.scenes = buildSettingsScenes.ToArray();
            }
        }

    }
}
