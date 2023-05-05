#if UNITY_2019_4_OR_NEWER

using System.Linq;
using StansAssets.Plugins.Editor;
using UnityEngine.UIElements;

namespace StansAssets.ResourceManager.Editor
{
    class ResourceManagerTab : BaseTab
    {
        internal ResourceManagerTab()
            : base($"{ResourceManagerEditorConfig.UIToolkitPath}/ResourceManagerTab/ResourceManagerTab")
        {
            var resourcesList = Root.Q<VisualElement>("resources-list");

            var resourceGroups = ResourceManagerData.Instance.ResourceGroups;
            if (!resourceGroups.Any())
            {
                resourceGroups.Add(ResourceManagerData.Instance.DefaultGroup);
            }

            foreach (var resourceGroup in resourceGroups)
            {
                BindResourceGroup(resourceGroup, resourcesList);
            }

            var addButton = Root.Q<Button>("add-group-btn");
            addButton.clicked += () =>
            {
                AddGroupWindow.ShowDialog("Resource group", groupName =>
                {
                    if (string.IsNullOrEmpty(groupName)) return;

                    var resourceGroup = new ResourceGroup
                    {
                        Name = groupName,
                        DisplayName = groupName,
                    };

                    BindResourceGroup(resourceGroup, resourcesList);

                    ResourceManagerData.Instance.ResourceGroups.Add(resourceGroup);
                    ResourceManagerData.Save();
                });
            };

            var buildButton = Root.Q<Button>("build-addressables-btn");
            buildButton.clicked += ResourceManagerEditor.SetupBuildOptions;
        }

        void BindResourceGroup(ResourceGroup resourceGroup, VisualElement resourcesList)
        {
            var group = new ListGroup(resourcesList, resourceGroup);
            group.Removed += OnRemoveResourceGroup;

            resourcesList.Add(group);
        }

        void OnRemoveResourceGroup(ResourceGroup resourceGroup)
        {
            ResourceManagerData.Instance.ResourceGroups.Remove(resourceGroup);
            ResourceManagerData.Save();
        }
    }
}

#endif