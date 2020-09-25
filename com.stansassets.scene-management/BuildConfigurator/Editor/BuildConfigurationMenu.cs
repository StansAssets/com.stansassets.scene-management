using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    public static class BuildConfigurationMenu
    {
        [MenuItem(SceneManagementPackage.RootMenu + "Build Settings", false, 1)]
        public static void OpenBuildSettings() {
            BuildConfigurationWindow.ShowTowardsInspector("Build Conf");
        }
    }
}
