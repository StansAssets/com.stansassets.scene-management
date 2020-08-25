#if UNITY_2019_4_OR_NEWER
using StansAssets.Plugins.Editor;
using UnityEditor;

namespace StansAssets.SceneManagement
{
    static class SceneManagementEditorMenu
    {

        [MenuItem(SceneManagementPackage.RootMenu + "Settings", false, 0)]
        public static void OpenSettings()
        {
            var windowTitle = SceneManagementSettingsWindow.WindowTitle;
            SceneManagementSettingsWindow.ShowTowardsInspector(windowTitle.text, windowTitle.image);
        }


        [MenuItem(SceneManagementPackage.RootMenu + "Start Landing &p", false, 1000)]
        public static void PlayMode()
        {
            StartLandingAction.Execute();
        }
    }
}
#endif
