using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement
{
    public static class StateStackVisualizer
    {

        static readonly List<IStateStackVisualizerController> StackMap = new List<IStateStackVisualizerController>();
        internal static Action<VisualElement> StackRegistered  = delegate {  };

        public static void Register<T>(ApplicationStateStack<T> stack, string stackName) where T: Enum
        {
            var view = new StateStackVisualizerView();
            StackMap.Add(new StateStackVisualizerController<T>(stack, stackName, view));
            StackRegistered.Invoke(view.Root);
        }
    }
}