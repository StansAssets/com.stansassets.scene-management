using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement.Build
{
    /// <summary>
    /// Build configurations for different platforms
    /// </summary>
    [Serializable]
    public class PlatformsConfiguration
    {
        public List<BuildTargetRuntime> BuildTargets = new List<BuildTargetRuntime>();
        public List<SceneAssetInfo> Scenes = new List<SceneAssetInfo>();

        public bool IsEmpty => BuildTargets.Count == 0 && Scenes.Count == 0;
    }
}
