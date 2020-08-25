using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;
using StansAssets.Plugins.Editor;

namespace StansAssets.SceneManagement.Build
{
    public class BuildConfigurationWindow : IMGUISettingsWindow<BuildConfigurationWindow>
    {
        const string k_DefaultScenesDescription = "If you are leaving the default scnese empty, " +
            "projects settings defined scene will be added to the build. " +
            "When Defult Scenes have atleaest one scene defined, " +
            "project scenes are ignored and only scene defined in this configuration will be used.";

        [SerializeField]
        IMGUIHyperLabel m_AddButton;

        protected override void OnAwake()
        {
            SetHeaderTitle("Cross-Platform build configuration");
            SetHeaderDescription("Make configuration for every platform per different build types.");

            SetHeaderVersion("preview");
            SetDocumentationUrl("https://github.com/StansAssets/com.stansassets.scene-management");

            if (BuildConfigurationSettings.Instance.BuildConfigurations.Count == 0)
            {
                var conf = new BuildConfiguration { Name = "Default" };
                BuildConfigurationSettings.Instance.BuildConfigurations.Add(conf);
            }

            m_MenuToolbar = new IMGUIHyperToolbar();

            foreach (var conf in BuildConfigurationSettings.Instance.BuildConfigurations)
            {
                AddBuildConfigurationTab(conf.Name);
            }

            UpdateActiveConfUI();

            m_AddButton = new IMGUIHyperLabel(new GUIContent("+"), EditorStyles.miniLabel);
            m_AddButton.SetMouseOverColor(SettingsWindowStyles.SelectedElementColor);
        }

        void UpdateActiveConfUI()
        {
            foreach (var btn in m_MenuToolbar.Buttons)
            {
                var index = m_MenuToolbar.Buttons.IndexOf(btn);
                var contentText = BuildConfigurationSettings.Instance.BuildConfigurations[index].Name;

                if (index == BuildConfigurationSettings.Instance.ActiveConfigurationIndex)
                {
                    contentText += " (A)";
                }

                btn.Content.text = contentText;
            }
        }

        void AddBuildConfigurationTab(string itemName)
        {
            var button = new IMGUIHyperLabel(new GUIContent(itemName), EditorStyles.boldLabel);
            button.SetMouseOverColor(SettingsWindowStyles.SelectedElementColor);
            m_MenuToolbar.AddButtons(button);
        }

        protected override void BeforeGUI()
        {
            EditorGUI.BeginChangeCheck();
        }

        protected override void AfterGUI()
        {
            if (EditorGUI.EndChangeCheck())
            {
                UpdateActiveConfUI();
                BuildConfigurationSettings.Save();
            }
        }

        int m_SelectionIndex;

        protected override void OnLayoutGUI()
        {
            DrawToolbar();
            DrawHeader();

            m_SelectionIndex = DrawTabs();

            DrawScrollView(() =>
            {
                DrawConfiguration(m_SelectionIndex);
            });
        }

        void DrawConfiguration(int index)
        {
            var conf = BuildConfigurationSettings.Instance.BuildConfigurations[index];
            using (new IMGUIBlockWithIndent(new GUIContent("Settings")))
            {
                conf.Name = IMGUILayout.TextField("Configuration Name:", conf.Name);
                conf.DefaultScenesFirst = IMGUILayout.ToggleFiled("Default Scenes First", conf.DefaultScenesFirst, IMGUIToggleStyle.ToggleType.YesNo);

                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.FlexibleSpace();

                    if (BuildConfigurationSettings.Instance.ActiveConfigurationIndex == index)
                    {
                        GUI.enabled = false;
                    }

                    bool active = GUILayout.Button("Set As Active", GUILayout.Width(100));
                    if (active)
                    {
                        BuildConfigurationSettings.Instance.ActiveConfigurationIndex = index;
                        UpdateActiveConfUI();
                    }

                    GUI.enabled = true;

                    bool remove = GUILayout.Button("Remove", GUILayout.Width(100));
                    if (remove)
                    {
                        BuildConfigurationSettings.Instance.BuildConfigurations.Remove(conf);
                        OnAwake();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
            }

            if (conf.DefaultScenesFirst)
            {
                DrawDefaultScenes(conf);
                DrawPlatforms(conf);
            }
            else
            {
                DrawPlatforms(conf);
                DrawDefaultScenes(conf);
            }
        }

        void DrawDefaultScenes(BuildConfiguration conf)
        {
            using (new IMGUIBlockWithIndent(new GUIContent("Default Scenes")))
            {
                EditorGUILayout.HelpBox(k_DefaultScenesDescription, MessageType.Info);
                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.Space(20);
                    using (new IMGUIBeginVertical())
                    {
                        ReorderableListGUI.ListField(conf.DefaultScenes, ContentTypeListItem, DrawEmptyScene);
                    }
                }
            }
        }

        void DrawPlatforms(BuildConfiguration conf)
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
                            EditorGUILayout.BeginHorizontal(GUI.skin.box);
                            {
                                EditorGUILayout.BeginVertical(GUILayout.Width(10));
                                {
                                    using (new IMGUIBeginHorizontal())
                                    {
                                        GUILayout.Space(2);
                                        bool delete = GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(18));
                                        if (delete)
                                        {
                                            conf.Platforms.Remove(platform);
                                            GUIUtility.ExitGUI();
                                            break;
                                        }

                                        GUILayout.Space(-5);
                                    }
                                }
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.BeginVertical(GUILayout.Width(150));
                                {
                                    ReorderableListGUI.Title("Build Targets");
                                    ReorderableListGUI.ListField(platform.BuildTargets, BuildTargetListItem, DrawEmptyPlatform);
                                }
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                                {
                                    ReorderableListGUI.Title("Scenes");
                                    ReorderableListGUI.ListField(platform.Scenes, ContentTypeListItem, DrawEmptyScene);
                                }
                                EditorGUILayout.EndVertical();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        PlatformsConfiguration s = new PlatformsConfiguration();
                        conf.Platforms.Add(s);
                    }
                }
            }
        }

        BuildTarget BuildTargetListItem(Rect pos, BuildTarget itemValue)
        {
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            BuildTarget target = (BuildTarget)EditorGUI.EnumPopup(pos, itemValue);
            EditorGUI.indentLevel = indentLevel;
            return target;
        }

        SceneAsset ContentTypeListItem(Rect pos, SceneAsset itemValue)
        {
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var assets = EditorGUI.ObjectField(pos, itemValue, typeof(SceneAsset), false) as SceneAsset;
            EditorGUI.indentLevel = indentLevel;
            return assets;
        }

        void DrawEmptyScene()
        {
            GUILayout.Label("Add a scenes", EditorStyles.miniLabel);
        }

        void DrawEmptyPlatform()
        {
            GUILayout.Label("Add a build target", EditorStyles.miniLabel);
        }

        protected int DrawTabs()
        {
            GUILayout.Space(2);

            int index;
            using (new IMGUIBeginHorizontal())
            {
                using (new IMGUIBeginVertical())
                {
                    index = m_MenuToolbar.Draw();
                }

                var add = m_AddButton.Draw(GUILayout.Width(20));
                if (add)
                {
                    var conf = BuildConfigurationSettings.Instance.BuildConfigurations[m_SelectionIndex];
                    var copy = conf.Copy();
                    BuildConfigurationSettings.Instance.BuildConfigurations.Add(copy);
                    AddBuildConfigurationTab(copy.Name);
                    GUIUtility.ExitGUI();
                }
            }

            GUILayout.Space(4);

            EditorGUILayout.BeginVertical(SettingsWindowStyles.SeparationStyle);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            return index;
        }
    }
}
