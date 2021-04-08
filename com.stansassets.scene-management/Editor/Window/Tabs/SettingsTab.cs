#if UNITY_2019_4_OR_NEWER
using System.Collections.Generic;
using StansAssets.Plugins.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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
        }

        void StackRegistered()
        {
            foreach (var stack in StateStackVisualizer.StackMap)
            {
                var stackUI = new VisualElement();
                m_Enums.Add(stackUI);
                stack.OnStackUpdatedPreprocess += (newStackUI) => { StackUpdated(stack, ref stackUI, newStackUI); };
                stack.OnStackUpdatedPostprocess += (newStackUI) => { StackUpdated(stack, ref stackUI, newStackUI); };
            }
        }

        void StackUpdated(StateStackVisualizerItem stack, ref VisualElement stackUI, VisualElement newStackUI)
        {
            if(m_Enums.Contains(stackUI))
                m_Enums.Remove(stackUI);
            
            if (stack.IsActive())
            {
                stackUI =  newStackUI;
                m_Enums.Add(stackUI);
            }
        }
    }
}
#endif