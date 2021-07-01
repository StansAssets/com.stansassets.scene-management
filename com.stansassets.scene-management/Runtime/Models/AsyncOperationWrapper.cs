using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace StansAssets.SceneManagement
{
    class AsyncOperationWrapper : IAsyncOperation
    {
        public OperationStatus Status => m_OperationStatus;
        public event Action<IAsyncOperation> OnComplete;
        public SceneInstance SceneInstance { get; }
        public string SceneName { get; }
        public bool IsDone => m_AsyncOperation.isDone;
        public float Progress =>  m_AsyncOperation.progress;

        readonly AsyncOperation m_AsyncOperation;

        OperationStatus m_OperationStatus = OperationStatus.Unknown;

        public AsyncOperationWrapper(string sceneName, AsyncOperation asyncOperation) {
            SceneName = sceneName;
            m_AsyncOperation = asyncOperation;
            m_AsyncOperation.completed += operation =>
            {
                m_OperationStatus = OperationStatus.Success;
                OnComplete?.Invoke(this);
            };
        }
    }
}