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

                    var path = AssetDatabase.GUIDToAssetPath(scene.Guid);
                    scene.Name = Path.GetFileNameWithoutExtension(path);
                }
            }
        }

        public static IEnumerable<SceneAssetInfo> BuildScenesCollection(this BuildConfiguration configuration, BuildTarget builtTarget, bool stripAddressables)
        {
            var scenes = new List<SceneAssetInfo>();
            var defaultSceneAssets = stripAddressables
                ? configuration.DefaultScenes.Where(s => !s.Addressable).ToList()
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

        public static bool IsActive(this BuildConfiguration configuration, PlatformsConfiguration platformsConfiguration)
        {
            BuildTargetRuntime buildTarget = (BuildTargetRuntime)(int)EditorUserBuildSettings.activeBuildTarget;
            return platformsConfiguration.BuildTargets.Contains(buildTarget);
        }

        public static int GetSceneIndex(this BuildConfiguration configuration, SceneAssetInfo scene)
        {
            int platformScenesCount = 0;
            foreach (var platformConfiguration in configuration.Platforms)
            {
                if (IsActive(configuration, platformConfiguration))
                {
                    var platformIndex = platformConfiguration.Scenes.IndexOf(scene);
                    if (platformIndex >= 0)
                    {
                        return configuration.DefaultScenesFirst ? configuration.DefaultScenes.Count + platformIndex : platformIndex;
                    }

                    platformScenesCount = platformConfiguration.Scenes.Count;
                }
            }

            var defaultIndex = configuration.DefaultScenes.IndexOf(scene);
            if (defaultIndex >= 0)
            {
                return configuration.DefaultScenesFirst ? defaultIndex : defaultIndex + platformScenesCount;
            }

            return -1;
        }

        public static void SetupBuildSettings(this BuildConfiguration configuration, BuildTarget buildTarget)
        {
            var buildSettingsScenes = EditorBuildSettings.scenes.ToList();
            var buildSettingsSceneGuids = new HashSet<string>(buildSettingsScenes.Select(s => s.guid.ToString()));

            bool shouldUpdateBuildSettings = false;
            var configurationSceneGuids = configuration.BuildScenesCollection(buildTarget, false).Select(s => s.Guid);
            foreach (var sceneGuid in configurationSceneGuids)
            {
                if (buildSettingsSceneGuids.Contains(sceneGuid) == false)
                {
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

        static void ProcessPlatforms(ref List<SceneAssetInfo> scenes, BuildTarget buildTarget, List<PlatformsConfiguration> platforms, bool stripAddressable)
        {
            foreach (var platformsConfiguration in platforms)
            {
                var editorBuildTargets = platformsConfiguration.GetBuildTargetsEditor();
                if (editorBuildTargets.Contains(buildTarget))
                {
                    var platformScenes = stripAddressable
                        ? platformsConfiguration.GetNonAddressableScenes()
                        : platformsConfiguration.Scenes;

                    InsertScenes(ref scenes, platformScenes);
                    break;
                }
            }
        }

        static void InsertScenes(ref List<SceneAssetInfo> scenes, List<SceneAssetInfo> sceneToInsert)
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
