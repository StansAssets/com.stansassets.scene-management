using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    internal class EditorNBuildSettingsDrawer
    {
        bool m_SyncFoldout;

        public EditorNBuildSettingsDrawer()
        {
            m_SyncFoldout = false;
        }

        internal void DrawSettings()
        {
            using (new IMGUIBlockWithIndent(new GUIContent("Editor & Build Settings")))
            {
                DrawScenesSync();
                DrawDuplicates();
            }
        }

        void DrawScenesSync()
        {
            var needScenesSync = EditorBuildSettingsValidator.CompareScenesWithBuildSettings();

            GUI.color = needScenesSync ? EditorBuildSettingsValidator.OutOfSyncColor : Color.white;
            m_SyncFoldout = EditorGUILayout.Foldout(m_SyncFoldout, "Scenes sync");
            GUI.color = Color.white;

            if (!m_SyncFoldout) return;

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
            // TODO: Implement
        }
    }
}