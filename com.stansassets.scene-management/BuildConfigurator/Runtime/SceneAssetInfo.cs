using System;

namespace StansAssets.SceneManagement.Build
{
    /// <summary>
    /// Class that contains serialized scene info
    /// </summary>
    [Serializable]
    public class SceneAssetInfo
    {
        public string Name;
        public string Guid;
        public bool Addressable;
        public SceneAssetInfo() { }

        public SceneAssetInfo(string name, string guid, bool addressable)
        {
            Name = name;
            Guid = guid;
            Addressable = addressable;
        }
    }
}
