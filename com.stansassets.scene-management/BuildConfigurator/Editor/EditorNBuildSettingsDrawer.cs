using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    class EditorNBuildSettingsDrawer
    {
        internal void DrawSettings()
        {
            using (new IMGUIBlockWithIndent(new GUIContent("Editor & Build Settings")))
            {
                PreventingDialogs();
                DrawDuplicates();
                DrawScenesSync();
            }
        }

        void PreventingDialogs()
        {
            EditorGUIUtility.labelWidth = 300f;
            BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog = EditorGUILayout
                .Toggle("Show scene sync warning on Entering Playmode",
                    BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog);
        }

        void DrawScenesSync()
        {
            var needScenesSync = EditorBuildSettingsValidator.CompareScenesWithBuildSettings();
            
            if (needScenesSync)
            {
                EditorGUILayout.HelpBox(EditorBuildSettingsValidator.ScenesSyncWarningDescription, MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox(EditorBuildSettingsValidator.ScenesSyncOkDescription, MessageType.Info);
            }

            GUI.enabled = needScenesSync;
            using (new IMGUIBeginHorizontal())
            {
                GUILayout.FlexibleSpace();

                var active = GUILayout.Button(
                    "Clear Build Settings & Sync",
                    GUILayout.Width(240));

                if (active)
                {
                    if (BuildConfigurationSettings.Instance.HasValidConfiguration)
                    {
                        BuildConfigurationSettings.Instance.Configuration.SetupEditorSettings(
                            EditorUserBuildSettings.activeBuildTarget, true);
                    }
                }
            }

            GUI.enabled = true;
        }

        void DrawDuplicates()
        {
            var hasDuplicates = EditorBuildSettingsValidator.HasScenesDuplicates();

            if (hasDuplicates)
            {
                EditorGUILayout.HelpBox(EditorBuildSettingsValidator.ScenesDuplicatesWarningDescription,
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(EditorBuildSettingsValidator.ScenesDuplicatesOkDescription, MessageType.Info);
            }
        }
    }
}