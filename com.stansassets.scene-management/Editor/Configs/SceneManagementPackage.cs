using StansAssets.Foundation.Editor;
using UnityEditor.PackageManager;

namespace StansAssets.SceneManagement
{
    static class SceneManagementPackage
    {
        public const string PackageName = "com.stansassets.scene-management";
        public const string DisplayName = "Scene Management";

        public static readonly string RootPath = PackageManagerUtility.GetPackageRootPath(PackageName);
        public static readonly PackageInfo Info = PackageManagerUtility.GetPackageInfo(PackageName);

        internal static readonly string WindowTabsPath = $"{RootPath}/Editor/Window/Tabs";
    }
}
