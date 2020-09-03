﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StansAssets.SceneManagement.Build
{
    [Serializable]
    public class BuildConfiguration
    {
        public string Name = string.Empty;
        public bool DefaultScenesFirst = false;
        public List<AddressableSceneAsset> DefaultScenes = new List<AddressableSceneAsset>();
        public List<PlatformsConfiguration> Platforms = new List<PlatformsConfiguration>();

        public bool IsEmpty => DefaultScenes.Count == 0 && Platforms.Count == 0;

        public BuildConfiguration Copy()
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

        public bool IsSceneAddressable(string sceneName)
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
        #if UNITY_EDITOR
        public void InitializeBuildData(BuildTargetRuntime builtTarget)
        {
            m_AddressableSceneNamesToSceneAssets = null;
            m_SceneNames.Clear();
            m_SceneAssets.Clear();

            foreach (var scene in DefaultScenes)
            {
                if (scene.Addressable)
                {
                    string path = AssetDatabase.GUIDToAssetPath(scene.Guid);
                    if (string.IsNullOrEmpty(path))
                        continue;

                    m_SceneNames.Add(Path.GetFileNameWithoutExtension(path));
                    m_SceneAssets.Add(scene);
                }
            }

            foreach (var platform in Platforms)
            {
                if (platform.BuildTargets.Contains(builtTarget))
                {
                    foreach (var scene in platform.Scenes)
                    {
                        if (scene.Addressable)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(scene.Guid);
                            if (string.IsNullOrEmpty(path))
                                continue;

                            m_SceneNames.Add(Path.GetFileNameWithoutExtension(path));
                            m_SceneAssets.Add(scene);
                        }
                    }
                }
            }

            Debug.Log("Addressable Scenes: " + m_SceneNames.Count);
            Debug.Log("Addressable Scenes List: " + string.Join("\n", m_SceneNames));
        }
        #endif

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