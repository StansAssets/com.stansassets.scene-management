using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;
using StansAssets.Plugins.Editor;

namespace StansAssets.SceneManagement.Build
{
    class BuildConfigurationWindow : IMGUISettingsWindow<BuildConfigurationWindow>, IHasCustomMenu
    {
        [SerializeField]
        IMGUIHyperLabel m_AddButton;

        BuildConfigurationContext m_Context;

        SettingsView m_SettingsView;
        DefaultScenesView m_DefaultSceneView;
        PlatformsView m_PlatformsView;
        
        int m_SelectionIndex;

        BuildConfigurationContext Context => m_Context ??= new BuildConfigurationContext();

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
            
            m_SettingsView = new SettingsView(Context);
            m_DefaultSceneView = new DefaultScenesView(Context);
            m_PlatformsView = new PlatformsView(Context);

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
                    if (Context.AutoSyncParams.Synced)
                    {
                        Context.SyncScenes();
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
                        Context.CheckNTryAutoSync(true);
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

            m_SettingsView.DrawSettings();
            
            if (conf.DefaultScenesFirst)
            {
                m_DefaultSceneView.DrawDefaultScenes(conf);
                m_PlatformsView.DrawPlatforms(conf);
            }
            else
            {
                m_PlatformsView.DrawPlatforms(conf);
                m_DefaultSceneView.DrawDefaultScenes(conf);
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

        void EditorBuildSettingsSceneListChanged()
        {
            Context.AutoSyncParams.NeedScenesSync = BuildConfigurationSettingsValidator.CompareScenesWithBuildSettings();
            Context.AutoSyncParams.Synced = !Context.AutoSyncParams.NeedScenesSync;
        }
    }
}