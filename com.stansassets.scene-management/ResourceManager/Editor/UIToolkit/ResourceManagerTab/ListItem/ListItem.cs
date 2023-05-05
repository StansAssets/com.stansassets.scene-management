#if UNITY_2019_4_OR_NEWER

using System;
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace StansAssets.ResourceManager.Editor
{
    public class ListItem : BaseTab
    {
        internal const int ItemHeight = 48;

        readonly TextField m_Name;
        readonly Toggle m_Addressable;
        readonly ObjectField m_ObjectField;
        readonly Button m_RemoveButton;

        internal ListItem()
            : base($"{ResourceManagerEditorConfig.UIToolkitPath}/ResourceManagerTab/ListItem/ListItem")
        {
            m_Name = Root.Q<TextField>("item-name");

            m_Addressable = Root.Q<Toggle>("addressable-tgl");
            m_Addressable.labelElement.style.display = DisplayStyle.None;

            m_ObjectField = Root.Q<ObjectField>("obj-field");
            m_ObjectField.objectType = typeof(Object);
            m_ObjectField.allowSceneObjects = false;
            
            m_RemoveButton = Root.Q<Button>("rmv-btn");
        }

        internal void Bind(ResourceItem resourceItem)
        {
            m_Name.RegisterValueChangedCallback(evt =>
            {
                resourceItem.DisplayName = evt.newValue;
                SaveNRefresh(resourceItem);
            });

            m_Addressable.RegisterValueChangedCallback(evt =>
            {
                resourceItem.Addressable = m_Addressable.value;
                SaveNRefresh(resourceItem);
            });

            m_ObjectField.RegisterValueChangedCallback(evt =>
            {
                resourceItem.ObjectRef = evt.newValue;
                
                if (string.IsNullOrEmpty(resourceItem.DisplayName)
                    // ReSharper disable once Unity.NoNullPropagation
                    || resourceItem.DisplayName.Equals(evt.previousValue?.name ?? ""))
                {
                    resourceItem.DisplayName = resourceItem.ObjectRef.name;
                }

                SaveNRefresh(resourceItem);
            });

            RefreshDisplayInfo(resourceItem);
        }

        internal void OnRemove(Action action)
        {
            m_RemoveButton.clicked += action;
        }

        void RefreshDisplayInfo(ResourceItem resourceItem)
        {
            if (m_Name.value != resourceItem.DisplayName)
            {
                m_Name.SetValueWithoutNotify(resourceItem.DisplayName);
            }

            if (m_Addressable.value != resourceItem.Addressable)
            {
                m_Addressable.SetValueWithoutNotify(resourceItem.Addressable);
            }

            if (m_ObjectField.value != resourceItem.ObjectRef)
            {
                m_ObjectField.SetValueWithoutNotify(resourceItem.ObjectRef);

                if (resourceItem.ObjectRef != null)
                {
                    var assetPath = AssetDatabase.GetAssetPath(resourceItem.ObjectRef);
                    var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                    m_ObjectField.tooltip = $"{assetType}";
                }
                else
                {
                    m_ObjectField.tooltip = $"Empty";
                }
            }
        }

        void SaveNRefresh(ResourceItem resourceItem)
        {
            ResourceManagerData.Save();
            RefreshDisplayInfo(resourceItem);
        }
    }
}

#endif