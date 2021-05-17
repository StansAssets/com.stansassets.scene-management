using System;
using UnityEngine.UIElements;

namespace StansAssets.SceneManagement.StackVisualizer
{
    interface IStackVisualizerController<T> : IStackVisualizerController, IApplicationStateDelegate<T>
        where T : Enum
    {
    }

    interface IStackVisualizerController
    {
        VisualElement ViewRoot { get; }
    }
}
