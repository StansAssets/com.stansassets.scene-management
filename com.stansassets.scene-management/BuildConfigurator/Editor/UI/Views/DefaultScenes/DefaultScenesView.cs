using System.Collections.Generic;
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

        int m_SelectedPlatform;
        readonly GUIContent[] m_ValidPlatformsGUIContent;
        readonly BuildTargetGroupData m_BuildTargetGroupData;
        readonly List<SceneAssetInfo> m_TempScenesCollection = new();
        
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
                m_ValidPlatformsGUIContent[t].tooltip = m_BuildTargetGroupData.ValidPlatforms[i].BuildTargetGroup.ToString();
            }
        }
        
        public void DrawDefaultScenes(BuildConfiguration conf)
        {
            using (new IMGUIBlockWithIndent(new GUIContent("Default Scenes")))
            {
                EditorGUILayout.HelpBox(k_DefaultScenesDescription, MessageType.Info);
                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.Space(20);
                    using (new IMGUIBeginVertical(DrawingUtility.StyleConfig.FrameBox))
                    {
                        m_SelectedPlatform = GUILayout.Toolbar(m_SelectedPlatform, m_ValidPlatformsGUIContent, DrawingUtility.StyleConfig.ToolbarButton);
                        
                        bool defaultTab = m_SelectedPlatform == 0;
                        if (defaultTab)
                        {
                            m_DefaultScenesList ??= DrawingUtility.CreateScenesReorderableList(conf.DefaultSceneConfigurations[0].Scenes, 
                                _ =>
                                {
                                    SyncDefaultConfigurations(conf);
                                    m_Context.CheckNTryAutoSync();
                                },
                                _ => {
                                    m_Context.CheckNTryAutoSync();
                                },
                                _ => {
                                    SyncDefaultConfigurations(conf);
                                    m_Context.CheckNTryAutoSync(true);
                                });
                            m_DefaultScenesList.DoLayoutList();
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
                        GUILayout.Space(defaultTab ? -20 : -9);
                    }
                }
                GUILayout.Space(30);
            }
        }

        void SyncDefaultConfigurations(BuildConfiguration conf)
        {
            var defaultConfiguration = conf.DefaultSceneConfigurations.FirstOrDefault(c => c.BuildTargetGroup == -1);
            var defaultScenes = defaultConfiguration.Scenes;
            foreach (var c in conf.DefaultSceneConfigurations)
            {
                m_TempScenesCollection.Clear();
                var scenes = c.Scenes;
                m_TempScenesCollection.AddRange(scenes);
                
                // Remove scenes
                for (var i = 0; i < m_TempScenesCollection.Count; ++i)
                {
                    var scene = m_TempScenesCollection[i];
                    if (!defaultScenes.Any(s => s.Guid.Equals(scene.Guid)))
                    {
                        scenes.Remove(scene);
                    }
                }
                
                // Update present and add new scenes
                for (var i = 0; i < defaultScenes.Count; ++i)
                {
                    var defaultScene = defaultScenes[i];
                    var scene = scenes.FirstOrDefault(s => s.Guid.Equals(defaultScene.Guid));
                    if (scene == null)
                    {
                        scenes.Insert(i, new SceneAssetInfo
                        {
                            Name = defaultScene.Name,
                            Guid = defaultScene.Guid,
                            Addressable = c.Override ? false : defaultScene.Addressable
                        });
                    }
                    else
                    {
                        scenes.Remove(scene);
                        scenes.Insert(i, scene);
                        if (!c.Override)
                        {
                            scene.Addressable = defaultScene.Addressable;
                        }
                    }
                }
            }
        }

        // TODO: Move from view into the model
        public void InitializeDefaultSceneConfigurations(BuildConfiguration conf)
        {
            if (conf.DefaultSceneConfigurations.All(c => c.BuildTargetGroup != -1))
            {
                var defaultPlatform = new DefaultScenesConfiguration(-1, new SceneAssetInfo());
                if(conf.DefaultSceneConfigurations.Count <= 0)
                    conf.DefaultSceneConfigurations.Add(defaultPlatform);
                else
                    conf.DefaultSceneConfigurations.Insert(0, defaultPlatform);
                
                for (int i = 0; i < m_BuildTargetGroupData.ValidPlatforms.Length; i++)
                {
                    BuildTargetGroup buildTargetGroup = m_BuildTargetGroupData.ValidPlatforms[i].BuildTargetGroup;
                    var buildTargetGroupInt = (int) buildTargetGroup;
                    if(conf.DefaultSceneConfigurations.All(c => c.BuildTargetGroup != buildTargetGroupInt))
                        conf.DefaultSceneConfigurations.Add(new DefaultScenesConfiguration((int)buildTargetGroup));
                }
            }
        }

        SceneAssetInfo ImmutableContentTypeListItem(Rect pos, SceneAssetInfo itemValue)
        {
            if (itemValue == null)
                itemValue = new SceneAssetInfo();

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect sceneIndexRect = DrawingUtility.ShowBuildIndex ? new Rect(pos.x, pos.y, 20f, pos.height) : new Rect(pos.x, pos.y, 0f, 0f);
            Rect objectFieldRect = new Rect(pos.x + sceneIndexRect.width, pos.y + 2, pos.width - 20f - sceneIndexRect.width, 16);
            Rect addressableToggleRect = new Rect(objectFieldRect.x + objectFieldRect.width + 2, pos.y, 20f, pos.height);

            if (DrawingUtility.ShowBuildIndex)
            {
                int sceneIndex = BuildConfigurationSettings.Instance.Configuration.GetSceneIndex(itemValue, EditorUserBuildSettings.activeBuildTarget);
                if(sceneIndex >= 0)
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
