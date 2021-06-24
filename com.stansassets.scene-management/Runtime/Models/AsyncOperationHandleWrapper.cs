using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace StansAssets.SceneManagement
{
    class AsyncOperationHandleWrapper : IAsyncOperation
    {
        public OperationStatus Status => m_OperationStatus;
        public event Action<IAsyncOperation> OnComplete;
        public SceneInstance SceneInstance => m_AsyncOperationHandle.Result;
        public string SceneName { get; }
        public bool IsDone => m_AsyncOperationHandle.IsDone;
        public float Progress =>  m_AsyncOperationHandle.PercentComplete;

        readonly AsyncOperationHandle<SceneInstance> m_AsyncOperationHandle;

        private OperationStatus m_OperationStatus = OperationStatus.Unknown;

        public AsyncOperationHandleWrapper(string sceneName, AsyncOperationHandle<SceneInstance> asyncOperationHandle) {
            SceneName = sceneName;
            m_AsyncOperationHandle = asyncOperationHandle;
            m_AsyncOperationHandle.Completed += handle => {
                m_OperationStatus = asyncOperationHandle.Status == AsyncOperationStatus.Succeeded ? OperationStatus.Success : OperationStatus.Fail;
                OnComplete?.Invoke(this);
            };
        }
    }
}
