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
                DrawScenesSync();
            }
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
    }
}