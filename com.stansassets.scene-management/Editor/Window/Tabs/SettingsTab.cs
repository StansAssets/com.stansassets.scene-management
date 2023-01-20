#if UNITY_2019_4_OR_NEWER
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    public class SettingsTab : BaseTab
    {
        readonly VisualElement m_StackVisualizersRoot;
        readonly EnumField m_PersistenceEnumField;

        public SettingsTab()
            : base($"{SceneManagementPackage.WindowTabsPath}/SettingsTab")
        {
            var landingSceneField = Root.Q<ObjectField>("landing-scene");
            m_PersistenceEnumField = Root.Q<EnumField>("persistence-enum-field");
            landingSceneField.objectType = typeof(SceneAsset);
            landingSceneField.SetValueWithoutNotify(SceneManagementSettings.Instance.LandingScene);

            landingSceneField.RegisterValueChangedCallback((e) =>
            {
                SceneAsset newSceneAsset = (SceneAsset)e.newValue;
                SceneManagementSettings.Instance.LandingScene = newSceneAsset;
                SceneManagementSettings.Save();
                DisplayPersistenceEnumField(newSceneAsset != null);
            });

            CreatePersistenceEnumField();

            m_StackVisualizersRoot = this.Q<VisualElement>("StackVisualizersRoot");
            StackVisualizer.StackVisualizer.OnVisualizersCollectionUpdated += SubscribeVisualizationStacks;
            EditorApplication.playModeStateChanged += ModeChanged;
            SubscribeVisualizationStacks();
        }

        void SubscribeVisualizationStacks()
        {
            m_StackVisualizersRoot.Clear();
            foreach (var stack in StackVisualizer.StackVisualizer.VisualizersRoots)
            {
                m_StackVisualizersRoot.Add(stack);
            }
        }

        void ModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                m_StackVisualizersRoot.Clear();
            }
        }

        void CreatePersistenceEnumField()
        {
            m_PersistenceEnumField.Init(IMGUIToggleStyle.YesNoBool.No);
            m_PersistenceEnumField.tooltip = "tooltip";
            bool useCameraAndScenePersistence = SceneManagementSettings.Instance.UseCameraAndScenePersistence;
            IMGUIToggleStyle.YesNoBool cachedValue = useCameraAndScenePersistence ? IMGUIToggleStyle.YesNoBool.Yes : IMGUIToggleStyle.YesNoBool.No;
            m_PersistenceEnumField.SetValueWithoutNotify(cachedValue);
            m_PersistenceEnumField.RegisterValueChangedCallback(eventValue =>
            {
                var selectedOption = (IMGUIToggleStyle.YesNoBool)eventValue.newValue;
                SceneManagementSettings.Instance.UseCameraAndScenePersistence = selectedOption == IMGUIToggleStyle.YesNoBool.Yes;
            });
        }

        void DisplayPersistenceEnumField(bool state)
        {
            DisplayStyle displayStyle = state ? DisplayStyle.Flex : DisplayStyle.None;
            m_PersistenceEnumField.style.display = displayStyle;
        }
    }
}
#endif