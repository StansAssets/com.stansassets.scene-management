#if UNITY_2019_4_OR_NEWER
using StansAssets.Plugins.Editor;
using StansAssets.SceneManagement.StackVisualizer;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    public class SettingsTab : BaseTab
    {
        readonly VisualElement m_StackVisualizersRoot;

        public SettingsTab()
            : base($"{SceneManagementPackage.WindowTabsPath}/SettingsTab")
        {
            var landingSceneField = Root.Q<ObjectField>("landing-scene");
            landingSceneField.objectType = typeof(SceneAsset);
            landingSceneField.SetValueWithoutNotify(SceneManagementSettings.Instance.LandingScene);

            landingSceneField.RegisterValueChangedCallback((e) =>
            {
                SceneManagementSettings.Instance.LandingScene = (SceneAsset)e.newValue;
                SceneManagementSettings.Save();
            });
            m_StackVisualizersRoot = this.Q<VisualElement>("StackVisualizersRoot");
            StateStackVisualizer.VisualizersCollectionUpdated += SubscribeVisualizationStacks;
            EditorApplication.playModeStateChanged += ModeChanged;
            SubscribeVisualizationStacks();
        }

        void SubscribeVisualizationStacks()
        {
            m_StackVisualizersRoot.Clear();
            foreach (var stack in StateStackVisualizer.StackMapVisualElements)
            {
                m_StackVisualizersRoot.Add(stack);
            }
        }

        void ModeChanged (PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode ) 
            {
                m_StackVisualizersRoot.Clear();
            }
        }
    }
}
#endif