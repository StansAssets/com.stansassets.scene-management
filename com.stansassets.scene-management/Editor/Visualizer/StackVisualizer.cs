using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement.StackVisualizer
{
    public static class StackVisualizer
    {
        static readonly Dictionary<int, IStackVisualizerController> s_VisualizersMap = new Dictionary<int, IStackVisualizerController>();
        internal static Action VisualizersCollectionUpdated = delegate {  };

        internal static List<IStackVisualizerController> Visualizers =>
            s_VisualizersMap.Values.ToList();
        internal static List<VisualElement> VisualizersRoots =>
            s_VisualizersMap.Values.Select(e => e.ViewRoot).ToList();

        public static void Register<T>(ApplicationStateStack<T> stack, string stackName = nameof(T)) where T: Enum
        {
            int stackId = GetUniqueIdForStack(stack);

            if (s_VisualizersMap.ContainsKey(stackId))
            {
                throw new ArgumentException($"An attempt to register an already registered stack: {stackName}");
            }

            var view = new StackVisualizerView();
            s_VisualizersMap.Add(stackId, new StackVisualizerController<T>(stack, stackName, view));
            VisualizersCollectionUpdated.Invoke();
        }

        static int GetUniqueIdForStack<T>(ApplicationStateStack<T> stack) where T : Enum
        {
            return stack.GetHashCode();
        }
    }
}
