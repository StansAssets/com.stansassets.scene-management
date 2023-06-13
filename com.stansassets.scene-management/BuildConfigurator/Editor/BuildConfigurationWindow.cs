using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;
using Rotorz.ReorderableList.Internal;
using StansAssets.Plugins.Editor;
using StansAssets.SceneManagement.Utilities;
using UnityEditorInternal;

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
        const string k_RepetitiveBuildTargetsWarningDescription = "Your configuration has duplicated build targets, consider fixing it.";

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

        ReorderableList m_DefaultScenesList;

        readonly Dictionary<PlatformsConfiguration, (ReorderableList platforms, ReorderableList scenes)>
            m_ReorderableLists
                = new Dictionary<PlatformsConfiguration, (ReorderableList platforms, ReorderableList scenes)>();

        internal void UpdateStatus()
        {
            EditorBuildSettingsSceneListChanged();
        }
        
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
            UpdateStatus();

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
            // TODO: Bug?
            // Try to comment this code and ReorderableListResources won't work!
            var ap = ReorderableListStyles.Title;
            // ~ bug

            var conf = BuildConfigurationSettings.Instance.BuildConfigurations[index];
            using (new IMGUIBlockWithIndent(new GUIContent("Settings")))
            {
                conf.Name = IMGUILayout.TextField("Configuration Name:", conf.Name);
                
                EditorGUI.BeginChangeCheck();
                conf.DefaultScenesFirst = IMGUILayout.ToggleFiled("Default Scenes First", conf.DefaultScenesFirst, IMGUIToggleStyle.ToggleType.YesNo);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_AutoSyncParams.Synced)
                    {
                        SyncScenes();
                    }
                }

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
                        CheckNTryAutoSync(true);
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
            if (m_DefaultScenesList == null)
            {
                m_DefaultScenesList = CreateScenesReorderableList(conf.DefaultScenes, false);
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

            m_ShowBuildIndex = true;
        }

        void DrawPlatform(BuildConfiguration conf, PlatformsConfiguration platform)
        {
            m_ShowBuildIndex = conf.IsActive(platform);
            
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
                        CheckNTryAutoSync(true);
                        
                        GUIUtility.ExitGUI();
                    }

                    GUILayout.Space(-5);
                }
            }
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
        
        Color LookForFieldColor(SceneAsset sceneAsset, bool scenesSynced, SceneAssetInfo itemValue,
            ReorderableList reorderableList)
        {
            var sceneDuplicate = BuildConfigurationSettings.Instance.Configuration
                .CheckSceneDuplicate(EditorUserBuildSettings.activeBuildTarget, itemValue.Guid);

            if (!sceneDuplicate && itemValue.Guid != null && !string.IsNullOrEmpty(itemValue.Guid))
            {
                var scenes = new SceneAssetInfo[reorderableList.list.Count];
                reorderableList.list.CopyTo(scenes, 0);

                var itemPath = AssetDatabase.GUIDToAssetPath(itemValue.Guid);
                sceneDuplicate = scenes.Count(i =>
                    itemValue.Guid.Equals(i.Guid) || AssetDatabase.GUIDToAssetPath(i.Guid).Equals(itemPath)) > 1;
            }
            
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
            
            m_AutoSyncParams.Synced = true;
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

        void CheckNTryAutoSync(bool ignoreCollectionsSize = false)
        {
            var hasAnyScene = BuildConfigurationSettingsValidator.HasAnyScene();
            if (!hasAnyScene)
            {
                m_AutoSyncParams.Synced = false;
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
                    m_AutoSyncParams.Synced = false;
                    return;
                }
            }
            
            m_AutoSyncParams.NeedScenesSync = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();

            if (m_AutoSyncParams.Synced && m_AutoSyncParams.NeedScenesSync)
            {
                SyncScenes();
                m_AutoSyncParams.Synced = true;
            }
            else if (!m_AutoSyncParams.Synced && !m_AutoSyncParams.NeedScenesSync)
            {
                m_AutoSyncParams.Synced = true;
            }
        }

        void EditorBuildSettingsSceneListChanged()
        {
            m_AutoSyncParams.NeedScenesSync = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();
            m_AutoSyncParams.Synced = !m_AutoSyncParams.NeedScenesSync;
        }

        ReorderableList CreateScenesReorderableList(IList elementsList, bool showBuildIndex)
        {
            var reorderableList = new ReorderableList(elementsList, typeof(SceneAssetInfo),
                true, true, true, false)
            {
                showDefaultBackground = false,
                footerHeight = 18f,
                elementHeightCallback = i => 22f,
            };

            // Draw element
            reorderableList.drawNoneElementCallback =
                rect => EditorGUI.LabelField(rect, "Add a scene", EditorStyles.miniLabel);
            
            reorderableList.drawElementCallback = (rect, index, active, focused) =>
                DrawSceneListItem(rect, index, reorderableList);
            
            reorderableList.drawElementBackgroundCallback = (rect, i, b, focused) =>
                DrawListItemBackground(rect, i, focused, reorderableList);

            // Head/foot
            reorderableList.drawHeaderCallback = rect =>
                DrawListHeaderCallback(rect, "Scenes", showBuildIndex && m_ShowBuildIndex);
            
            reorderableList.drawFooterCallback = rect => DrawListFooterCallback(rect, reorderableList);

            // Actions
            reorderableList.onChangedCallback = list => { CheckNTryAutoSync(); };

            reorderableList.onReorderCallback = list => { CheckNTryAutoSync(); };

            reorderableList.onRemoveCallback = list => { CheckNTryAutoSync(true); };

            return reorderableList;
        }

        ReorderableList CreatePlatformsReorderableList(IList elementsList)
        {
            var reorderableList = new ReorderableList(elementsList, typeof(BuildTargetRuntime),
                true, true, true, false)
            {
                showDefaultBackground = false,
                footerHeight = 18f,
                elementHeightCallback = i => 22f,
            };

            // Draw element
            reorderableList.drawNoneElementCallback = rect =>
                EditorGUI.LabelField(rect, "Add a build target", EditorStyles.miniLabel);
            
            reorderableList.drawElementCallback = (rect, index, active, focused) =>
                DrawBuildTargetListItem(rect, index, reorderableList);
            
            reorderableList.drawElementBackgroundCallback = (rect, i, b, focused) =>
                DrawListItemBackground(rect, i, focused, reorderableList);

            // Head/foot
            reorderableList.drawHeaderCallback =
                rect => DrawListHeaderCallback(rect, "Build Targets", m_ShowBuildIndex);
            
            reorderableList.drawFooterCallback = rect => DrawListFooterCallback(rect, reorderableList);

            // Actions
            reorderableList.onChangedCallback = list => { CheckNTryAutoSync(true); };

            reorderableList.onRemoveCallback = list => { CheckNTryAutoSync(true); };

            return reorderableList;
        }

        (ReorderableList platforms, ReorderableList scenes) GetPlatformReorderableList(PlatformsConfiguration platform)
        {
            if (m_ReorderableLists.ContainsKey(platform)) return m_ReorderableLists[platform];

            var platforms = CreatePlatformsReorderableList(platform.BuildTargets);
            var scenes = CreateScenesReorderableList(platform.Scenes, true);

            m_ReorderableLists.Add(platform, (platforms, scenes));

            return m_ReorderableLists[platform];
        }

        void DrawListHeaderCallback(Rect rect, string titleText, bool showBuildIndex = false)
        {
            var style = ReorderableListStyles.Title;

            rect.x -= 20f;
            rect.width += 25f;

            GUI.backgroundColor = showBuildIndex ? GUI.skin.settings.selectionColor : Color.white;
            EditorGUI.LabelField(rect, titleText, style);
            GUI.backgroundColor = Color.white;
        }

        void DrawListFooterCallback(Rect rect, ReorderableList reorderableList)
        {
            var removeButtonRect = rect.RightOf(rect).ShiftHorizontally(-28.0f);
            removeButtonRect.width = 24f;
            removeButtonRect.height = 18f;

            var iconNormal = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Add_Normal);
            var iconActive = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Add_Active);

            var removeButton = GUIHelper.IconButton(removeButtonRect, true, iconNormal, iconActive,
                ReorderableListStyles.ItemButton);
            if (removeButton)
            {
                var elementType = reorderableList.list.GetType().GetGenericArguments().Single();
                reorderableList.list.Add(Activator.CreateInstance(elementType));
            }
        }

        void DrawListItemBackground(Rect rect, int index, bool isFocused, ReorderableList reorderableList)
        {
            if (!isFocused && reorderableList.count - 1 <= index)
            {
                return;
            }

            var startPos = new Vector2(rect.xMin, rect.yMax);
            var endPos = new Vector2(rect.xMax, rect.yMax);

            Handles.color = isFocused
                ? ReorderableListStyles.SelectionBackgroundColor
                : ReorderableListStyles.HorizontalLineColor;
            Handles.DrawLine(startPos, endPos);
            Handles.color = Color.white;
        }

        void DrawSceneListItem(Rect rect, int index, ReorderableList reorderableList)
        {
            var itemValue = reorderableList.list[index] as SceneAssetInfo ?? new SceneAssetInfo();

            rect.y += 1f;
            rect.height = 18f;

            GUI.BeginGroup(rect);
            {
                const float addressablesToggleWidth = 20.0f;
                const float removeButtonWidth = 24.0f;
                const float objectFieldRectWidth = 60.0f;
                const float removeButtonRectWidth = 24;

                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var positionRect = Rect.zero.WithSize(rect.size);
                var sceneIndexRect = m_ShowBuildIndex
                    ? positionRect.WithWidth(addressablesToggleWidth)
                    : positionRect.WithSize(Vector2.zero);
                var objectFieldRect = positionRect
                    .WithWidth(
                        Mathf.Clamp(
                            positionRect.width - sceneIndexRect.width - addressablesToggleWidth - removeButtonWidth,
                            objectFieldRectWidth, float.MaxValue)
                    )
                    .RightOf(sceneIndexRect);

                var addressableToggleRect = positionRect.WithWidth(addressablesToggleWidth).RightOf(objectFieldRect)
                    .ShiftHorizontally(4.0f);

                if (m_ShowBuildIndex)
                {
                    var sceneIndex = BuildConfigurationSettings.Instance.Configuration.GetSceneIndex(itemValue, EditorUserBuildSettings.activeBuildTarget);
                    GUI.Label(sceneIndexRect, sceneIndex.ToString());
                }

                var sceneAsset = itemValue.GetSceneAsset();
                var sceneSynced = BuildConfigurationSettings.Instance.Configuration
                    .CheckIntersectSceneWhBuildSettings(EditorUserBuildSettings.activeBuildTarget, itemValue.Guid);

                GUI.color = LookForFieldColor(sceneAsset, sceneSynced, itemValue, reorderableList);

                EditorGUI.indentLevel = 0;
                EditorGUI.BeginChangeCheck();
                var newSceneAsset =
                    EditorGUI.ObjectField(objectFieldRect, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
                if (EditorGUI.EndChangeCheck())
                {
                    itemValue.SetSceneAsset(newSceneAsset);
                    reorderableList.onChangedCallback?.Invoke(reorderableList);
                }

                GUI.color = Color.white;

                itemValue.Addressable = GUI.Toggle(addressableToggleRect, itemValue.Addressable, AddressableGuiContent);

                var removeButtonRect = positionRect.WithWidth(removeButtonWidth).RightOf(addressableToggleRect)
                    .ShiftHorizontally(-2.0f);
                removeButtonRect.width = removeButtonRectWidth;

                DrawRemoveButtonOfListElement(removeButtonRect, reorderableList, index);

                EditorGUI.indentLevel = indentLevel;
            }
            GUI.EndGroup();
        }

        void DrawBuildTargetListItem(Rect rect, int index, ReorderableList reorderableList)
        {
            var element = (BuildTargetRuntime)reorderableList.list[index];

            rect.y += 1f;
            rect.height = 18f;

            const float removeButtonWidth = 24.0f;

            var positionRect = new Rect(rect);
            positionRect.width -= removeButtonWidth;

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var style = new GUIStyle(EditorStyles.popup)
            {
                fontStyle = element == (BuildTargetRuntime)EditorUserBuildSettings.activeBuildTarget ||
                            element == BuildTargetRuntime.Editor
                    ? FontStyle.Bold
                    : FontStyle.Normal
            };

            EditorGUI.BeginChangeCheck();
            
            GUI.color = LookForBuildTargetFieldColor(element);
            element = (BuildTargetRuntime)EditorGUI.EnumPopup(positionRect, element, style);
            GUI.color = Color.white;
            
            if (EditorGUI.EndChangeCheck())
            {
                reorderableList.list[index] = element;
                reorderableList.onChangedCallback?.Invoke(reorderableList);
            }

            EditorGUI.indentLevel = indentLevel;

            var removeButtonRect = rect.RightOf(positionRect).ShiftHorizontally(+2.0f);
            removeButtonRect.width = removeButtonWidth;

            DrawRemoveButtonOfListElement(removeButtonRect, reorderableList, index);
        }

        Color LookForBuildTargetFieldColor(BuildTargetRuntime buildTargetRuntime)
        {
            if (buildTargetRuntime == BuildTargetRuntime.NoTarget)
            {
                return new Color(1f, 0.95f, 0.7f);
            }
            
            var hasDuplicates = BuildConfigurationSettings.Instance.Configuration
                .CheckBuildTargetDuplicate(buildTargetRuntime);

            return hasDuplicates ? s_DuplicateColor : Color.white;
        }

        void DrawRemoveButtonOfListElement(Rect rect, ReorderableList reorderableList, int index)
        {
            var iconNormal = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Remove_Normal);
            var iconActive = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Remove_Active);

            var removeButton = GUIHelper.IconButton(rect, true, iconNormal, iconActive, ReorderableListStyles.ItemButton);
            if (removeButton)
            {
                reorderableList.list.Remove(reorderableList.list[index]);
                reorderableList.onRemoveCallback?.Invoke(reorderableList);
                
                GUIUtility.ExitGUI();
            }
        }
        
        struct AutoSyncParams
        {
            public bool Synced;
            public bool NeedScenesSync;
        }
    }
}