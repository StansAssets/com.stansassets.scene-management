using System.Collections.Generic;
using StansAssets.Plugins;

namespace StansAssets.SceneManagement.Build {

    public class BuildConfigurationSettings :  PackageScriptableSettingsSingleton<BuildConfigurationSettings>
    {
        protected override bool IsEditorOnly => true;
        public override string PackageName => SceneManagementPackage.PackageName;
        
        
        public int ActiveConfigurationIndex = 0;
        public List<BuildConfiguration> BuildConfigurations = new List<BuildConfiguration>();

        public BuildConfiguration Configuration => ActiveConfigurationIndex >= BuildConfigurations.Count 
            ? new BuildConfiguration() 
            : BuildConfigurations[ActiveConfigurationIndex];
    }
}