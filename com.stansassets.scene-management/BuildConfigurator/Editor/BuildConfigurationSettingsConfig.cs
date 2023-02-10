using UnityEditor;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    static class BuildConfigurationSettingsConfig
    {
        const string k_ShowOutOfSyncPreventingDialogPref = "show_out_of_sync_preventing_dialog";

        static bool s_ShowOutOfSyncPreventingDialog;

        static BuildConfigurationSettingsConfig()
        {
            s_ShowOutOfSyncPreventingDialog = EditorPrefs.GetBool(k_ShowOutOfSyncPreventingDialogPref, true);
        }

        internal static bool ShowOutOfSyncPreventingDialog
        {
            get => s_ShowOutOfSyncPreventingDialog;
            set
            {
                s_ShowOutOfSyncPreventingDialog = value;
                EditorPrefs.SetBool(k_ShowOutOfSyncPreventingDialogPref, value);
            }
        }
    }
}