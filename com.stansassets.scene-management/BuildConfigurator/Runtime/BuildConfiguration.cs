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
        public List<AddressableSceneAsset> DefaultScenes = new List<AddressableSceneAsset>();
        public List<PlatformsConfiguration> Platforms = new List<PlatformsConfiguration>();

        Dictionary<string, AddressableSceneAsset> m_AddressableSceneNamesToSceneAssets;
        [SerializeField] List<string> m_SceneNames = new List<string>();
        [SerializeField] List<AddressableSceneAsset> m_SceneAssets = new List<AddressableSceneAsset>();
        [SerializeField] List<string> m_AllSceneNames = new List<string>();
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

        internal bool IsSceneAddressable(string sceneName)
        {
            if (AddressableSceneNamesToSceneAssets.TryGetValue(sceneName, out var asset))
            {
                return asset.Addressable;
            }
            return false;
        }

        internal bool HasScene(string sceneName) {
            return m_AllSceneNames.Contains(sceneName);
        }

        internal void SetScenesConfig(List<string> addressableSceneNames, List<AddressableSceneAsset> addressableSceneAssets, List<string> allSceneNames)
        {
            m_AddressableSceneNamesToSceneAssets = null;
            m_SceneNames = addressableSceneNames;
            m_SceneAssets = addressableSceneAssets;
            m_AllSceneNames = allSceneNames;
        }

        Dictionary<string, AddressableSceneAsset> AddressableSceneNamesToSceneAssets
        {
            get
            {
                if (m_AddressableSceneNamesToSceneAssets == null)
                {
                    m_AddressableSceneNamesToSceneAssets = new Dictionary<string, AddressableSceneAsset>();
                    for (var i = 0; i < m_SceneNames.Count; ++i)
                    {
                        var sceneName = m_SceneNames[i];
                        var sceneAsset = m_SceneAssets[i];
                        m_AddressableSceneNamesToSceneAssets.Add(sceneName, sceneAsset);
                    }
                }
                return m_AddressableSceneNamesToSceneAssets;
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

        public List<AddressableSceneAsset> GetPlatformScenesAssets(BuildTargetRuntime buildTarget)
        {
            List<AddressableSceneAsset> result = new List<AddressableSceneAsset>();
            foreach (var platformsConfiguration in Platforms.Where(pc => pc.BuildTargets.Contains(buildTarget)))
            {
                result.AddRange(platformsConfiguration.Scenes.Where(sa => sa != null && !string.IsNullOrEmpty(sa.Guid)));
            }

            return result;
        }
        
        public List<AddressableSceneAsset> GetAllScenesForPlatform(BuildTargetRuntime target)
        {
            List<AddressableSceneAsset> result = new List<AddressableSceneAsset>();
            IEnumerable<AddressableSceneAsset> defaultScenes = DefaultScenes.Where(sa =>  sa != null && !string.IsNullOrEmpty(sa.Guid));
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

        public List<AddressableSceneAsset> GetAddressableSceneAssets(BuildTargetRuntime target)
        {
            var addressablesMode = GetAddressablesModeForPlatform(target);
            switch (addressablesMode)
            {
                case PlatformAddressablesMode.UsePerSceneSettings:
                    return GetAllScenesForPlatform(target).Where(sa => sa.Addressable).ToList();
                    break;
                case PlatformAddressablesMode.AllScenesAreNonAddressables:
                    return new List<AddressableSceneAsset>();
                    break;
                case PlatformAddressablesMode.AllScenesAreAddressable:
                    return GetAllScenesForPlatform(target).Skip(1).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /*
         * Alias for GetSceneAssetsForBuild.
         * It is because both names (GetNonAddressableSceneAssets and GetSceneAssetsForBuild) are not obvious in some cases, so it is questionary which name is better
         */
        public List<AddressableSceneAsset> GetNonAddressableSceneAssets(BuildTargetRuntime target)  
        {
            return GetSceneAssetsToIncludeInBuild(target);
        }
        
        public List<AddressableSceneAsset> GetSceneAssetsToIncludeInBuild(BuildTargetRuntime target)
        {
            var addressablesMode = GetAddressablesModeForPlatform(target);
            switch (addressablesMode)
            {
                case PlatformAddressablesMode.UsePerSceneSettings:
                    return GetAllScenesForPlatform(target).Where(sa => !sa.Addressable).ToList();
                    break;
                case PlatformAddressablesMode.AllScenesAreNonAddressables:
                    return GetAllScenesForPlatform(target);
                    break;
                case PlatformAddressablesMode.AllScenesAreAddressable:
                    return GetAllScenesForPlatform(target).Take(1).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
