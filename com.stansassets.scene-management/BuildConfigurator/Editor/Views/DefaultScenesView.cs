using System.Linq;
using Rotorz.ReorderableList;
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    class DefaultScenesView
    {
        const string k_DefaultScenesDescription = "If you are leaving the default scnese empty, " +
            "projects settings defined scene will be added to the build. " +
            "When Defult Scenes have atleaest one scene defined, " +
            "project scenes are ignored and only scene defined in this configuration will be used.";
        
        readonly BuildConfigurationContext m_Context;
        ReorderableList m_DefaultScenesList;

        public DefaultScenesView(BuildConfigurationContext context)
        {
            m_Context = context;
        }
        
        public void DrawDefaultScenes(BuildConfiguration conf)
        {
            if (m_DefaultScenesList == null)
            {
                m_DefaultScenesList = DrawingUtility.CreateScenesReorderableList(conf.DefaultScenes, false,
                    _ => { m_Context.CheckNTryAutoSync(); },
                    _ => { m_Context.CheckNTryAutoSync(); },
                    _ => { m_Context.CheckNTryAutoSync(true); });
            }

            using (new IMGUIBlockWithIndent(new GUIContent("Default Scenes")))
            {
                EditorGUILayout.HelpBox(k_DefaultScenesDescription, MessageType.Info);
                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.Space(20);
                    using (new IMGUIBeginVertical(ReorderableListStyles.Container2))
                    {
                        m_DefaultScenesList.DoLayoutList();
                    }
                }
            }
        }
    }
    
    struct AutoSyncParams
    {
        public bool Synced;
        public bool NeedScenesSync;
    }

    class UIStyleConfig
    {
        public Color DuplicateColor { get; } = new Color(1f, 0.78f, 1f);
        public Color ErrorColor = new Color(1f, 0.8f, 0.0f);
        public Color OutOfSyncColor= new Color(0.93f, 0.39f, 0.32f);
        
        public GUIContent AddressableGuiContent => m_AddressableGuiContent ??= new GUIContent("", "Mark scene Addressable?\nIf true - scene will be added as Addressable asset into \"Scenes\" group, otherwise - scene will be added into build settings.");
        
        GUIContent m_AddressableGuiContent;
    }

    class BuildConfigurationContext
    {
        public AutoSyncParams AutoSyncParams;
        public bool ShowBuildIndex;

        public void SyncScenes()
        {
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration) return;
            
            BuildConfigurationSettings.Instance.Configuration.ClearMissingScenes();
            
            BuildConfigurationSettings.Instance.Configuration
                .SetupEditorSettings(EditorUserBuildSettings.activeBuildTarget, true);
            
            AutoSyncParams.Synced = true;
        }
        
        public void CheckNTryAutoSync(bool ignoreCollectionsSize = false)
        {
            var hasAnyScene = BuildConfigurationSettingsValidator.HasAnyScene();
            if (!hasAnyScene)
            {
                AutoSyncParams.Synced = false;
                return;
            }

            var hasBuildTargetDuplicates = BuildConfigurationSettingsValidator.HasBuildTargetsDuplicates();
            if (hasBuildTargetDuplicates)
            {
                return;
            }
            
            var hasDuplicates = BuildConfigurationSettingsValidator.HasScenesDuplicates();
            if (hasDuplicates)
            {
                return;    
            }
            
            var hasMissingScenes = BuildConfigurationSettingsValidator.HasMissingScenes();
            if (hasMissingScenes)
            {
                return;
            }

            if (!ignoreCollectionsSize)
            {
                var scenesCollections = BuildConfigurationSettingsValidator.GetScenesCollections();
                if (scenesCollections.buildScenes.Count() > scenesCollections.confScenes.Count())
                {
                    AutoSyncParams.Synced = false;
                    return;
                }
            }
            
            AutoSyncParams.NeedScenesSync = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();

            if (AutoSyncParams.Synced && AutoSyncParams.NeedScenesSync)
            {
                SyncScenes();
                AutoSyncParams.Synced = true;
            }
            else if (!AutoSyncParams.Synced && !AutoSyncParams.NeedScenesSync)
            {
                AutoSyncParams.Synced = true;
            }
        }
    }
}
