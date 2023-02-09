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
                case PlayModeStateChange.ExitingEditMode:
                    PreventOfPlayingOutOfSync();
                    break;
            }
        }

        static void PreventOfPlayingOutOfSync()
        {
            if (!BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog)
            {
                return;
            }

            var outOfSync = EditorBuildSettingsValidator.CompareScenesWithBuildSettings();

            if (!outOfSync)
            {
                return;
            }
            
            var result = EditorUtility.DisplayDialogComplex(
                "Scenes Management",
                EditorBuildSettingsValidator.ScenesSyncWarningDescription,
                "Ok, continue",
                "Cancel, exit playmode",
                "Don't show again");

            switch (result)
            {
                case 0:
                    break;
                case 1:
                    EditorApplication.isPlaying = false;
                    break;
                case 2:
                    BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog = false;
                    break;
            }
        }
    }
}