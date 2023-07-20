using System;
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    class SettingsView
    {
        const string k_ScenesSyncDescription = "Current Editor Build Settings are our of sync " +
            "with the Scene Management build configuration.";
        
        const string k_SceneMissingWarningDescription = "Your configuration has missing scenes, consider fixing it.";
        const string k_RepetitiveScenesWarningDescription = "Your configuration has duplicated scenes, consider fixing it.";
        const string k_RepetitiveBuildTargetsWarningDescription = "Your configuration has duplicated build targets, consider fixing it.";

        readonly BuildConfigurationContext m_Context;
        
        public SettingsView(BuildConfigurationContext context)
        {
            m_Context = context;
        }
        
        public void DrawSettings()
        {
            using (new IMGUIBlockWithIndent(new GUIContent("Editor & Build Settings")))
            {
                PreventingDialogs();
                
                var hasAnyScene = BuildConfigurationSettingsValidator.HasAnyScene();
                if (!hasAnyScene)
                {
                    DrawMessage("There are no scenes in the configuration", MessageType.Info);
                    return;
                }

                var hasBuildTargetDuplicates = BuildConfigurationSettingsValidator.HasBuildTargetsDuplicates();
                if (hasBuildTargetDuplicates)
                {
                    DrawMessage(k_RepetitiveBuildTargetsWarningDescription,
                        MessageType.Warning);
                    return;
                }

                var hasDuplicates = BuildConfigurationSettingsValidator.HasScenesDuplicates();
                if (hasDuplicates)
                {
                    DrawMessage(k_RepetitiveScenesWarningDescription,
                        MessageType.Warning);
                    return;
                }

                var needScenesSync = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();
                if (needScenesSync)
                {
                    DrawMessage(k_ScenesSyncDescription, MessageType.Error,
                        "Fix and sync", m_Context.SyncScenes);
                    return;
                }

                var hasMissingScenes = BuildConfigurationSettingsValidator.HasMissingScenes();
                if (hasMissingScenes)
                {
                    DrawMessage(k_SceneMissingWarningDescription, MessageType.Warning,
                        "Fix and sync", m_Context.SyncScenes);
                    return;
                }
                
                DrawMessage("No issues found with the configuration.", MessageType.Info);
            }
        }
        
        void PreventingDialogs()
        {
            using (new IMGUIBeginHorizontal())
            {
                var labelStyle = new GUIStyle(GUI.skin.GetStyle("label"))
                {
                    wordWrap = true
                };
                
                EditorGUILayout.LabelField("Show scene sync warning on Entering Playmode", labelStyle);
 
                BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog =
                    EditorGUILayout.Toggle(BuildConfigurationSettingsConfig.ShowOutOfSyncPreventingDialog);
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
    }
}
