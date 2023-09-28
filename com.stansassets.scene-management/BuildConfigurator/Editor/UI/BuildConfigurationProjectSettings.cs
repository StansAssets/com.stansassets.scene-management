using JetBrains.Annotations;
using StansAssets.Foundation.Editor;
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace StansAssets.SceneManagement.Build
{
    /*[UsedImplicitly]
    class BuildConfigurationProjectSettings : PackagePreferencesWindow
    {
        BuildConfigurationView m_View;
        
        protected override PackageInfo GetPackageInfo()
            => PackageManagerUtility.GetPackageInfo(SceneManagementSettings.Instance.PackageName);

        protected override string SettingsPath => $"{PluginsDevKitPackage.RootMenu}/Build Configuration";
        protected override SettingsScope Scope => SettingsScope.Project;
        protected override void OnActivate(string searchContext, VisualElement rootElement)
        {
            ContentContainerFlexGrow(1);
            
            // m_View = new BuildConfigurationView()
            rootElement.Add(new IMGUIContainer());
        }

        protected override void OnDeactivate() { }
    }*/
}
