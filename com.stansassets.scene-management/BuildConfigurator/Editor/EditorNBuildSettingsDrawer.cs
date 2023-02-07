using System.Linq;
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    internal class EditorNBuildSettingsDrawer
    {
        AutoSyncParams m_AutoSyncParams;

        internal void DrawSettings(BuildConfiguration buildConfiguration)
        {
            CheckNTryAutoSync(buildConfiguration);

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
            if (m_AutoSyncParams.NeedScenesSync)
            {
                EditorGUILayout.HelpBox(EditorBuildSettingsValidator.ScenesSyncWarningDescription, MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox(EditorBuildSettingsValidator.ScenesSyncOkDescription, MessageType.Info);
            }

            GUI.enabled = m_AutoSyncParams.NeedScenesSync;
            using (new IMGUIBeginHorizontal())
            {
                GUILayout.FlexibleSpace();

                var active = GUILayout.Button(
                    "Clear Build Settings & Sync",
                    GUILayout.Width(240));

                if (active)
                {
                    SyncScenes();
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

        void SyncScenes()
        {
            if (BuildConfigurationSettings.Instance.HasValidConfiguration)
            {
                BuildConfigurationSettings.Instance.Configuration.SetupEditorSettings(
                    EditorUserBuildSettings.activeBuildTarget, true);
            }
        }

        void CheckNTryAutoSync(BuildConfiguration buildConfiguration)
        {
            m_AutoSyncParams.NeedScenesSync = EditorBuildSettingsValidator.CompareScenesWithBuildSettings();

            var scenesCount = buildConfiguration.DefaultScenes.Count +
                              buildConfiguration.Platforms.Sum(i => i.Scenes.Count);

            if (scenesCount == m_AutoSyncParams.LastScenesCount)
            {
                return;
            }

            if (m_AutoSyncParams.Synced && m_AutoSyncParams.NeedScenesSync)
            {
                SyncScenes();
                return;
            }

            if (!m_AutoSyncParams.Synced && !m_AutoSyncParams.NeedScenesSync)
            {
                m_AutoSyncParams.Synced = true;
                m_AutoSyncParams.LastScenesCount = buildConfiguration.DefaultScenes.Count +
                                                   buildConfiguration.Platforms.Sum(i => i.Scenes.Count);
            }
        }

        struct AutoSyncParams
        {
            public int LastScenesCount;
            public bool Synced;
            public bool NeedScenesSync;

            public AutoSyncParams(int lastScenesCount, bool synced, bool needScenesSync)
            {
                LastScenesCount = lastScenesCount;
                Synced = synced;
                NeedScenesSync = needScenesSync;
            }
        }
    }
}