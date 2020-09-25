using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    static class AddressableSceneAssetExtension
    {
        public static SceneAsset GetSceneAsset(this AddressableSceneAsset sceneAsset)
        {
            if (string.IsNullOrEmpty(sceneAsset.Guid))
            {
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(sceneAsset.Guid);
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            return scene;
        }

        public static void SetSceneAsset(this AddressableSceneAsset addressableSceneAsset, SceneAsset sceneAsset)
        {
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            if (string.IsNullOrEmpty(path) == false)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                addressableSceneAsset.Guid = guid;
            }
        }
    }
}
