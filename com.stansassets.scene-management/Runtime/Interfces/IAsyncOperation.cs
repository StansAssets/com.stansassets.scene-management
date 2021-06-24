using System;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace StansAssets.SceneManagement
{
    public interface IAsyncOperation
    {
        SceneInstance SceneInstance { get; }
        string SceneName { get; }
        bool IsDone { get; }
        float Progress { get; }
        OperationStatus Status { get; }
        event Action<IAsyncOperation> OnComplete;
    }
}
