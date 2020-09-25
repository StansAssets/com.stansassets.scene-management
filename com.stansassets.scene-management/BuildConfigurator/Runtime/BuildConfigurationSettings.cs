using System.Collections.Generic;
using StansAssets.Plugins;

namespace StansAssets.SceneManagement.Build
{
    /// <summary>
    /// Build settings
    /// </summary>
    public class BuildConfigurationSettings :  PackageScriptableSettingsSingleton<BuildConfigurationSettings>
    {
        public override string PackageName => "com.stansassets.scene-management";

        public int ActiveConfigurationIndex = 0;
        public List<BuildConfiguration> BuildConfigurations = new List<BuildConfiguration>();

        public BuildConfiguration Configuration => ActiveConfigurationIndex >= BuildConfigurations.Count
            ? new BuildConfiguration()
            : BuildConfigurations[ActiveConfigurationIndex];
    }
}