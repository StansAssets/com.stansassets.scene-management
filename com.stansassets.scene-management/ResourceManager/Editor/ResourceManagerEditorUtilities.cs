using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace StansAssets.ResourceManager.Editor
{
    public static class ResourceManagerEditorUtilities
    {
        public static AddressableAssetGroup GetOrCreateGroup(string name)
        {
            var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup((g) => g.name == name);
            if (group != null) {
                AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(group);
            }

            group = AddressableAssetSettingsDefaultObject.Settings.CreateGroup(name, false, false, true, new List<AddressableAssetGroupSchema>());
            group.ClearSchemas(true, true);
            group.AddSchema<ContentUpdateGroupSchema>();
            var schema = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
            schema.UseAssetBundleCache = false;
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.AppendHash;
            group.AddSchema(schema);

            return group;
        }
    }
}