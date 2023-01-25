using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;
using Scenes.Editor;
using StansAssets.Plugins.Editor;

namespace StansAssets.SceneManagement.Build
{
    class BuildConfigurationWindow : IMGUISettingsWindow<BuildConfigurationWindow>, IHasCustomMenu
    {
        const string k_DefaultScenesDescription = "If you are leaving the default scene empty, " +
            "projects settings defined scene will be added to the build. " +
            "When Defult Scenes have atleaest one scene defined, " +
            "project scenes are ignored and only scene defined in this configuration will be used.";
        const int k_DefaultBuildTarget = -1;

        static readonly Color s_ErrorColor = new Color(1f, 0.8f, 0.0f);
        static readonly Color s_InactiveColor = new Color(1f, 0.8f, 0.0f);
        static readonly GUIContent s_DuplicatesGUIContent = new GUIContent("", "Scene is duplicated!");
        static readonly GUIContent s_EmptySceneGUIContent = new GUIContent("", "Scene is empty! Please drop a scene or remove this element.");

        [SerializeField]
        IMGUIHyperLabel m_AddButton;

        bool m_ShowBuildIndex;
        int m_SelectedPlatform;
        GUIContent[] m_ValidPlatformsGUIContent;
        BuildTargetGroupData m_BuildTargetGroupData;

        protected override void OnAwake()
        {
            titleContent = new GUIContent("Cross-Platform build configuration");
            SetPackageName(SceneManagementPackage.PackageName);

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
                foreach (var buildConfiguration in BuildConfigurationSettings.Instance.BuildConfigurations)
                {
                    buildConfiguration.UpdateSceneNames();
                }

                BuildConfigurationSettings.Save();
            }
        }

        int m_SelectionIndex;

        protected override void OnLayoutGUI()
        {
            DrawToolbar();
            DrawHeader();

            InitializeValidPlatform();

            m_SelectionIndex = DrawTabs();

            DrawScrollView(() => { DrawConfiguration(m_SelectionIndex); });
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
                        conf.CleanEditorPrefsData();
                        BuildConfigurationSettings.Instance.BuildConfigurations.Remove(conf);
                        OnAwake();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
            }

            using (new IMGUIBlockWithIndent(new GUIContent("Addressables")))
            {
                conf.UseAddressablesInEditor =
                    IMGUILayout.ToggleFiled("Use Addressables InEditor", conf.UseAddressablesInEditor, IMGUIToggleStyle.ToggleType.YesNo);

                conf.ClearAllAddressablesCache =
                    IMGUILayout.ToggleFiled("Clear All Addressables Cache", conf.ClearAllAddressablesCache, IMGUIToggleStyle.ToggleType.YesNo);

                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.FlexibleSpace();
                    var active = GUILayout.Button("Build", GUILayout.Width(100));
                    if (active)
                    {
                        BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                        BuildScenesPreprocessor.SetupAddressableScenes(EditorUserBuildSettings.activeBuildTarget, buildTargetGroup);
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
                        EditorGUILayout.LabelField("Selected Platform: ");
                        m_SelectedPlatform = GUILayout.SelectionGrid(m_SelectedPlatform, m_ValidPlatformsGUIContent, m_ValidPlatformsGUIContent.Length);

                        InitializeDefaultSceneConfigurations(conf);

                        if (m_SelectedPlatform == 0)
                        {
                            ReorderableListGUI.ListField(conf.DefaultSceneConfigurations[0].Scenes, ContentTypeListItem, DrawEmptyScene);
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

                            if (conf.DefaultSceneConfigurations[m_SelectedPlatform].Override)
                            {
                                ReorderableListGUI.ListField(conf.DefaultSceneConfigurations[m_SelectedPlatform].Scenes,
                                    ImmutableContentTypeListItem, DrawEmptyScene,
                                    ReorderableListFlags.DisableReordering | ReorderableListFlags.HideAddButton | ReorderableListFlags.HideRemoveButtons);
                            }
                        }
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
                            m_ShowBuildIndex = conf.IsActive(platform);
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
                                    GUI.backgroundColor = m_ShowBuildIndex ? GUI.skin.settings.selectionColor : Color.white;
                                    ReorderableListGUI.Title("Scenes");
                                    GUI.backgroundColor = Color.white;

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

            m_ShowBuildIndex = true;
        }

        BuildTargetRuntime BuildTargetListItem(Rect pos, BuildTargetRuntime itemValue)
        {
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            BuildTargetRuntime target = (BuildTargetRuntime)EditorGUI.EnumPopup(pos, itemValue);
            EditorGUI.indentLevel = indentLevel;
            return target;
        }

        SceneAssetInfo ContentTypeListItem(Rect pos, SceneAssetInfo itemValue)
        {
            if (itemValue == null)
                itemValue = new SceneAssetInfo();

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect sceneIndexRect = m_ShowBuildIndex ? new Rect(pos.x, pos.y, 20f, pos.height) : new Rect(pos.x, pos.y, 0f, 0f);
            Rect objectFieldRect = new Rect(pos.x + sceneIndexRect.width, pos.y + 2, pos.width - 20f - sceneIndexRect.width, 16);
            Rect addressableToggleRect = new Rect(objectFieldRect.x + objectFieldRect.width + 2, pos.y, 20f, pos.height);

            if (m_ShowBuildIndex)
            {
                int sceneIndex = BuildConfigurationSettings.Instance.Configuration.GetSceneIndex(itemValue);
                GUI.Label(sceneIndexRect, sceneIndex.ToString());
            }

            var sceneAsset = itemValue.GetSceneAsset();
            bool sceneWithError = sceneAsset == null;
            GUI.color = sceneWithError ? s_ErrorColor : Color.white;

            EditorGUI.BeginChangeCheck();
            var newSceneAsset = EditorGUI.ObjectField(objectFieldRect, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
            if (EditorGUI.EndChangeCheck())
            {
                itemValue.SetSceneAsset(newSceneAsset);
            }

            GUI.color = Color.white;

            itemValue.Addressable = GUI.Toggle(addressableToggleRect, itemValue.Addressable, AddressableGuiContent);
            EditorGUI.indentLevel = indentLevel;

            return itemValue;
        }

        SceneAssetInfo ImmutableContentTypeListItem(Rect pos, SceneAssetInfo itemValue)
        {
            if (itemValue == null)
                itemValue = new SceneAssetInfo();

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect sceneIndexRect = m_ShowBuildIndex ? new Rect(pos.x, pos.y, 20f, pos.height) : new Rect(pos.x, pos.y, 0f, 0f);
            Rect objectFieldRect = new Rect(pos.x + sceneIndexRect.width, pos.y + 2, pos.width - 20f - sceneIndexRect.width, 16);
            Rect addressableToggleRect = new Rect(objectFieldRect.x + objectFieldRect.width + 2, pos.y, 20f, pos.height);

            if (m_ShowBuildIndex)
            {
                int sceneIndex = BuildConfigurationSettings.Instance.Configuration.GetSceneIndex(itemValue);
                GUI.Label(sceneIndexRect, sceneIndex.ToString());
            }

            EditorGUI.BeginDisabledGroup(true);
            {
                var sceneAsset = itemValue.GetSceneAsset();
                bool sceneWithError = sceneAsset == null;
                GUI.color = sceneWithError ? s_ErrorColor : Color.white;

                EditorGUI.BeginChangeCheck();
                var newSceneAsset = EditorGUI.ObjectField(objectFieldRect, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
                if (EditorGUI.EndChangeCheck())
                {
                    itemValue.SetSceneAsset(newSceneAsset);
                }
            }
            EditorGUI.EndDisabledGroup();
            GUI.color = Color.white;

            itemValue.Addressable = GUI.Toggle(addressableToggleRect, itemValue.Addressable, AddressableGuiContent);
            EditorGUI.indentLevel = indentLevel;

            return itemValue;
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

        void CopyScenesFromDefaultConfiguration(BuildConfiguration conf, bool isOverride)
        {
            List<SceneAssetInfo> defaultScenes = conf.DefaultSceneConfigurations[0].Scenes;
            List<SceneAssetInfo> selectedSceneAssetInfo = conf.DefaultSceneConfigurations[m_SelectedPlatform].Scenes;

            if (!isOverride)
            {
                conf.DefaultSceneConfigurations[m_SelectedPlatform].Scenes.Clear();
                for (var i = 0; i < defaultScenes.Count; i++)
                {
                    conf.DefaultSceneConfigurations[m_SelectedPlatform].Scenes
                        .Add(new SceneAssetInfo(defaultScenes[i].Guid, defaultScenes[i].Name, defaultScenes[i].Addressable));
                }

                return;
            }

            for (var i = 0; i < selectedSceneAssetInfo.Count; i++)
            {
                if (defaultScenes.Any(x => x.Guid == selectedSceneAssetInfo[i].Guid)) continue;
                selectedSceneAssetInfo.RemoveAt(i);
                i--;
            }

            for (var i = 0; i < defaultScenes.Count; i++)
            {
                if (selectedSceneAssetInfo.Any(x => x.Guid == defaultScenes[i].Guid)) continue;
                var newElement = new SceneAssetInfo(defaultScenes[i].Name, defaultScenes[i].Guid, defaultScenes[i].Addressable);
                selectedSceneAssetInfo.Add(newElement);
            }

            selectedSceneAssetInfo = selectedSceneAssetInfo.OrderBy(x => defaultScenes.IndexOf(defaultScenes.First(y => y.Guid == x.Guid))).ToList();

            conf.DefaultSceneConfigurations[m_SelectedPlatform].Scenes = selectedSceneAssetInfo;
        }

        void InitializeDefaultSceneConfigurations(BuildConfiguration conf)
        {
            if (conf.DefaultSceneConfigurations.Count == m_ValidPlatformsGUIContent.Length) return;
            var newSceneAssetInfo = new SceneAssetInfo();
            conf.DefaultSceneConfigurations.Add(new DefaultSceneConfiguration(k_DefaultBuildTarget, newSceneAssetInfo));
            for (int i = 1; i < m_ValidPlatformsGUIContent.Length; i++)
            {
                BuildTargetGroup buildTargetGroup = m_BuildTargetGroupData.ValidPlatforms[i - 1].BuildTargetGroup;
                conf.DefaultSceneConfigurations.Add(new DefaultSceneConfiguration(buildTargetGroup.ConvertBuildTargetGroupToRuntime(), newSceneAssetInfo));
            }
        }

        void InitializeValidPlatform()
        {
            m_BuildTargetGroupData = new BuildTargetGroupData();

            m_ValidPlatformsGUIContent = new GUIContent[m_BuildTargetGroupData.ValidPlatforms.Length + 1];
            m_ValidPlatformsGUIContent[0] = new GUIContent("Default");
            for (var i = 0; i < m_BuildTargetGroupData.ValidPlatforms.Length; i++)
            {
                int t = i + 1;
                m_ValidPlatformsGUIContent[t] = EditorGUIUtility.IconContent($"{m_BuildTargetGroupData.ValidPlatforms[i].IconName}");
            }
        }

        static GUIContent s_AddressableGuiContent;

        static GUIContent AddressableGuiContent =>
            s_AddressableGuiContent ?? (s_AddressableGuiContent = new GUIContent("",
                "Mark scene Addressable?\nIf true - scene will be added as Addressable asset into \"Scenes\" group, otherwise - scene will be added into build settings."));

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Add Build Settings Scenes to Default"), false, () =>
            {
                var conf = BuildConfigurationSettings.Instance.BuildConfigurations[m_SelectionIndex];
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    var sceneAssetInfo = new SceneAssetInfo();
                    sceneAssetInfo.SetSceneAsset(scene);
                    conf.DefaultSceneConfigurations.Add(new DefaultSceneConfiguration(k_DefaultBuildTarget, sceneAssetInfo));
                }

                BuildConfigurationSettings.Save();
            });
        }
    }
}
