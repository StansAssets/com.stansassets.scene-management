using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    /// <summary>
    /// Build settings for a separate configuration
    /// </summary>
    [Serializable]
    public class BuildConfiguration
    {
        public string Guid;
        public string Name = string.Empty;
        public bool DefaultScenesFirst;
        public List<DefaultScenesConfiguration> DefaultSceneConfigurations = new();
        public List<PlatformsConfiguration> Platforms = new();

        const string k_UseAddressablesInEditorKey = "_user-addressables-in-editor";
        const string k_ClearAllAddressableCacheKey = "_user-clear-all-addressable-cache";

        public BuildConfiguration()
        {
            Guid = System.Guid.NewGuid().ToString();
        }

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

                return DefaultSceneConfigurations.Count == 0;
            }
        }

        internal bool UseAddressablesInEditor
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool($"{Guid}{k_UseAddressablesInEditorKey}", false);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool($"{Guid}{k_UseAddressablesInEditorKey}", value);
#endif
            }
        }

        internal bool ClearAllAddressablesCache
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool($"{Guid}{k_ClearAllAddressableCacheKey}", false);
#else
                return false;
#endif
            }

            set
            {
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool($"{Guid}{k_ClearAllAddressableCacheKey}", value);
#endif
            }
        }

        internal BuildConfiguration Copy()
        {
            var copy = new BuildConfiguration();
            copy.Name = Name + " Copy";

            foreach (var sceneConfiguration in DefaultSceneConfigurations)
            {
                copy.DefaultSceneConfigurations.Add(sceneConfiguration);
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
        internal bool IsSceneAddressable(string sceneName)
        {
            foreach (var sceneConfiguration in DefaultSceneConfigurations)
            {
                foreach (var scene in sceneConfiguration.Scenes)
                {
                    if (sceneName.Equals(scene.Name))
                    {
                        return scene.Addressable;
                    }
                }
            }

            // TODO should come from another runtime settings
            var buildTarget = ConvertRuntimePlatformToBuildTarget(Application.platform);
            var platform = GetConfigurationFroBuildTarget(buildTarget);
            if (platform != null)
            {
                foreach (var sceneAssetInfo in platform.Scenes)
                {
                    if (sceneName.Equals(sceneAssetInfo.Name))
                    {
                        return sceneAssetInfo.Addressable;
                    }
                }
            }

            return false;
        }

        internal void CleanEditorPrefsData()
        {
            //This is Editor-only code. Runtime part of this method should be empty.
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.DeleteKey($"{Guid}{k_UseAddressablesInEditorKey}");
            UnityEditor.EditorPrefs.DeleteKey($"{Guid}{k_ClearAllAddressableCacheKey}");
#endif
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

        BuildTargetRuntime ConvertRuntimePlatformToBuildTarget(RuntimePlatform platform)
        {
            switch (platform)
            {
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
        
        internal bool HasScene(string sceneName)
        {
            return DefaultSceneConfigurations.Any(c => c.Scenes.Any(s => s.Name.Equals(sceneName)))
                || Platforms.Any( p => p.Scenes.Any(i => i.Name.Equals(sceneName)));
        }
    }
}