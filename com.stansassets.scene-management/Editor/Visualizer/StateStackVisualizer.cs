using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement.StackVisualizer
{
    public static class StateStackVisualizer
    {

        static readonly Dictionary<int, IStateStackVisualizerController> s_StackMap = new Dictionary<int, IStateStackVisualizerController>();
        internal static Action VisualizersCollectionUpdated = delegate {  };
        
        internal static List<IStateStackVisualizerController> StackMap =>
            s_StackMap.Values.ToList();
        internal static List<VisualElement> StackMapVisualElements =>
            s_StackMap.Values.Select(e => e.ViewRoot).ToList();

        public static void Register<T>(ApplicationStateStack<T> stack, string stackName = nameof(T)) where T: Enum
        {
            var view = new StateStackVisualizerView();
            if (!s_StackMap.ContainsKey(stack.GetHashCode()))
            {
                s_StackMap.Add(stack.GetHashCode(), new StateStackVisualizerController<T>(stack, stackName, view));
                VisualizersCollectionUpdated.Invoke();
            }
            else
            {
                throw new NotImplementedException(
                    $"An attempt to register an already registered stack: {stackName}");
            }


        }
    }
}
