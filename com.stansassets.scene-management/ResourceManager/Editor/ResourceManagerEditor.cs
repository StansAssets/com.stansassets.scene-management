using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;

namespace StansAssets.ResourceManager.Editor
{
    [InitializeOnLoad]
    public class ResourceManagerEditor
    {
        public const string ResourcesAddressablesGroupName = "ResourceManagerAssets";

        static ResourceManagerEditor()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(options =>
            {
                SetupBuildOptions();

                EditorApplication.delayCall += () =>
                {
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
                };
            });
        }

        internal static void SetupBuildOptions()
        {
            var addressableAssetGroup = ResourceManagerEditorUtilities.GetOrCreateGroup(ResourcesAddressablesGroupName);
            var resourceGroups = ResourceManagerData.Instance.ResourceGroups
                .SelectMany(i => i.Resources).Where(r => r.Addressable);

            foreach (var resourceItem in resourceGroups)
            {
                var sceneAssetPath = AssetDatabase.GUIDToAssetPath(resourceItem.Guid);
                if (string.IsNullOrEmpty(sceneAssetPath))
                    continue;

                var guid = AssetDatabase.AssetPathToGUID(sceneAssetPath);
                var entry = AddressableAssetSettingsDefaultObject.Settings
                    .CreateOrMoveEntry(guid, addressableAssetGroup, false, true);
                entry.address = resourceItem.FileName;
            }
        }
    }
}