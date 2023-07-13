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
        public BuildTargetGroupRuntime BuildTargetGroup;
        public List<SceneAssetInfo> Scenes = new List<SceneAssetInfo>();
        public bool Override;

        public DefaultScenesConfiguration(int buildTargets, SceneAssetInfo sceneAssetInfo)
        { 
            BuildTargetGroup = (BuildTargetGroupRuntime)buildTargets;
            Scenes.Add(sceneAssetInfo);
        }

        public DefaultScenesConfiguration(BuildTargetGroupRuntime buildTargets, SceneAssetInfo sceneAssetInfo)
        { 
            BuildTargetGroup = buildTargets;
            Scenes.Add(sceneAssetInfo);
        }

        public DefaultScenesConfiguration(int buildTargets)
        {
            BuildTargetGroup = (BuildTargetGroupRuntime)buildTargets;
        }
    }
}
