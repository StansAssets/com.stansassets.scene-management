using JetBrains.Annotations;
using StansAssets.Foundation.Editor;
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace StansAssets.SceneManagement
{
    /*[UsedImplicitly]
    class SceneManagementProjectSettings : PackagePreferencesWindow
    {
        protected override PackageInfo GetPackageInfo()
            => PackageManagerUtility.GetPackageInfo(SceneManagementSettings.Instance.PackageName);

        protected override string SettingsPath => $"{PluginsDevKitPackage.RootMenu}/{GetPackageInfo().displayName}";
        protected override SettingsScope Scope => SettingsScope.Project;
        protected override void OnActivate(string searchContext, VisualElement rootElement)
        {
            ContentContainerFlexGrow(1);
            AddTab("Settings", new SettingsTab());
            AddTab("About", new AboutTab());
        }

        protected override void OnDeactivate() { }
    }*/
}
