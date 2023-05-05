#if UNITY_2019_4_OR_NEWER

using System;
using UnityEditor;
using UnityEngine;

namespace StansAssets.ResourceManager.Editor
{
    class AddGroupWindow : EditorWindow
    {
        internal event Action<string> Confirmed;

        void OnEnable()
        {
            var tab = new AddGroupTab();
            tab.Complete += OnConfirmed;

            rootVisualElement.Add(tab);
        }

        void OnConfirmed(string obj)
        {
            Confirmed?.Invoke(obj);
        }

        internal static void ShowDialog(string label, Action<string> confirmed)
        {
            var dialog = CreateInstance<AddGroupWindow>();

            var windowSize = new Vector2(350, 280);
            dialog.minSize = windowSize;
            dialog.maxSize = windowSize;
            dialog.titleContent.text = label;

            dialog.Confirmed += value => dialog.Close();
            dialog.Confirmed += confirmed;

            dialog.ShowUtility();
        }
    }
}

#endif