using System;

namespace StansAssets.SceneManagement.Build
{
    /// <summary>
    /// Class that indicates is scene addressable
    /// </summary>
    [Serializable]
    public class AddressableSceneAsset
    {
        public string Guid;
        public bool Addressable;
    }
}
