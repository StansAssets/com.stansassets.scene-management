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
        public List<AddressableSceneAsset> DefaultScenes = new List<AddressableSceneAsset>();
        public List<PlatformsConfiguration> Platforms = new List<PlatformsConfiguration>();

        public bool IsEmpty => DefaultScenes.Count == 0 && Platforms.Count == 0;

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

        Dictionary<string, AddressableSceneAsset> m_AddressableSceneNamesToSceneAssets;
        [SerializeField] List<string> m_SceneNames = new List<string>();
        [SerializeField] List<AddressableSceneAsset> m_SceneAssets = new List<AddressableSceneAsset>();

        internal void SetScenesConfig(List<string> sceneNames, List<AddressableSceneAsset> sceneAssets)
        {
            m_AddressableSceneNamesToSceneAssets = null;
            m_SceneNames = sceneNames;
            m_SceneAssets = sceneAssets;
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
    }
}
