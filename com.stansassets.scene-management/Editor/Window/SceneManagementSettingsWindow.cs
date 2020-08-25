#if UNITY_2019_4_OR_NEWER
using StansAssets.Foundation.Editor;
using StansAssets.Plugins.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace StansAssets.SceneManagement
{
    public class SceneManagementSettingsWindow : PackageSettingsWindow<SceneManagementSettingsWindow>
    {
        protected override PackageInfo GetPackageInfo()
            => PackageManagerUtility.GetPackageInfo(SceneManagementSettings.Instance.PackageName);

        protected override void OnWindowEnable(VisualElement root)
        {
            AddTab("Settings", new SettingsTab());
            AddTab("About", new AboutTab());
        }

        public static GUIContent WindowTitle => new GUIContent(SceneManagementPackage.DisplayName);
    }
}
#endif