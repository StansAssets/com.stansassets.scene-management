using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StansAssets.SceneManagement
{
    static class SceneManagementEditorMenu
    {
        const string k_RootMenu = PackagesConfigEditor.RootMenu + "/" + SceneManagementPackage.DisplayName + "/";

        [MenuItem(k_RootMenu + "Settings", false, 0)]
        public static void OpenSettings()
        {
            var windowTitle = SceneManagementSettingsWindow.WindowTitle;
            SceneManagementSettingsWindow.ShowTowardsInspector(windowTitle.text, windowTitle.image);
        }

        [MenuItem(k_RootMenu + "Start Landing &p", false, 1000)]
        public static void PlayMode()
        {
            StartLandingAction.Execute();
        }
    }
}
