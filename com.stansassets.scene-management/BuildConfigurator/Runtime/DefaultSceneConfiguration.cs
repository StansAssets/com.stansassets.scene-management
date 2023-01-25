using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Serialization;

namespace StansAssets.SceneManagement.Build
{
    /// <summary>
    /// Build configurations for default scenes
    /// </summary>
    [Serializable]
    public class DefaultSceneConfiguration
    {
        public BuildTargetGroup BuildTargetGroup;
        public List<SceneAssetInfo> Scenes = new List<SceneAssetInfo>();

        public DefaultSceneConfiguration(int buildTargets, SceneAssetInfo sceneAssetInfo)
        { 
            BuildTargetGroup = (BuildTargetGroup)buildTargets;
            Scenes.Add(sceneAssetInfo);
        }

        public DefaultSceneConfiguration(BuildTargetGroup buildTargets, SceneAssetInfo sceneAssetInfo)
        { 
            BuildTargetGroup = buildTargets;
            Scenes.Add(sceneAssetInfo);
        }

        public DefaultSceneConfiguration(int buildTargets)
        {
            BuildTargetGroup = (BuildTargetGroup)buildTargets;
        }
    }
}