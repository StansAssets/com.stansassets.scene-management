using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    public static class BuildConfigurationMenu
    {
        static BuildConfigurationWindow s_Window;

        [MenuItem(SceneManagementPackage.RootMenu + "Build Settings", false, 1)]
        public static void OpenBuildSettings()
        {
            if (s_Window == null)
            {
                s_Window = BuildConfigurationWindow.ShowTowardsInspector("Build Conf");
            }

            s_Window.Show();
        }
    }
}