using UnityEngine.SceneManagement;

namespace StansAssets.SceneManagement
{
    /// <summary>
    /// The info about the scene.
    /// This alternative was created since <see cref="Scene"/> is not always has valid info
    /// specially when it was loaded / unloaded using addressable.
    /// </summary>
    public class SceneInfo
    {
        public string Name { get; }

        internal SceneInfo(string name) {
            Name = name;
        }
    }
}
