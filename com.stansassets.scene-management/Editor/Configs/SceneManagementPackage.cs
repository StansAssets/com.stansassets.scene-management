using StansAssets.Foundation.Editor;
using StansAssets.Plugins.Editor;

namespace StansAssets.SceneManagement
{
    static class SceneManagementPackage
    {
        public const string PackageName = "com.stansassets.scene-management";
        public const string DisplayName = "Scene Management";
        public const string RootMenu = PluginsDevKitPackage.RootMenu + "/" + DisplayName + "/";

        public static readonly string RootPath = PackageManagerUtility.GetPackageRootPath(PackageName);

#if UNITY_2019_4_OR_NEWER
        public static readonly UnityEditor.PackageManager.PackageInfo Info = PackageManagerUtility.GetPackageInfo(PackageName);
#endif

        internal static readonly string WindowTabsPath = $"{RootPath}/Editor/Window/Tabs";
    }
}
