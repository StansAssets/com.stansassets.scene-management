using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement.StackVisualizer
{
    class StateStackVisualizerController<T> : IStateStackVisualizerController<T> where T : Enum
    {
        readonly ApplicationStateStack<T> m_Stack;
        readonly IStateStackVisualizerView m_View;
        
        static Dictionary<T, string> s_StackTitles;
        
        public VisualElement ViewRoot => m_View.Root;
        public string StackName { get; }

        internal StateStackVisualizerController(ApplicationStateStack<T> stack, string stackName, IStateStackVisualizerView view)
        {
            m_Stack = stack;
            StackName = stackName;
            m_View = view;
            m_View.SetStackName(StackName);
            m_View.ShowView(false);
            CreateStackTitles();

            m_Stack.AddDelegate(this);
        }

        public void OnApplicationStateWillChanged(StackOperationEvent<T> e)
        {
            m_View.ShowView(true);
            var oldStack = CreateTemplatesFor(e.OldStackValue);
            var newStack = CreateTemplatesFor(e.NewStackValue);
            m_View.SetStackChange(oldStack, newStack);
        }

        public void ApplicationStateChangeProgressChanged(float progress, StackChangeEvent<T> e)
        {
            m_View.UpdateProgress(progress, $"{e.State}: {e.Action}");
        }

        public void ApplicationStateChanged(StackOperationEvent<T> e)
        {
            m_View.ShowView(true);
            var newStack = CreateTemplatesFor(e.NewStackValue);
            m_View.SetStack(newStack);
        }

        static IEnumerable<VisualStackTemplate> CreateTemplatesFor(IEnumerable<T> stack)
        {
            var newStack = stack.Select(st => new VisualStackTemplate() {Title = s_StackTitles[st]}).ToList();
            
            if(newStack.Any())
                newStack.Last().Status = VisualStackItemStatus.Active;
            
            // Reverse to display the stack from top to bottom
            newStack.Reverse();
            return newStack; 
        }
        
        void CreateStackTitles()
        {
            s_StackTitles = new Dictionary<T, string>();
            foreach (var enumItem in (T[]) Enum.GetValues(typeof(T)))
            {
                s_StackTitles.Add(enumItem, enumItem.ToString()[0].ToString().ToUpper());
            }
        }
    }
}
