using System;
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

                var needScenesSync = EditorBuildSettingsValidator.CompareScenesWithBuildSettings();
                if (needScenesSync)
                {
                    DrawMessage(EditorBuildSettingsValidator.ScenesSyncWarningDescription, MessageType.Error,
                        "Clear Build Settings & Sync", SyncScenes);
                    return;
                }

                var hasMissingScenes = EditorBuildSettingsValidator.HasMissingScenes();
                if (hasMissingScenes)
                {
                    DrawMessage(EditorBuildSettingsValidator.SceneMissingWarningDescription,
                        MessageType.Warning);
                    return;
                }

                var hasDuplicates = EditorBuildSettingsValidator.HasScenesDuplicates();
                if (hasDuplicates)
                {
                    DrawMessage(EditorBuildSettingsValidator.ScenesDuplicatesWarningDescription,
                        MessageType.Warning);
                    return;
                }

                DrawMessage("All good", MessageType.Info);
            }
        }

        void DrawMessage(string message, MessageType messageType, string actionText = "", Action actionCallback = null)
        {
            EditorGUILayout.HelpBox(message, messageType);

            using (new IMGUIBeginHorizontal())
            {
                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(actionText))
                {
                    var active = GUILayout.Button(actionText);

                    if (active)
                    {
                        actionCallback?.Invoke();
                    }
                }
                else
                {
                    GUILayout.Label("", GUILayout.Height(17f));
                }
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

        void PreventingDialogs()
        {
            using (new IMGUIBeginHorizontal())
            {
                EditorGUILayout.LabelField("Show scene sync warning on Entering Playmode");

                BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog =
                    EditorGUILayout.Toggle(BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog);
            }
        }
    }
}