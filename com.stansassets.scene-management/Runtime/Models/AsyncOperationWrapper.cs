using System;
using UnityEngine;

namespace StansAssets.SceneManagement
{
    class AsyncOperationWrapper : IAsyncOperation
    {
        public event Action<IAsyncOperation> OnComplete;
        public bool IsDone => m_AsyncOperation.isDone;
        public float Progress =>  m_AsyncOperation.progress;

        readonly AsyncOperation m_AsyncOperation;
        
        public AsyncOperationWrapper(AsyncOperation asyncOperation)
        {
            m_AsyncOperation = asyncOperation;
            m_AsyncOperation.completed += operation =>
            {
                OnComplete?.Invoke(this);
            };
        }
    }
}
