#if UNITY_2019_4_OR_NEWER

using System;
using System.Linq;
using StansAssets.Foundation.Editor;
using StansAssets.Plugins.Editor;
using UnityEngine.UIElements;

namespace StansAssets.ResourceManager.Editor
{
    class AddGroupTab : BaseTab
    {
        internal event Action<string> Complete;

        internal AddGroupTab()
            : base($"{ResourceManagerEditorConfig.UIToolkitPath}/AddGroupTab/AddGroupTab")
        {
            var groupName = "";
            
            var nameTextField = Root.Q<TextField>("name-tf");
            var searchTextField = Root.Q<TextField>("search-tf");
            var packagesListView = Root.Q<ListView>("packages-list");
            var addButton = Root.Q<Button>("add-btn");
            var selectButton = Root.Q<Button>("select-btn");

            // Packages list
            var manifest = new Manifest();
            manifest.Fetch();
            var dependencies = manifest.GetDependencies().ToList();

            packagesListView.makeItem += () =>
            {
                var label = new Label();
                label.AddToClassList("packages-item");
                return label;
            };
            packagesListView.bindItem += (element, i) =>
            {
                var item = packagesListView.itemsSource[i] as Dependency;
                var label = element.Q<Label>();
                label.text = $"{item?.Name ?? "Null"}";
            };

            packagesListView.itemHeight = 18;
            packagesListView.style.height = 155;
            packagesListView.itemsSource = dependencies;

            // Name field
            nameTextField.RegisterValueChangedCallback(evt =>
            {
                groupName = evt.newValue;
            });
            
            // Search field
            searchTextField.RegisterValueChangedCallback(evt =>
            {
                packagesListView.itemsSource = string.IsNullOrEmpty(evt.newValue)
                    ? dependencies
                    : dependencies.FindAll(i => i.Name.ToLower().Contains(evt.newValue.ToLower()));
                packagesListView.RebuildInCompatibleMode();
            });

            // Select button
            selectButton.clicked += () =>
            {
                if (packagesListView.selectedItem != null)
                {
                    OnComplete((packagesListView.selectedItem as Dependency)?.Name);
                }
            };

            // Add button
            addButton.clicked += () =>
            {
                if (!string.IsNullOrEmpty(groupName))
                {
                    OnComplete(groupName);
                }
            };
        }

        void OnComplete(string group)
        {
            Complete?.Invoke(group);
        }
    }
}

#endif