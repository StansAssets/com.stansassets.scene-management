using System;
using System.Collections.Generic;
using System.Linq;
using StansAssets.SceneManagement.StackVisualizer.Utility;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement.StackVisualizer
{
    class StackVisualizerController<T> : IStackVisualizerController<T> where T : Enum
    {
        readonly ApplicationStateStack<T> m_Stack;
        readonly IStackVisualizerView m_View;

        public VisualElement ViewRoot => m_View.Root;

        internal StackVisualizerController(ApplicationStateStack<T> stack, string stackName, IStackVisualizerView view)
        {
            m_Stack = stack;
            m_View = view;

            m_View.SetStackName(stackName);
            m_View.ShowView(false);

            m_Stack.AddDelegate(this);
        }

        public void ApplicationStateWillChange(StackOperationEvent<T> e)
        {
            m_View.ShowView(true);
            var oldStack = StackVisualizerUtility.CreateTemplatesFor(e.OldStackValue);
            var newStack = StackVisualizerUtility.CreateTemplatesFor(e.NewStackValue);
            m_View.SetStackChange(oldStack, newStack);
        }

        public void ApplicationStateChangeProgressUpdated(float progress, StackChangeEvent<T> e)
        {
            m_View.UpdateProgress(progress, $"{e.State}: {e.Action}");
        }

        public void ApplicationStateChanged(StackOperationEvent<T> e)
        {
            m_View.ShowView(true);
            var newStack = StackVisualizerUtility.CreateTemplatesFor(e.NewStackValue);
            m_View.SetStack(newStack);
        }
    }
}
