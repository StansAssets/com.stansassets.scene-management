using System;
using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;
using StansAssets.Plugins.Editor;
using StansAssets.SceneManagement.Utilities;

namespace StansAssets.SceneManagement.Build
{
    class BuildConfigurationWindow : IMGUISettingsWindow<BuildConfigurationWindow>, IHasCustomMenu
    {
        const string k_DefaultScenesDescription = "If you are leaving the default scnese empty, " +
            "projects settings defined scene will be added to the build. " +
            "When Defult Scenes have atleaest one scene defined, " +
            "project scenes are ignored and only scene defined in this configuration will be used.";

        const string k_ScenesSyncDescription = "Current Editor Build Settings are our of sync " +
                                                      "with the Scene Management build configuration.";
        
        const string k_SceneMissingWarningDescription = "Your configuration has missing scenes, consider fixing it.";
        const string k_RepetitiveScenesWarningDescription = "Your configuration has duplicated scenes, consider fixing it.";

        static readonly Color s_ErrorColor = new Color(1f, 0.8f, 0.0f);
        static readonly Color s_InactiveColor = new Color(1f, 0.8f, 0.0f);
        static readonly  GUIContent s_DuplicatesGUIContent = new GUIContent("","Scene is duplicated!");
        static readonly GUIContent s_EmptySceneGUIContent = new GUIContent("","Scene is empty! Please drop a scene or remove this element.");
        static readonly Color s_OutOfSyncColor = new Color(0.93f, 0.39f, 0.32f);
        static readonly Color s_DuplicateColor = new Color(1f, 0.78f, 1f);
        
        [SerializeField]
        IMGUIHyperLabel m_AddButton;

        bool m_ShowBuildIndex;
        AutoSyncParams m_AutoSyncParams;
        
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
                foreach (var buildConfiguration in BuildConfigurationSettings.Instance.BuildConfigurations) {
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
                        conf.CleanEditorPrefsData();
                        BuildConfigurationSettings.Instance.BuildConfigurations.Remove(conf);
                        OnAwake();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
            }

            using (new IMGUIBlockWithIndent(new GUIContent("Addressables"))) {
                conf.UseAddressablesInEditor = IMGUILayout.ToggleFiled("Use Addressables InEditor", conf.UseAddressablesInEditor, IMGUIToggleStyle.ToggleType.YesNo);

                conf.ClearAllAddressablesCache = IMGUILayout.ToggleFiled("Clear All Addressables Cache", conf.ClearAllAddressablesCache, IMGUIToggleStyle.ToggleType.YesNo);
                
                using (new IMGUIBeginHorizontal())
                {
                    GUILayout.FlexibleSpace();
                    var active = GUILayout.Button("Build", GUILayout.Width(100));
                    if (active)
                    {
                        BuildScenesPreprocessor.SetupAddressableScenes(EditorUserBuildSettings.activeBuildTarget);
                    }
                }
            }

            CheckNTryAutoSync();
            DrawSettings();

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

                                EditorGUILayout.BeginVertical(GUILayout.Width(235f));
                                {
                                    ReorderableListGUI.Title("Build Targets");

                                    ReorderableListGUI.ListField(platform.BuildTargets, BuildTargetListItem, DrawEmptyPlatform);
                                }
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.BeginVertical();
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

            GUI.BeginGroup(pos);
            {
                const float addressablesToggleWidth = 20.0f;
                const float objectFieldRectWidth = 60.0f;

                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var rect = Rect.zero.WithSize(pos.size);
                var sceneIndexRect = m_ShowBuildIndex ? rect.WithWidth(addressablesToggleWidth) : rect.WithSize(Vector2.zero);
                var objectFieldRect = rect.WithWidth(Mathf.Clamp(rect.width - sceneIndexRect.width - addressablesToggleWidth,
                        objectFieldRectWidth,
                        float.MaxValue))
                    .RightOf(sceneIndexRect);
                var addressableToggleRect = rect.WithWidth(addressablesToggleWidth).RightOf(objectFieldRect).ShiftHorizontally(4.0f);

                if (m_ShowBuildIndex)
                {
                    var sceneIndex = BuildConfigurationSettings.Instance.Configuration.GetSceneIndex(itemValue);
                    GUI.Label(sceneIndexRect, sceneIndex.ToString());
                }

                var sceneAsset = itemValue.GetSceneAsset();
                var sceneSynced = BuildConfigurationSettings.Instance.Configuration
                    .CheckIntersectSceneWhBuildSettings(EditorUserBuildSettings.activeBuildTarget, itemValue.Guid);

                GUI.color = LookForFieldColor(sceneAsset, sceneSynced, itemValue);

                EditorGUI.indentLevel = 0;
                EditorGUI.BeginChangeCheck();
                var newSceneAsset = EditorGUI.ObjectField(objectFieldRect, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
                if (EditorGUI.EndChangeCheck())
                {
                    itemValue.SetSceneAsset(newSceneAsset);
                    
                    if (sceneSynced)
                    {
                        BuildConfigurationSettings.Instance.Configuration.SetupEditorSettings(
                            EditorUserBuildSettings.activeBuildTarget, true);
                    }
                }

                GUI.color = Color.white;

                itemValue.Addressable = GUI.Toggle(addressableToggleRect, itemValue.Addressable, AddressableGuiContent);

                EditorGUI.indentLevel = indentLevel;
            }
            GUI.EndGroup();

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

        static GUIContent s_AddressableGuiContent;

        static GUIContent AddressableGuiContent => s_AddressableGuiContent ?? (s_AddressableGuiContent = new GUIContent("", "Mark scene Addressable?\nIf true - scene will be added as Addressable asset into \"Scenes\" group, otherwise - scene will be added into build settings."));

        public void AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Add Build Settings Scenes to Default"), false, () => {
                var conf = BuildConfigurationSettings.Instance.BuildConfigurations[m_SelectionIndex];
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    var sceneAssetInfo = new SceneAssetInfo();
                    sceneAssetInfo.SetSceneAsset(scene);
                    conf.DefaultScenes.Add(sceneAssetInfo);
                }

                BuildConfigurationSettings.Save();
            });
        }
        
        Color LookForFieldColor(SceneAsset sceneAsset, bool scenesSynced, SceneAssetInfo itemValue)
        {
            var sceneDuplicate = BuildConfigurationSettings.Instance.Configuration
                .CheckSceneDuplicate(EditorUserBuildSettings.activeBuildTarget, itemValue.Guid);
            var sceneWithError = sceneAsset == null;
            var color = Color.white;
            
            if (sceneDuplicate)
            {
                color = s_DuplicateColor;
            }
            else if (sceneWithError)
            {
                color = s_ErrorColor;
            }
            else if (!scenesSynced)
            {
                color = s_OutOfSyncColor;
            }
            
            return color;
        }
        
        void DrawSettings()
        {
            using (new IMGUIBlockWithIndent(new GUIContent("Editor & Build Settings")))
            {
                PreventingDialogs();
                
                var needScenesSync = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();
                if (needScenesSync)
                {
                    DrawMessage(k_ScenesSyncDescription, MessageType.Error,
                        "Fix and sync", SyncScenes);
                    return;
                }

                var hasMissingScenes = BuildConfigurationSettingsValidator.HasMissingScenes();
                if (hasMissingScenes)
                {
                    DrawMessage(k_SceneMissingWarningDescription, MessageType.Warning,
                        "Fix and sync", SyncScenes);
                    return;
                }
                
                var hasDuplicates = BuildConfigurationSettingsValidator.HasScenesDuplicates();
                if (hasDuplicates)
                {
                    DrawMessage(k_RepetitiveScenesWarningDescription,
                        MessageType.Warning);
                    return;
                }
                
                DrawMessage("No issues found with the configuration.", MessageType.Info);
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
            if (!BuildConfigurationSettings.Instance.HasValidConfiguration) return;
            
            BuildConfigurationSettings.Instance.Configuration.ClearMissingScenes();
            
            BuildConfigurationSettings.Instance.Configuration
                .SetupEditorSettings(EditorUserBuildSettings.activeBuildTarget, true);
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
        
        void CheckNTryAutoSync()
        {
            m_AutoSyncParams.NeedScenesSync = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();

            if (m_AutoSyncParams.Synced && m_AutoSyncParams.NeedScenesSync)
            {
                SyncScenes();
            }
            else if (!m_AutoSyncParams.Synced && !m_AutoSyncParams.NeedScenesSync)
            {
                m_AutoSyncParams.Synced = true;
            }
        }

        struct AutoSyncParams
        {
            public bool Synced;
            public bool NeedScenesSync;
        }
    }
}