using System;
using JetBrains.Annotations;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace StansAssets.SceneManagement
{
    public class AddressableSceneUnloaderResult
    {
        readonly string m_SceneName;
        readonly AsyncOperationHandle<SceneInstance> m_AsyncOperationHandle;

        public AddressableSceneUnloaderResult(string sceneName, AsyncOperationHandle<SceneInstance> asyncOperationHandle)
        {
            m_SceneName = sceneName;
            m_AsyncOperationHandle = asyncOperationHandle;
        }

        public string SceneName => m_SceneName;
        public AsyncOperationHandle<SceneInstance> AsyncOperationHandle => m_AsyncOperationHandle;
        public Scene Scene => m_AsyncOperationHandle.Result.Scene;
    }

    class AddressableSceneUnloader
    {
        readonly SceneInstance m_SceneInstance;
        readonly string m_SceneName;

        Action<AddressableSceneUnloaderResult> m_Complete;
        AsyncOperationHandle<SceneInstance> m_AsyncOperationHandle;

        public AddressableSceneUnloader(SceneInstance sceneInstance)
        {
            m_SceneInstance = sceneInstance;
            m_SceneName = string.Copy(m_SceneInstance.Scene.name);
            Assert.IsNotNull(m_SceneName);
        }

        public void Unload(Action<AddressableSceneUnloaderResult> complete)
        {
            m_Complete = complete;
            var asyncOperationHandle = Addressables.UnloadSceneAsync(m_SceneInstance);
            asyncOperationHandle.Completed += AddressableSceneUnloaded;
        }

        void AddressableSceneUnloaded(AsyncOperationHandle<SceneInstance> asyncOperation)
        {
            m_AsyncOperationHandle = asyncOperation;
            m_Complete.Invoke(new AddressableSceneUnloaderResult(m_SceneName, m_AsyncOperationHandle));
        }
    }
}
