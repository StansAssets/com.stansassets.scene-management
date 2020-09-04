using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace StansAssets.SceneManagement
{
    class AsyncOperationHandleWrapper<T> : IAsyncOperation
    {
        public event Action<IAsyncOperation> OnComplete;
        public bool IsDone => m_AsyncOperationHandle.IsDone;
        public float Progress =>  m_AsyncOperationHandle.PercentComplete;
        
        AsyncOperationHandle<T> m_AsyncOperationHandle;
        public AsyncOperationHandleWrapper(AsyncOperationHandle<T> asyncOperationHandle)
        {
            m_AsyncOperationHandle = asyncOperationHandle;
            m_AsyncOperationHandle.Completed += handle =>
            {
                OnComplete?.Invoke(this);
            };
        }
    }
}
