using System;

namespace StansAssets.SceneManagement
{
    public interface IAsyncOperation
    {
        bool IsDone { get; }
        float Progress { get; }
        event Action<IAsyncOperation> OnComplete;
    }
}
