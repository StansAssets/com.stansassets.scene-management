using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    public static class BuildConfigurationMenu
    {
        private static BuildConfigurationWindow m_Window;

        [MenuItem(SceneManagementPackage.RootMenu + "Build Settings", false, 1)]
        public static void OpenBuildSettings()
        {
            if (m_Window == null)
            {
                m_Window = BuildConfigurationWindow.ShowTowardsInspector("Build Conf");
            }

            m_Window.Show();
        }
    }
}