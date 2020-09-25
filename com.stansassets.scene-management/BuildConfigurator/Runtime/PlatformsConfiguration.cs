using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement.Build
{
    [Serializable]
    class PlatformsConfiguration
    {
        public List<BuildTargetRuntime> BuildTargets = new List<BuildTargetRuntime>();
        public List<AddressableSceneAsset> Scenes = new List<AddressableSceneAsset>();
    }
}