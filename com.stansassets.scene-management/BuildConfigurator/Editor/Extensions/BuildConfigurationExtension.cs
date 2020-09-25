using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    static class BuildConfigurationExtension
    {
        public static List<SceneAsset> GetAddressableDefaultScenes(this BuildConfiguration platformsConfiguration)
        {
            return platformsConfiguration.DefaultScenes.Where(scene => scene.GetSceneAsset() != null && scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        public static List<SceneAsset> GetNonAddressableDefaultScenes(this BuildConfiguration platformsConfiguration)
        {
            return platformsConfiguration.DefaultScenes.Where(scene => scene.GetSceneAsset() != null && !scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        public static void InitializeBuildData(this BuildConfiguration platformsConfiguration, BuildTargetRuntime builtTarget)
        {
            var sceneNames =  new List<string>();
            var sceneAssets = new List<AddressableSceneAsset>();

            foreach (var scene in platformsConfiguration.DefaultScenes)
            {
                if (scene.Addressable)
                {
                    string path = AssetDatabase.GUIDToAssetPath(scene.Guid);
                    if (string.IsNullOrEmpty(path))
                        continue;

                    sceneNames.Add(Path.GetFileNameWithoutExtension(path));
                    sceneAssets.Add(scene);
                }
            }

            foreach (var platform in platformsConfiguration.Platforms)
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

                            sceneNames.Add(Path.GetFileNameWithoutExtension(path));
                            sceneAssets.Add(scene);
                        }
                    }
                }
            }

            Debug.Log("Addressable Scenes: " + sceneNames.Count);
            Debug.Log($"Addressable Scenes List:\n{string.Join("\n", sceneAssets.Select(asset => AssetDatabase.GUIDToAssetPath(asset.Guid)))}");
            platformsConfiguration.SetScenesConfig(sceneNames, sceneAssets);
        }
    }
}
