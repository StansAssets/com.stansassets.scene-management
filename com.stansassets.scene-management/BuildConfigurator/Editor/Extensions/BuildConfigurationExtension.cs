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

        public static IEnumerable<SceneAssetInfo> BuildScenesCollection(this BuildConfiguration configuration, BuildScenesParams buildScenesParams)
        {
            var stripAddressables = buildScenesParams.StripAddressables;
            var scenes = new List<SceneAssetInfo>();
            var defaultSceneAssets = stripAddressables
                ? configuration.DefaultScenes.Where(s => !s.Addressable).ToList()
                : configuration.DefaultScenes;

            if (configuration.DefaultScenesFirst)
            {
                ProcessPlatforms(ref scenes, configuration.Platforms, buildScenesParams);
                InsertScenes(ref scenes, defaultSceneAssets);
            }
            else
            {
                InsertScenes(ref scenes, defaultSceneAssets);
                ProcessPlatforms(ref scenes, configuration.Platforms, buildScenesParams);
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

        public static void SetupBuildSettings(this BuildConfiguration configuration, BuildTarget buildTarget,
            bool clearBuildSettings)
        {
            var buildSettingsScenes = clearBuildSettings
                ? new List<EditorBuildSettingsScene>()
                : EditorBuildSettings.scenes.ToList();
            var buildSettingsSceneGuids = new HashSet<string>(buildSettingsScenes.Select(s => s.guid.ToString()));

            bool shouldUpdateBuildSettings = false;
            var configurationSceneGuids = configuration.BuildScenesCollection(new BuildScenesParams(buildTarget, false, false)).Select(s => s.Guid);
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

                    if (!buildSettingsScenes.Any(i => i.guid.ToString().Equals(sceneGuid)))
                    {
                        buildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    }

                    Debug.Log($"{BuildConfigurationSettingsValidator.TAG} Automatically added scene: {scenePath}");
                    shouldUpdateBuildSettings = true;
                }
            }

            if (shouldUpdateBuildSettings)
            {
                EditorBuildSettings.scenes = buildSettingsScenes.ToArray();
            }
        }

        public static void SetupEditorSettings(this BuildConfiguration configuration, BuildTarget buildTarget,
            bool clearBuildSettings)
        {
            var buildSettingsScenes = clearBuildSettings
                ? new List<EditorBuildSettingsScene>()
                : EditorBuildSettings.scenes.ToList();
            var buildSettingsSceneGuids = new HashSet<string>(buildSettingsScenes.Select(s => s.guid.ToString()));

            bool shouldUpdateBuildSettings = false;
            var configurationSceneGuids =
                configuration.BuildScenesCollection(new BuildScenesParams(buildTarget, false, true)).Select(s => s.Guid);
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

                    if (!buildSettingsScenes.Any(i => i.guid.ToString().Equals(sceneGuid)))
                    {
                        buildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    }

                    Debug.Log($"{BuildConfigurationSettingsValidator.TAG} Automatically added scene: {scenePath}");
                    shouldUpdateBuildSettings = true;
                }
            }

            if (shouldUpdateBuildSettings)
            {
                EditorBuildSettings.scenes = buildSettingsScenes.ToArray();
            }
        }

        static void ProcessPlatforms(ref List<SceneAssetInfo> scenes, List<PlatformsConfiguration> platforms, BuildScenesParams buildScenesParams)
        {
            var buildTarget = buildScenesParams.BuiltTarget;
            var stripAddressable = buildScenesParams.StripAddressables;
            var includeEditorScene = buildScenesParams.IncludeEditorScene;
            
            foreach (var platformsConfiguration in platforms)
            {
                var editorBuildTargets = platformsConfiguration.GetBuildTargetsEditor();
                if (editorBuildTargets.Contains(buildTarget)
                    || (
                        includeEditorScene &&
                        platformsConfiguration.BuildTargets.Contains(BuildTargetRuntime.Editor)
                    )
                   )
                {
                    var platformScenes = stripAddressable
                        ? platformsConfiguration.GetNonAddressableScenes()
                        : platformsConfiguration.Scenes;

                    InsertScenes(ref scenes, platformScenes);
                }
            }
        }

        static void InsertScenes(ref List<SceneAssetInfo> scenes, List<SceneAssetInfo> sceneToInsert)
        {
            for (var index = 0; index < sceneToInsert.Count; index++)
            {
                var scene = sceneToInsert[index];
                if (scene == null || string.IsNullOrEmpty(scene.Guid))
                    continue;

                if (scenes.Contains(scene))
                {
                    scenes.Remove(scene);
                }

                scenes.Insert(index, scene);
            }
        }

        public static bool CheckIntersectScenesWhBuildSettings(
            this BuildConfiguration configuration,
            BuildTarget buildTarget)
        {
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration)
            {
                return false;
            }

            var buildSettingsSceneGuids = new List<string>(EditorBuildSettings.scenes
                    .Select(s => s.guid.ToString()))
                .ToList();

            var configurationSceneGuids = configuration
                .BuildScenesCollection(new BuildScenesParams(buildTarget, false, true))
                .Select(s => s.Guid.ToString())
                .ToList();

            var intersect = configurationSceneGuids.Where(i => !buildSettingsSceneGuids.Contains(i)).ToList();
            var viseVersaIntersect = buildSettingsSceneGuids.Where(i => !configurationSceneGuids.Contains(i)).ToList();

            return intersect.Any() || viseVersaIntersect.Any();
        }

        public static bool CheckIntersectSceneWhBuildSettings(this BuildConfiguration configuration,
            BuildTarget buildTarget, string sceneGuid)
        {
            var configurationSceneGuids = configuration
                .BuildScenesCollection(new BuildScenesParams(buildTarget, false, true))
                .Select(s => s.Guid.ToString())
                .ToList();

            var inCurrentConfiguration = configurationSceneGuids.Any(i => i.Equals(sceneGuid));
            if (!inCurrentConfiguration)
            {
                return true;
            }

            var synced = EditorBuildSettings.scenes.Any(i => i.guid.ToString().Equals(sceneGuid));
            return synced;
        }
        
        public static bool CheckSceneDuplicate(this BuildConfiguration configuration, BuildTarget buildTarget, string sceneGuid)
        {
            var duplicates = GetDuplicateScenes(configuration, buildTarget);
            return duplicates.Any(i => i.Guid.Equals(sceneGuid));
        }
        
        public static IEnumerable<SceneAssetInfo> GetDuplicateScenes(this BuildConfiguration configuration, BuildTarget buildTarget)
        {
            var configurationSceneGuids = configuration
                .BuildScenesCollection(new BuildScenesParams(buildTarget, false, true))
                .ToArray();
            var scenesPaths = configurationSceneGuids.Select(s => AssetDatabase.GUIDToAssetPath(s.Guid)).ToArray();
            var duplicates = new List<SceneAssetInfo>();

            foreach (var sceneAssetInfo in configurationSceneGuids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneAssetInfo.Guid);
                var duplicated = scenesPaths.Count(i => i.Equals(scenePath)) > 1;

                if (duplicated)
                {
                    duplicates.Add(sceneAssetInfo);
                }
            }
   
            return duplicates;
        }
    }

    internal struct BuildScenesParams
    {
        internal readonly BuildTarget BuiltTarget; 
        internal readonly bool StripAddressables;
        
        /// <summary>
        /// Include in the collection the "Editor" platform scenes.
        /// It is only needed to work in the editor, otherwise set to "false" to prepare the collection for build.
        /// </summary>
        internal readonly bool IncludeEditorScene;

        public BuildScenesParams(BuildTarget builtTarget, bool stripAddressables, bool includeEditorScene)
        {
            BuiltTarget = builtTarget;
            StripAddressables = stripAddressables;
            IncludeEditorScene = includeEditorScene;
        }
    }
}