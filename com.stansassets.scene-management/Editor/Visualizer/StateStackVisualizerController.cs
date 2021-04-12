using System;
using System.Collections.Generic;
using System.Linq;

namespace StansAssets.SceneManagement
{
    class StateStackVisualizerController<T> : IStateStackVisualizerController<T> where T : Enum
    {
        readonly ApplicationStateStack<T> m_Stack;
        readonly IStateStackVisualizerView m_View;

        internal StateStackVisualizerController(ApplicationStateStack<T> stack, string stackName, IStateStackVisualizerView view)
        {
            m_Stack = stack;
            StackName = stackName;
            m_View = view;
            m_View.SetStackName(StackName);
            m_View.ShowView(false);

            m_Stack.AddDelegate(this);
        }

        public string StackName { get; }

        public bool IsBusy => m_Stack.IsBusy;
        public bool IsActive => m_Stack.States.Any();

        public void OnApplicationStateWillChanged(StackOperationEvent<T> e)
        {
            m_View.ShowView(true);
            var oldStack = CreateVisualStack(e.OldStackValue);
            var newStack = CreateVisualStack(e.NewStackValue);
            m_View.SetTwoStack(oldStack, newStack);
        }

        public void ApplicationStateChangeProgressChanged(float progress, StackChangeEvent<T> e)
        {
            m_View.UpdateProgress(progress, $"{e.State}: {e.Action}");
        }

        public void ApplicationStateChanged(StackOperationEvent<T> e)
        {
            m_View.ShowView(true);
            var newStack = CreateVisualStack(e.NewStackValue);
            m_View.SetStack(newStack);
        }

        static IEnumerable<StackVisualModel> CreateVisualStack(IEnumerable<T> stack)
        {
            var newStack = stack.Select(st => new StackVisualModel() {Title = st.ToString()[0].ToString().ToUpper()}).ToList();
            
            if(newStack.Any())
                newStack.Last().Status = StackVisualItemStatus.Active;

            newStack.Reverse();
            return newStack; 
        }
    }
}
