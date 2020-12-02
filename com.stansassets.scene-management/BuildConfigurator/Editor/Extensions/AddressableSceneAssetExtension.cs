using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    static class AddressableSceneAssetExtension
    {
        public static SceneAsset GetSceneAsset(this SceneAssetInfo sceneAssetInfo)
        {
            if (string.IsNullOrEmpty(sceneAssetInfo.Guid))
            {
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(sceneAssetInfo.Guid);
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            return scene;
        }

        public static void SetSceneAsset(this SceneAssetInfo sceneAssetInfo, SceneAsset sceneAsset)
        {
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            if (string.IsNullOrEmpty(path) == false)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                sceneAssetInfo.Guid = guid;
            }
            else
            {
                sceneAssetInfo.Guid = string.Empty;
            }
        }

        public static void SetSceneAsset(this SceneAssetInfo sceneAssetInfo, EditorBuildSettingsScene sceneAsset)
        {
            var path = sceneAsset.path;
            if (string.IsNullOrEmpty(path) == false)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                sceneAssetInfo.Guid = guid;
            }
            else
            {
                sceneAssetInfo.Guid = string.Empty;
            }
        }
    }
}
