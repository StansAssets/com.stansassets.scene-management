using System.Collections.Generic;
using System.Linq;
using StansAssets.Foundation.UIElements;
using StansAssets.Plugins.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    public class StateStackVisualizerView : BaseTab, IStateStackVisualizerView
    {
        Label m_header;
        VisualElement m_container;
        ProgressBar m_progressBar;

        public StateStackVisualizerView() : base($"{SceneManagementPackage.VisualizerViewPath}/StateStackVisualizerView")
        {
            Root.AddToClassList("root");
            m_header = new Label();
            m_header.AddToClassList("stack-title");
            m_container = new VisualElement();
            m_container.AddToClassList("stack-container");
            m_progressBar = new ProgressBar();
            m_progressBar.AddToClassList("stack-progress-bar");
            Root.Add(m_header);
            Root.Add(m_container);
            Root.Add(m_progressBar);
            
            m_progressBar.style.display = DisplayStyle.None;
        }

        public void ShowView(bool show)
        {
            Root.style.display = (show)? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetStackName(string stackName)
        {
            m_header.text = stackName;
        }

        public void SetTwoStack(IEnumerable<StackVisualModel> oldStack, IEnumerable<StackVisualModel> newStack)
        {
            m_progressBar.style.display = DisplayStyle.Flex;
            m_progressBar.value = 0;
            m_container.Clear();

            m_container.Add(ElementStateStack(oldStack.ToList()));
            
            var labelArrow = new Label {text = "→"};
            labelArrow.AddToClassList("stack-arrow");
            m_container.Add(labelArrow);

            m_container.Add(ElementStateStack(newStack.ToList()));
        }

        public void SetStack(IEnumerable<StackVisualModel> stack)
        {
            m_progressBar.style.display = DisplayStyle.None;
            m_progressBar.value = 0;
            m_container.Clear();

            m_container.Add(ElementStateStack(stack.ToList()));
        }

        public void UpdateProgress(float progress, string title)
        {
            m_progressBar.value = progress;
            m_progressBar.title = title;
        }

        VisualElement ElementStateStack(List<StackVisualModel> stateStack)
        {
            var containerStates = new VisualElement();
            containerStates.AddToClassList("stack-content");
            
            foreach (var state in stateStack)
            {
                var label = new Label {text = state.Title};
                label.AddToClassList("stack-item");
                
                if(state.Status == StackVisualItemStatus.Active)
                    label.AddToClassList("stack-item-active");
                
                containerStates.Add(label);
            }

            return containerStates;
        }
    }
}
