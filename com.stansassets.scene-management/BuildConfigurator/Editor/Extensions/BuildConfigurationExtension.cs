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
            return platformsConfiguration.BuildTargets.Contains(buildTarget)
                || platformsConfiguration.BuildTargets.Contains(BuildTargetRuntime.Editor);
        }

        public static int GetSceneIndex(this BuildConfiguration configuration, SceneAssetInfo scene, BuildTarget builtTarget)
        {
            var configurationSceneGuids =
                configuration.BuildScenesCollection(new BuildScenesParams(builtTarget, false, true));

            return configurationSceneGuids.ToList().IndexOf(scene);
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
            var stripAddressables = buildScenesParams.StripAddressables;
            var includeEditorScene = buildScenesParams.IncludeEditorScene;

            if (includeEditorScene)
            {
                var platformsConfiguration = platforms
                    .Where(b => b.BuildTargets.Contains(BuildTargetRuntime.Editor))
                    .ToArray();

                for (var i = platformsConfiguration.Length -1; i >= 0; i--)
                {
                    ProcessScene(ref scenes, platformsConfiguration[i], stripAddressables);
                }
            }

            foreach (var platformsConfiguration in platforms
                         .Where(b => b.GetBuildTargetsEditor().Contains(buildTarget)))
            {
                ProcessScene(ref scenes, platformsConfiguration, stripAddressables);
            }

            void ProcessScene(ref List<SceneAssetInfo> addIn, PlatformsConfiguration platformsConfiguration, bool stripAddressable)
            {
                var platformScenes = stripAddressable
                    ? platformsConfiguration.GetNonAddressableScenes()
                    : platformsConfiguration.Scenes;

                InsertScenes(ref addIn, platformScenes);
            }
        }

        static void InsertScenes(ref List<SceneAssetInfo> scenes, List<SceneAssetInfo> sceneToInsert)
        {
            var index = 0;

            foreach (var scene in sceneToInsert)
            {
                if (scene == null || string.IsNullOrEmpty(scene.Guid))
                    continue;

                if (scenes.Contains(scene))
                {
                    scenes.Remove(scene);
                }

                scenes.Insert(index, scene);
                index++;
            }
        }

        internal static bool CheckIntersectScenesWhBuildSettings(
            this BuildConfiguration configuration,
            BuildTarget buildTarget)
        {
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration)
            {
                return false;
            }

            var buildSettingsSceneGuids = new List<string>(EditorBuildSettings.scenes
                    .Select(s => s.path)).ToArray();

            var configurationSceneGuids = configuration
                .BuildScenesCollection(new BuildScenesParams(buildTarget, false, true))
                .Select(s => AssetDatabase.GUIDToAssetPath(s.Guid)).ToArray();

            if (!configurationSceneGuids.Any())
            {
                return false;
            }
            
            var outOfSync = configurationSceneGuids.Except(buildSettingsSceneGuids).Any()
                            || buildSettingsSceneGuids.Except(configurationSceneGuids).Any();
            
            if(!outOfSync)
            {
                outOfSync = configurationSceneGuids.Length != buildSettingsSceneGuids.Length
                || configurationSceneGuids.Where((t, i) => buildSettingsSceneGuids[i] != t).Any();
            }

            return outOfSync;
        }

        internal static bool CheckIntersectSceneWhBuildSettings(this BuildConfiguration configuration,
            BuildTarget buildTarget, string sceneGuid)
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            
            var configurationSceneGuids = configuration
                .BuildScenesCollection(new BuildScenesParams(buildTarget, false, true))
                .Select(s => s.Guid.ToString())
                .ToList();

            var inCurrentConfiguration = configurationSceneGuids
                .Any(i => AssetDatabase.GUIDToAssetPath(i).Equals(scenePath));
            if (!inCurrentConfiguration)
            {
                return true;
            }

            var synced = EditorBuildSettings.scenes
                .Any(i => AssetDatabase.GUIDToAssetPath(i.guid.ToString()).Equals(scenePath));
            return synced;
        }

        internal static bool CheckSceneDuplicate(this BuildConfiguration configuration, BuildTarget buildTarget, string sceneGuid)
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            
            bool IsSceneEqual(SceneAssetInfo i) => 
                i.Guid.Equals(sceneGuid) ||
                AssetDatabase.GUIDToAssetPath(i.Guid).Equals(scenePath);

            var buildInPlatform = GetDefaultInPlatformsDuplicateScenes(configuration);
            if (buildInPlatform.Any(IsSceneEqual))
            {
                return true;
            }

            var inConfig = GetConfigurationRepetitiveScenes(configuration, buildTarget);
            if(inConfig.Any(IsSceneEqual))
            {
                return true;
            }

            var inBuildTarget = GetBuildTargetDuplicateScenes(configuration, buildTarget);
            if(inBuildTarget.Any(IsSceneEqual))
            {
                return true;
            }
            
            return false;
        }

        internal static Dictionary<PlatformsConfiguration, IEnumerable<SceneAssetInfo>> GetConfigurationRepetitiveScenes(this BuildConfiguration configuration)
        {
            var result = new Dictionary<PlatformsConfiguration, IEnumerable<SceneAssetInfo>>();
            
            foreach (var platform in configuration.Platforms)
            {
                var duplicates = GetPlatformsConfigurationDuplicates(platform).ToArray();

                if (duplicates.Any())
                {
                    result.Add(platform, duplicates);
                }
            }
            
            return result;
        }

        internal static IEnumerable<SceneAssetInfo> GetPlatformsConfigurationDuplicates(PlatformsConfiguration platformsConfiguration)
        {
            var sceneAssetInfos = platformsConfiguration.Scenes
                .Where(g => g != null && !string.IsNullOrEmpty(g.Guid))
                .ToArray();

            var paths = sceneAssetInfos
                .ToDictionary(assetInfo => assetInfo, assetInfo => AssetDatabase.GUIDToAssetPath(assetInfo.Guid));

            var duplicates = paths
                .Where(d => paths.Count(p => p.Value.Equals(d.Value)) > 1)
                .Select(s=>s.Key)
                .ToArray();
            
            return duplicates;
        }
            
        internal static IEnumerable<SceneAssetInfo> GetConfigurationRepetitiveScenes(this BuildConfiguration configuration,
            BuildTarget buildTarget)
        {
            var configurationSceneGuids = configuration
                .BuildScenesCollection(new BuildScenesParams(buildTarget, false, true))
                .ToArray();

            var scenesPaths = configurationSceneGuids
                .Where(g => g != null && !string.IsNullOrEmpty(g.Guid))
                .Select(s => AssetDatabase.GUIDToAssetPath(s.Guid)).ToArray();
            
            var duplicates = configurationSceneGuids
                .Where (i=> i != null && 
                            scenesPaths.Count(x => x.Equals(AssetDatabase.GUIDToAssetPath(i.Guid))) > 1);
            return duplicates;
        }

        internal static IEnumerable<SceneAssetInfo> GetBuildTargetDuplicateScenes(this BuildConfiguration configuration,
            BuildTarget buildTarget)
        {
            var platform = configuration.Platforms
                .FirstOrDefault(f => 
                    f.BuildTargets.Contains((BuildTargetRuntime)buildTarget) || 
                    f.BuildTargets.Contains(BuildTargetRuntime.Editor));

            if (platform == null)
            {
                return new SceneAssetInfo[]{};
            }

            var duplicates = GetPlatformsConfigurationDuplicates(platform);
            return duplicates;
        }
        
        internal static IEnumerable<SceneAssetInfo> GetDefaultInPlatformsDuplicateScenes(this BuildConfiguration configuration)
        {
            var defaultScenesGuids = configuration.DefaultScenes
                .Where(g => g != null && !string.IsNullOrEmpty(g.Guid))
                .ToArray();
            
            var platformsScenesGuids = configuration.Platforms
                .SelectMany(p => p.Scenes)
                .Where(g => g != null && !string.IsNullOrEmpty(g.Guid))
                .ToArray();

            var duplicates = new List<SceneAssetInfo>();
            foreach (var sceneAssetInfo in defaultScenesGuids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneAssetInfo.Guid);
                var inConfig = platformsScenesGuids
                    .Where(s => AssetDatabase.GUIDToAssetPath(s.Guid).Equals(scenePath))
                    .ToArray();

                if (!inConfig.Any()) continue;
                
                duplicates.Add(sceneAssetInfo);
                duplicates.AddRange(inConfig);
            }

            return duplicates;
        }

        internal static void ClearMissingScenes(this BuildConfiguration configuration)
        {
            bool FindMissingScene(SceneAssetInfo i) => i == null || string.IsNullOrEmpty(i.Guid);

            configuration.DefaultScenes.RemoveAll(FindMissingScene);
            configuration.Platforms.ForEach(p => p.Scenes.RemoveAll(FindMissingScene));
        }

        internal static bool CheckBuildTargetDuplicate(this BuildConfiguration configuration, BuildTargetRuntime buildTarget)
        {
            var left = configuration.Platforms
                .Any(p => p.BuildTargets.Count(t => t == buildTarget) > 1);
            if (left)
            {
                return true;
            }
            
            var right = configuration.Platforms
                .Count(p => p.BuildTargets.Any(t => t == buildTarget)) > 1;
            if (right)
            {
                return true;
            }

            return false;
        }
    }

    struct BuildScenesParams
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