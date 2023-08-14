using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement.Build
{
    /// <summary>
    /// Build configurations for default scenes
    /// </summary>
    [Serializable]
    public class DefaultScenesConfiguration
    {
        public int BuildTargetGroup;
        public List<SceneAssetInfo> Scenes = new();
        public bool Override;

        public DefaultScenesConfiguration(int buildTarget, SceneAssetInfo sceneAssetInfo)
        { 
            BuildTargetGroup = buildTarget;
            Scenes.Add(sceneAssetInfo);
        }

        public DefaultScenesConfiguration(int buildTarget)
        {
            BuildTargetGroup = buildTarget;
        }
    }
}
