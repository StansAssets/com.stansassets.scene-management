using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    [InitializeOnLoad]
    internal static class BuildConfigurationSettingsConfig
    {
        const string k_ShowOutOfSyncPreventingDialogPref = "show_out_of_sync_preventing_dialog";

        static bool s_ShowOutOfSyncPreventingDialog;

        static BuildConfigurationSettingsConfig()
        {
            s_ShowOutOfSyncPreventingDialog = PlayerPrefs.GetInt(k_ShowOutOfSyncPreventingDialogPref, 1) == 1;
        }

        internal static bool ShowOutOfSyncPreventingDialog
        {
            get => s_ShowOutOfSyncPreventingDialog;
            set
            {
                s_ShowOutOfSyncPreventingDialog = value;
                PlayerPrefs.SetInt(k_ShowOutOfSyncPreventingDialogPref, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}