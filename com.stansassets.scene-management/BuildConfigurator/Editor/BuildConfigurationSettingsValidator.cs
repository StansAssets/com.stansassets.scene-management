using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    public class BuildConfigurationSettingsValidator
    {
        public const string TAG = "[Build Configuration]";

        static BuildConfigurationSettingsValidator() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state) {
            switch (state) {
                case PlayModeStateChange.EnteredPlayMode:
                    // TODO: Do we really need to sync scenes on play mode state changed?
                    // if (BuildConfigurationSettings.Instance.HasValidConfiguration) {
                    //     BuildConfigurationSettings.Instance.Configuration
                    //         .SetupEditorSettings(EditorUserBuildSettings.activeBuildTarget, false);
                    // }
                    break;
            }
        }
    }
}
