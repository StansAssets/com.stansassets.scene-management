#if UNITY_2019_4_OR_NEWER
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    public class SettingsTab : BaseTab
    {
        readonly VisualElement m_Enums;

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
            m_Enums = this.Q<VisualElement>("enums");
            StateStackVisualizer.StackRegistered += StackRegistered;
            EditorApplication.playModeStateChanged += ModeChanged;
        }

        void StackRegistered(VisualElement stack)
        {
            m_Enums.Add(stack);
        }
        
        void ModeChanged (PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode ) 
            {
                m_Enums.Clear();
            }
        }
    }
}
#endif