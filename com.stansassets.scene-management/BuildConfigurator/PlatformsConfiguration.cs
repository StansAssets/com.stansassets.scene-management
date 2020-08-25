using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    [Serializable]
    public class PlatformsConfiguration
    {
        public List<BuildTarget> BuildTargets = new List<BuildTarget>();
        public List<SceneAsset> Scenes = new List<SceneAsset>();
    }
}