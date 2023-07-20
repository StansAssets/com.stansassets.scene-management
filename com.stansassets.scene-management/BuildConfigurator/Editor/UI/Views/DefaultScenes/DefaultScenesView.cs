using System.Collections.Generic;
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

        int m_SelectedPlatform;
        GUIContent[] m_ValidPlatformsGUIContent;
        BuildTargetGroupData m_BuildTargetGroupData;
        
        public DefaultScenesView(BuildConfigurationContext context)
        {
            m_Context = context;
            
            m_BuildTargetGroupData = new BuildTargetGroupData();

            m_ValidPlatformsGUIContent = new GUIContent[m_BuildTargetGroupData.ValidPlatforms.Length + 1];
            m_ValidPlatformsGUIContent[0] = new GUIContent("Default");
            for (var i = 0; i < m_BuildTargetGroupData.ValidPlatforms.Length; i++)
            {
                int t = i + 1;
                m_ValidPlatformsGUIContent[t] = EditorGUIUtility.IconContent($"{m_BuildTargetGroupData.ValidPlatforms[i].IconName}");
            }
        }
        
        public void DrawDefaultScenes(BuildConfiguration conf)
        {
            /*if (m_DefaultScenesList == null)
            {
                m_DefaultScenesList = DrawingUtility.CreateScenesReorderableList(conf.DefaultScenes, false,
                    _ => { m_Context.CheckNTryAutoSync(); },
                    _ => { m_Context.CheckNTryAutoSync(); },
                    _ => { m_Context.CheckNTryAutoSync(true); });
            }*/

            using (new IMGUIBlockWithIndent(new GUIContent("Default Scenes")))
            {
                EditorGUILayout.HelpBox(k_DefaultScenesDescription, MessageType.Info);
                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.Space(20);
                    using (new IMGUIBeginVertical(DrawingUtility.StyleConfig.FrameBox))
                    {
                        m_SelectedPlatform = GUILayout.Toolbar(m_SelectedPlatform, m_ValidPlatformsGUIContent, DrawingUtility.StyleConfig.ToolbarButton);
                        
                        InitializeDefaultSceneConfigurations(conf);

                        bool defaultTab = m_SelectedPlatform == 0;
                        if (defaultTab)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Space(1f);
                                ReorderableListGUI.ListField(conf.DefaultSceneConfigurations[0].Scenes, ContentTypeListItem, DrawingUtility.DrawEmptyScene);
                                GUILayout.Space(-5f);
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            {
                                conf.DefaultSceneConfigurations[m_SelectedPlatform].Override
                                    = EditorGUILayout.Toggle(GUIContent.none, conf.DefaultSceneConfigurations[m_SelectedPlatform].Override, GUILayout.Width(15));
                                EditorGUILayout.LabelField($"Override for {m_BuildTargetGroupData.ValidPlatforms[m_SelectedPlatform - 1].BuildTargetGroup}");
                            }
                            GUILayout.EndHorizontal();

                            CopyScenesFromDefaultConfiguration(conf, conf.DefaultSceneConfigurations[m_SelectedPlatform].Override);

                            var prevEnableState = GUI.enabled;
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(1f);
                                    GUI.enabled = conf.DefaultSceneConfigurations[m_SelectedPlatform].Override;
                                    ReorderableListGUI.ListField(conf.DefaultSceneConfigurations[m_SelectedPlatform].Scenes,
                                        ImmutableContentTypeListItem, DrawingUtility.DrawEmptyScene,
                                        ReorderableListFlags.DisableReordering | ReorderableListFlags.HideAddButton | ReorderableListFlags.HideRemoveButtons);
                                    GUILayout.Space(-4f);
                                }
                                GUILayout.EndHorizontal();
                            }
                            GUI.enabled = prevEnableState;
                        }
                        GUILayout.Space(defaultTab ? -25 : -9);
                    }
                }
                GUILayout.Space(10);
            }
        }
        
        void CopyScenesFromDefaultConfiguration(BuildConfiguration conf, bool isOverride)
        {
            List<SceneAssetInfo> defaultScenes = conf.DefaultSceneConfigurations[0].Scenes;
            if (defaultScenes.Count == 0) return;
            List<SceneAssetInfo> selectedPlatformScenes = conf.DefaultSceneConfigurations[m_SelectedPlatform].Scenes;
            if (defaultScenes.Count != selectedPlatformScenes.Count)
            {
                selectedPlatformScenes.Clear();
                for (var i = 0; i < defaultScenes.Count; i++)
                {
                    SceneAssetInfo sceneAssetInfo = defaultScenes[i];
                    var item = new SceneAssetInfo
                    {
                        Name = sceneAssetInfo.Name,
                        Guid = sceneAssetInfo.Guid
                    };
                    if (isOverride)
                    {
                        item.Addressable = sceneAssetInfo.Addressable;
                    }

                    selectedPlatformScenes.Add(item);
                }

                conf.DefaultSceneConfigurations[m_SelectedPlatform].Scenes = selectedPlatformScenes;
            }
        }
        
        void InitializeDefaultSceneConfigurations(BuildConfiguration conf)
        {
            // TODO: Poor place, need to rework
            // Case 1: I uninstalled module and count of valid platforms became -1. It will cause infinite adding platforms becauce 3 configurations != 1 Valid Platform and 1 Default
            if (conf.DefaultSceneConfigurations.Count != m_ValidPlatformsGUIContent.Length)
            {
                conf.DefaultSceneConfigurations.Add(new DefaultScenesConfiguration(-1, new SceneAssetInfo()));
                for (int i = 1; i < m_BuildTargetGroupData.ValidPlatforms.Length; i++)
                {
                    BuildTargetGroup buildTargetGroup = m_BuildTargetGroupData.ValidPlatforms[i-1].BuildTargetGroup;
                    conf.DefaultSceneConfigurations.Add(new DefaultScenesConfiguration((int)buildTargetGroup, new SceneAssetInfo()));
                }
            }
        }
        
        SceneAssetInfo ContentTypeListItem(Rect pos, SceneAssetInfo itemValue)
        {
            if (itemValue == null)
                itemValue = new SceneAssetInfo();

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect sceneIndexRect = m_Context.ShowBuildIndex ? new Rect(pos.x, pos.y, 20f, pos.height) : new Rect(pos.x, pos.y, 0f, 0f);
            Rect objectFieldRect = new Rect(pos.x + sceneIndexRect.width, pos.y + 2, pos.width - 20f - sceneIndexRect.width, 16);
            Rect addressableToggleRect = new Rect(objectFieldRect.x + objectFieldRect.width + 2, pos.y, 20f, pos.height);

            if (m_Context.ShowBuildIndex)
            {
                int sceneIndex = BuildConfigurationSettings.Instance.Configuration.GetSceneIndex(itemValue, EditorUserBuildSettings.activeBuildTarget);
                GUI.Label(sceneIndexRect, sceneIndex.ToString());
            }

            var sceneAsset = itemValue.GetSceneAsset();
            bool sceneWithError = sceneAsset == null;
            GUI.color = sceneWithError ? DrawingUtility.StyleConfig.ErrorColor : Color.white;

            EditorGUI.BeginChangeCheck();
            var newSceneAsset = EditorGUI.ObjectField(objectFieldRect, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
            if (EditorGUI.EndChangeCheck())
            {
                itemValue.SetSceneAsset(newSceneAsset);
            }

            GUI.color = Color.white;

            itemValue.Addressable = GUI.Toggle(addressableToggleRect, itemValue.Addressable, DrawingUtility.StyleConfig.AddressableGuiContent);
            EditorGUI.indentLevel = indentLevel;

            return itemValue;
        }
        
        SceneAssetInfo ImmutableContentTypeListItem(Rect pos, SceneAssetInfo itemValue)
        {
            if (itemValue == null)
                itemValue = new SceneAssetInfo();

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect sceneIndexRect = m_Context.ShowBuildIndex ? new Rect(pos.x, pos.y, 20f, pos.height) : new Rect(pos.x, pos.y, 0f, 0f);
            Rect objectFieldRect = new Rect(pos.x + sceneIndexRect.width, pos.y + 2, pos.width - 20f - sceneIndexRect.width, 16);
            Rect addressableToggleRect = new Rect(objectFieldRect.x + objectFieldRect.width + 2, pos.y, 20f, pos.height);

            if (m_Context.ShowBuildIndex)
            {
                int sceneIndex = BuildConfigurationSettings.Instance.Configuration.GetSceneIndex(itemValue, EditorUserBuildSettings.activeBuildTarget);
                GUI.Label(sceneIndexRect, sceneIndex.ToString());
            }

            EditorGUI.BeginDisabledGroup(true);
            {
                var sceneAsset = itemValue.GetSceneAsset();
                bool sceneWithError = sceneAsset == null;
                GUI.color = sceneWithError ? DrawingUtility.StyleConfig.ErrorColor : Color.white;

                EditorGUI.BeginChangeCheck();
                var newSceneAsset = EditorGUI.ObjectField(objectFieldRect, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
                if (EditorGUI.EndChangeCheck())
                {
                    itemValue.SetSceneAsset(newSceneAsset);
                }
            }
            EditorGUI.EndDisabledGroup();
            GUI.color = Color.white;

            itemValue.Addressable = GUI.Toggle(addressableToggleRect, itemValue.Addressable, DrawingUtility.StyleConfig.AddressableGuiContent);
            EditorGUI.indentLevel = indentLevel;
            return itemValue;
        }
    }
}
