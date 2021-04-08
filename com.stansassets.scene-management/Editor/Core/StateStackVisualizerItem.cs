using System;
using System.Collections.Generic;
using System.Linq;
using StansAssets.Foundation.UIElements;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    abstract class StateStackVisualizerItem
    {
        /// <summary>
        /// 
        /// </summary>
        internal Action<VisualElement> OnStackUpdatedPreprocess = delegate { };
        internal Action<VisualElement> OnStackUpdatedPostprocess = delegate { };
        internal abstract string StackName();
        internal abstract bool IsBusy();
        internal abstract bool IsActive();

        //internal abstract VisualElement UpdateStackUIPreprocess();
        //internal abstract VisualElement UpdateStackUIPostprocess();
    }

    class StateStackVisualizerItem<T> : StateStackVisualizerItem where T : Enum
    {
        readonly ApplicationStateStack<T> m_Stack;

        readonly string m_StackName;

        internal StateStackVisualizerItem(ApplicationStateStack<T> stack, string stackName)
        {
            m_Stack = stack;
            //stack.SetVisualizerAction(OnStackUpdated);
            m_Stack.m_VisualizerActionPreprocess += (oldStackState, newStackState) =>
            {
                OnStackUpdatedPreprocess.Invoke(UpdateStackUIPreprocess(oldStackState, newStackState));
            };
            m_Stack.m_VisualizerActionPostprocess += (stackState) => {
                OnStackUpdatedPostprocess.Invoke(UpdateStackUIPostprocess(stackState));
            };
            m_StackName = stackName;
        }

        /// <summary>
        /// 
        /// </summary>
        internal override string StackName() => m_StackName;

        /// <summary>
        /// 
        /// </summary>
        internal override bool IsBusy() => m_Stack.IsBusy;

        /// <summary>
        /// 
        /// </summary>
        internal override bool IsActive() => m_Stack.States.Any();

        private VisualElement UpdateStackUIPreprocess(List<T> oldStackState, List<T> newStackState)
        {
            var container = new VisualElement();
            container.AddToClassList("stack-container");
            var labelHeader = new Label { text = m_StackName };
            labelHeader.AddToClassList("stack-title");
            container.Add(labelHeader);
            labelHeader = new Label { text = "old stack:" };
            labelHeader.AddToClassList("stack-title-small");
            container.Add(labelHeader);
            
            container.Add(ElementStateStack(oldStackState));
            
            container.Add(ElementLoading("New stack Loading..."));
            
            labelHeader = new Label { text = "new stack:" };
            labelHeader.AddToClassList("stack-title-small");
            container.Add(labelHeader);
            
            container.Add(ElementStateStack(newStackState));

            return container;
        }
        
        private VisualElement UpdateStackUIPostprocess(List<T> stateStack)
        {
            var container = new VisualElement();
            container.AddToClassList("stack-container");
            var labelHeader = new Label { text = m_StackName };
            labelHeader.AddToClassList("stack-title");
            container.Add(labelHeader);
            
            container.Add(ElementStateStack(stateStack));

            return container;
        }

        private VisualElement ElementStateStack(List<T> stateStack)
        {
            var containerStates = new VisualElement();
            containerStates.AddToClassList("stack-content");
            var index = 0;
            var count = stateStack.Count();
            foreach (var state in stateStack)
            {
                index++;
                var label = new Label { text = state.ToString() };
                label.AddToClassList("stack-item");
                containerStates.Add(label);
                if (index != count)
                {
                    var labelArrow = new Label { text = "→" };
                    labelArrow.AddToClassList("stack-arrow");
                    containerStates.Add(labelArrow);
                }
            }
            return containerStates;
        }
        
        private VisualElement ElementLoading(string textSpinner)
        {
            var container = new VisualElement();
            container.AddToClassList("stack-loading");
            var label = new Label { text = textSpinner };
            label.AddToClassList("stack-loading-text");
            container.Add(label);
            var spinner = new LoadingSpinner();
            container.Add(spinner);
            return container;
        }
    }
}