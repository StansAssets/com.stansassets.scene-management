using System;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement.StackVisualizer
{
    interface IStateStackVisualizerController<T> : IStateStackVisualizerController, IApplicationStateDelegate<T>
        where T : Enum
    {
    }

    interface IStateStackVisualizerController
    {
        string StackName { get; }
        VisualElement ViewRoot { get; }
    }
}
