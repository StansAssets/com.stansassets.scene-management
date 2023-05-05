#if UNITY_2019_4_OR_NEWER

using System;
using StansAssets.Plugins.Editor;
using UnityEngine.UIElements;

namespace StansAssets.ResourceManager.Editor
{
    class ListGroup : BaseTab
    {
        internal event Action<ResourceGroup> Removed;

        internal ListGroup(VisualElement parentContainer, ResourceGroup resourceGroup)
            : base($"{ResourceManagerEditorConfig.UIToolkitPath}/ResourceManagerTab/ListGroup/ListGroup")
        {
            var groupName = Root.Q<TextField>("group-name");
            groupName.value = "";

            var groupOriginalName = Root.Q<Label>("group-original-name");
            groupOriginalName.text = "";

            var groupList = Root.Q<ListView>("group-elements-list");
            var listFoldout = Root.Q<Foldout>("list-foldout");

            var addButton = Root.Q<Button>("add-btn");
            var removeButton = Root.Q<Button>("remove-group-btn");

            // Display Name
            groupName.value = resourceGroup.DisplayName;
            groupName.RegisterValueChangedCallback(evt =>
            {
                resourceGroup.DisplayName = evt.newValue;
            });

            // Original Name
            groupOriginalName.text = resourceGroup.Name;

            // Resources List
            groupList.makeItem += () =>
            {
                var item = new ListItem();
                return item;
            };
            groupList.bindItem += (element, i) =>
            {
                var item = element.Q<ListItem>();
                item.Bind(groupList.itemsSource[i] as ResourceItem);
                item.OnRemove(() =>
                {
                    groupList.itemsSource.RemoveAt(i);
                    RefreshList(groupList);
                });
            };

            groupList.itemHeight = ListItem.ItemHeight;
            groupList.style.height = resourceGroup.Resources.Count * ListItem.ItemHeight;
            groupList.itemsSource = resourceGroup.Resources;

            // Foldout
            SetToggleHeight(groupList);

            listFoldout.RegisterValueChangedCallback(evt =>
            {
                SetToggleHeight(groupList);
            });

            // Add button
            addButton.clicked += () =>
            {
                var group = new ResourceItem();
                groupList.itemsSource.Add(group);
                RefreshList(groupList);
            };

            // Remove button
            var defaultGroupName = ResourceManagerData.Instance.DefaultGroup.Name;
            removeButton.style.display = resourceGroup.Name.Equals(defaultGroupName)
                ? DisplayStyle.None
                : DisplayStyle.Flex;
            removeButton.clicked += () =>
            {
                parentContainer.Remove(this);
                Removed?.Invoke(resourceGroup);
            };
        }

        void SetToggleHeight(ListView list)
        {
            list.style.height = ListItem.ItemHeight * list.itemsSource.Count;
        }

        void RefreshList(ListView list)
        {
            list.Refresh();

            SetToggleHeight(list);
            ResourceManagerData.Save();
        }
    }
}

#endif