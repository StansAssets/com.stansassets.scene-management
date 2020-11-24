using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    /// <summary>
    /// Build settings for a separate configuration
    /// </summary>
    [Serializable]
    public class BuildConfiguration
    {
        public string Name = string.Empty;
        public bool DefaultScenesFirst = false;
        public List<SceneAssetInfo> DefaultScenes = new List<SceneAssetInfo>();
        public List<PlatformsConfiguration> Platforms = new List<PlatformsConfiguration>();

        public PlatformAddressablesMode DefaultAddressablesMode;
        public List<PlatformAddressableModeConfiguration> PlatformAddressableModeConfiguration;
        
        public bool IsEmpty
        {
            get
            {
                foreach (var platform in Platforms)
                {
                    if (platform.IsEmpty == false)
                    {
                        return false;
                    }
                }
                return DefaultScenes.Count == 0;
            }
        }

        internal BuildConfiguration Copy()
        {
            var copy = new BuildConfiguration();
            copy.Name = Name + " Copy";
            foreach (var scene in DefaultScenes)
            {
                copy.DefaultScenes.Add(scene);
            }

            foreach (var platformsConfiguration in Platforms)
            {
                var p = new PlatformsConfiguration();
                foreach (var target in platformsConfiguration.BuildTargets)
                {
                    p.BuildTargets.Add(target);
                }

                foreach (var scene in platformsConfiguration.Scenes)
                {
                    p.Scenes.Add(scene);
                }

                copy.Platforms.Add(p);
            }

            return copy;
        }

        // TODO we might need to cache this data once
        internal bool IsSceneAddressable(string sceneName) {
            foreach (var scene in DefaultScenes) {
                if (sceneName.Equals(scene.Name)) {
                    return scene.Addressable;
                }
            }

            // TODO should come from another runtime settings
            var buildTarget = ConvertRuntimePlatformToBuildTarget(Application.platform);
            var platform = GetConfigurationFroBuildTarget(buildTarget);

            foreach (var sceneAssetInfo in platform.Scenes) {
                if (sceneName.Equals(sceneAssetInfo.Name)) {
                    return sceneAssetInfo.Addressable;
                }
            }

            return false;
        }

        PlatformsConfiguration GetConfigurationFroBuildTarget(BuildTargetRuntime buildTarget)
        {
            foreach (var platform in Platforms)
            {
                if (platform.BuildTargets.Contains(buildTarget))
                {
                    return platform;
                }
            }

            return null;
        }

        BuildTargetRuntime ConvertRuntimePlatformToBuildTarget(RuntimePlatform platform) {
            switch (platform) {
#if UNITY_EDITOR
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:

                    return (BuildTargetRuntime)(int)UnityEditor.EditorUserBuildSettings.activeBuildTarget;
#endif
                case RuntimePlatform.Android:
                    return BuildTargetRuntime.Android;
                case RuntimePlatform.IPhonePlayer:
                    return BuildTargetRuntime.iOS;
                case RuntimePlatform.WebGLPlayer:
                    return BuildTargetRuntime.WebGL;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }

        public IEnumerable<BuildTargetRuntime> GetTargetPlatforms()
        {
            return Platforms.SelectMany(configuration => configuration.BuildTargets).Where(target => (int)target>0).Distinct();
        }

        public PlatformAddressablesMode GetAddressablesModeForPlatform(BuildTargetRuntime target)
        {
            var mode = PlatformAddressableModeConfiguration.FirstOrDefault(c => c.BuildTarget.Equals(target));
            return mode?.AddressablesMode ?? DefaultAddressablesMode;
        }

        public List<SceneAssetInfo> GetPlatformScenesAssets(BuildTargetRuntime buildTarget)
        {
            List<SceneAssetInfo> result = new List<SceneAssetInfo>();
            foreach (var platformsConfiguration in Platforms.Where(pc => pc.BuildTargets.Contains(buildTarget)))
            {
                result.AddRange(platformsConfiguration.Scenes.Where(sa => sa != null && !string.IsNullOrEmpty(sa.Guid)));
            }

            return result;
        }
        
        public List<SceneAssetInfo> GetAllScenesForPlatform(BuildTargetRuntime target)
        {
            List<SceneAssetInfo> result = new List<SceneAssetInfo>();
            IEnumerable<SceneAssetInfo> defaultScenes = DefaultScenes.Where(sa =>  sa != null && !string.IsNullOrEmpty(sa.Guid));
            if (DefaultScenesFirst)
            {
                result.AddRange(defaultScenes);
                result.AddRange(GetPlatformScenesAssets(target));
            }
            else
            {
                result.AddRange(GetPlatformScenesAssets(target));
                result.AddRange(defaultScenes);
            }
            return result;
        }

        public List<SceneAssetInfo> GetAddressableSceneAssets(BuildTargetRuntime target)
        {
            List<SceneAssetInfo> result;
            var addressablesMode = GetAddressablesModeForPlatform(target);
            switch (addressablesMode)
            {
                case PlatformAddressablesMode.UsePerSceneSettings:
                    result = GetAllScenesForPlatform(target).Where(sa => sa.Addressable).ToList();
                    break;
                case PlatformAddressablesMode.AllScenesAreNonAddressables:
                    result = new List<SceneAssetInfo>();
                    break;
                case PlatformAddressablesMode.AllScenesAreAddressable:
                    result = GetAllScenesForPlatform(target).Skip(1).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
        
        /*
         * Alias for GetSceneAssetsForBuild.
         * It is because both names (GetNonAddressableSceneAssets and GetSceneAssetsForBuild) are not obvious in some cases, so it is questionary which name is better
         */
        public List<SceneAssetInfo> GetNonAddressableSceneAssets(BuildTargetRuntime target)  
        {
            return GetSceneAssetsToIncludeInBuild(target);
        }
        
        public List<SceneAssetInfo> GetSceneAssetsToIncludeInBuild(BuildTargetRuntime target)
        {
            List<SceneAssetInfo> result;
            var addressablesMode = GetAddressablesModeForPlatform(target);
            switch (addressablesMode)
            {
                case PlatformAddressablesMode.UsePerSceneSettings:
                    result =  GetAllScenesForPlatform(target).Where(sa => !sa.Addressable).ToList();
                    break;
                case PlatformAddressablesMode.AllScenesAreNonAddressables:
                    result =  GetAllScenesForPlatform(target);
                    break;
                case PlatformAddressablesMode.AllScenesAreAddressable:
                    result =  GetAllScenesForPlatform(target).Take(1).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return result;
        }
    }
}
