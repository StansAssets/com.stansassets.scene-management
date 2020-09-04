using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    static class PlatformsConfigurationExtension
    {
        public static List<SceneAsset> GetAddressableScenes(this PlatformsConfiguration platformsConfiguration)
        {
            return platformsConfiguration.Scenes.Where(scene => scene.GetSceneAsset() != null && scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        public static List<SceneAsset> GetNonAddressableScenes(this PlatformsConfiguration platformsConfiguration)
        {
            return platformsConfiguration.Scenes.Where(scene => scene.GetSceneAsset() != null && !scene.Addressable).Select(addressableScene => addressableScene.GetSceneAsset()).ToList();
        }

        public static List<BuildTarget> GetBuildTargetsEditor(this PlatformsConfiguration platformsConfiguration)
        {
            return platformsConfiguration.BuildTargets.Select(bt => (BuildTarget)bt).ToList();
        }
    }
}
