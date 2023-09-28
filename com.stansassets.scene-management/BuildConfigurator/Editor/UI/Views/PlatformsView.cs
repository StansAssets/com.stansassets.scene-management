using System.Collections.Generic;
using Rotorz.ReorderableList;
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace StansAssets.SceneManagement.Build
{
    class PlatformsView
    {
        readonly BuildConfigurationContext m_Context;
        readonly Dictionary<PlatformsConfiguration, (ReorderableList platforms, ReorderableList scenes)> m_ReorderableLists = new();

        public PlatformsView(BuildConfigurationContext context)
        {
            m_Context = context;
        }
        
        public void DrawPlatforms(BuildConfiguration conf)
        {
            using (new IMGUIBlockWithIndent(new GUIContent("Platforms")))
            {
                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.Space(20);
                    using (new IMGUIBeginVertical())
                    {
                        foreach (var platform in conf.Platforms)
                        {
                            DrawPlatform(conf, platform);
                        }
                    }
                }

                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        conf.Platforms.Add(new PlatformsConfiguration());
                    }
                }
            }

            DrawingUtility.ShowBuildIndex = true;
        } 
        
        void DrawPlatform(BuildConfiguration conf, PlatformsConfiguration platform)
        {
            DrawingUtility.ShowBuildIndex = conf.IsActive(platform);
            
            var reorderableList = GetPlatformReorderableList(platform);

            using (new IMGUIBeginHorizontal(GUI.skin.box))
            {
                DrawPlatformRemoveButton(conf, platform);

                using (new IMGUIBeginVertical(ReorderableListStyles.Container2, GUILayout.MinWidth(100f), GUILayout.MaxWidth(Screen.width / 2f)))
                {
                    reorderableList.platforms.DoLayoutList();
                }

                using (new IMGUIBeginVertical(ReorderableListStyles.Container2, GUILayout.MinWidth(100f), GUILayout.MaxWidth(Screen.width)))
                {
                    reorderableList.scenes.DoLayoutList();
                }
            }
        }

        void DrawPlatformRemoveButton(BuildConfiguration conf, PlatformsConfiguration platform)
        {
            using (new IMGUIBeginVertical(GUILayout.Width(10)))
            {
                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.Space(2);
                        
                    var delete = GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(18));
                    if (delete)
                    {
                        conf.Platforms.Remove(platform);
                        m_Context.CheckNTryAutoSync(true);
                        
                        GUIUtility.ExitGUI();
                    }

                    GUILayout.Space(-5);
                }
            }
        }
        
        (ReorderableList platforms, ReorderableList scenes) GetPlatformReorderableList(PlatformsConfiguration platform)
        {
            if (m_ReorderableLists.ContainsKey(platform))
            {
                return m_ReorderableLists[platform];
            }

            var platforms = DrawingUtility.CreatePlatformsReorderableList(platform.BuildTargets,
                _ => { m_Context.CheckNTryAutoSync(true); },
                _ => { m_Context.CheckNTryAutoSync(true); },
                );
            
            var scenes = DrawingUtility.CreateScenesReorderableList(platform.Scenes,
                _ => { m_Context.CheckNTryAutoSync(); },
                _ => { m_Context.CheckNTryAutoSync(); },
                _ => { m_Context.CheckNTryAutoSync(true); });
            m_ReorderableLists.Add(platform, (platforms, scenes));

            return m_ReorderableLists[platform];
        }
    }
}
