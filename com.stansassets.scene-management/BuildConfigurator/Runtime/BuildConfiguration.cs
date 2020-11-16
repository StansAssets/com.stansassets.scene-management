using System;
using System.Collections.Generic;
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
    }
}
