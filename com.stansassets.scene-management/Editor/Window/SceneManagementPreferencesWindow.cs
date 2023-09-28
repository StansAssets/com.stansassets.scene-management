using JetBrains.Annotations;
using StansAssets.Foundation.Editor;
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace StansAssets.SceneManagement
{
    [UsedImplicitly]
    sealed class SceneManagementPreferencesWindow : PackagePreferencesWindow
    {
        protected override PackageInfo GetPackageInfo()
            => PackageManagerUtility.GetPackageInfo(SceneManagementSettings.Instance.PackageName);

        protected override string SettingsPath => $"{PluginsDevKitPackage.RootMenu}/{GetPackageInfo().displayName}";
        protected override SettingsScope Scope => SettingsScope.Project;

        protected override void OnActivate(string searchContext, VisualElement rootElement)
        {
            ContentContainerFlexGrow(1);
            AddTab("Settings", new SettingsTab());
        }

        protected override void OnDeactivate() { }
    }
}
