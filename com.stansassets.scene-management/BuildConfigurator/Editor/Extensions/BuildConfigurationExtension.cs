using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    public static class BuildConfigurationExtension
    {
        public static List<SceneAsset> GetAddressableDefaultScenes(this BuildConfiguration platformsConfiguration)
        {
            return platformsConfiguration.DefaultScenes.Where(scene => scene.GetSceneAsset() != null && scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        public static List<SceneAsset> GetNonAddressableDefaultScenes(this BuildConfiguration platformsConfiguration)
        {
            return platformsConfiguration.DefaultScenes.Where(scene => scene.GetSceneAsset() != null && !scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        // public bool IsAddressable(SceneAs)
    }
}
