#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace StansAssets.ResourceManager
{
    public static class ResourceManagerRuntime
    {
        /// <summary>
        /// Load the object by its full name, for example: "group-name/object-name".
        /// If you pass "object-name" with no group then it will be searched in all groups.
        /// </summary>
        /// <param name="objectFullName">Object name (original or display),
        /// can be combined with Group name (original or display) in format: "group/object"</param>
        /// <param name="result">A callback action that will return T (as a result) and a bool indicating success</param>
        /// <typeparam name="T">The type of resource you are looking for</typeparam>
        public static void LoadObject<T>(string objectFullName, Action<T, bool> result) where T : Object
        {
            var path = ResourceManagerUtilities.SplitObjectPath(objectFullName);

            LoadObject(path.objectName, path.groupName, result);
        }

        /// <summary>
        /// Load the object by its name in specific group.
        /// If you pass empty group name then it will be searched in all groups.
        /// </summary>
        /// <param name="objectName">Object name (original or display)</param>
        /// <param name="groupName">Group name (original or display)</param>
        /// <param name="result">A callback action that will return T (as a result) and a bool indicating success</param>
        /// <typeparam name="T">The type of resource you are looking for</typeparam>
        public static void LoadObject<T>(string objectName, string groupName, Action<T, bool> result) where T : Object
        {
            ResourceItem item = null;

            var comparableGroupName = groupName.ToLower();
            if (string.IsNullOrEmpty(comparableGroupName))
            {
                foreach (var resourceGroup in ResourceManagerData.Instance.ResourceGroups)
                {
                    item = resourceGroup.Resources.FirstOrDefault(i => CompareResourceItem(i, objectName));

                    if (item != null)
                        break;
                }
            }
            else
            {
                var group = ResourceManagerData.Instance.ResourceGroups
                    .FirstOrDefault(g =>
                        g.Name.ToLower().Equals(comparableGroupName)
                        || g.DisplayName.ToLower().Equals(comparableGroupName)
                    );
                
                if (group == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Resource group '{groupName}' not found!");
#endif
                }
                else
                {
                    item = group.Resources.FirstOrDefault(i => CompareResourceItem(i, objectName));
                }
            }

            if (item == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Resource item '{objectName}' not found in '{groupName}' group!");
#endif
                result.Invoke(null, false);
            }
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(item.Guid);

                if (item.Addressable)
                {
                    var operationHandle = Addressables.LoadAssetAsync<T>(path);
                    operationHandle.Completed += handle =>
                    {
                        result.Invoke(handle.Result, handle.Status == AsyncOperationStatus.Succeeded);
                    };
                }
                else
                {
                    var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                    result.Invoke(asset, asset != null);
                }
            }
            
            bool CompareResourceItem(ResourceItem i, string n)
            {
                n = n.ToLower();
                return i.DisplayName.ToLower().Equals(n)
                       || i.FileName.ToLower().Equals(n);
            }
        }
    }
}