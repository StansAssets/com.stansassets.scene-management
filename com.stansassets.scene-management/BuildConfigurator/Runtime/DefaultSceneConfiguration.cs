using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement.Build
{
    /// <summary>
    /// Build configurations for default scenes
    /// </summary>
    [Serializable]
    public class DefaultSceneConfiguration
    {
        public BuildTargetGroupRuntime BuildTargetGroup;
        public List<SceneAssetInfo> Scenes = new List<SceneAssetInfo>();

        public DefaultSceneConfiguration(int buildTargets, SceneAssetInfo sceneAssetInfo)
        { 
            BuildTargetGroup = (BuildTargetGroupRuntime)buildTargets;
            Scenes.Add(sceneAssetInfo);
        }

        public DefaultSceneConfiguration(BuildTargetGroupRuntime buildTargets, SceneAssetInfo sceneAssetInfo)
        { 
            BuildTargetGroup = buildTargets;
            Scenes.Add(sceneAssetInfo);
        }

        public DefaultSceneConfiguration(int buildTargets)
        {
            BuildTargetGroup = (BuildTargetGroupRuntime)buildTargets;
        }
    }
}