using System;
using System.Collections.Generic;
using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    [Serializable]
    public class BuildConfiguration
    {
        public string Name = string.Empty;
        public bool DefaultScenesFirst = false;
        public List<SceneAsset> DefaultScenes = new List<SceneAsset>();
        public List<PlatformsConfiguration> Platforms = new List<PlatformsConfiguration>();

        public bool IsEmpty => DefaultScenes.Count == 0 && Platforms.Count == 0;

        public BuildConfiguration Copy()
        {
            var copy = new BuildConfiguration();
            copy.Name = Name + " Copy";
            foreach (var scene in DefaultScenes)
            {
                copy.DefaultScenes.Add(scene);
            }

            foreach (var platformsConfiguration in Platforms)
            {
                var p = new PlatformsConfiguration();
                foreach (var target in platformsConfiguration.BuildTargets)
                {
                    p.BuildTargets.Add(target);
                }

                foreach (var scene in platformsConfiguration.Scenes)
                {
                    p.Scenes.Add(scene);
                }

                copy.Platforms.Add(p);
            }

            return copy;
        }
    }
}
