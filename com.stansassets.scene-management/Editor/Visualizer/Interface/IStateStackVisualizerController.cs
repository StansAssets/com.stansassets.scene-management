using System;

namespace StansAssets.SceneManagement
{
    interface IStateStackVisualizerController<T> : IStateStackVisualizerController, IApplicationStateDelegate<T>
        where T : Enum
    {
    }

    interface IStateStackVisualizerController
    {
        string StackName { get; }
        bool IsBusy { get; }
        bool IsActive { get; }
    }
}