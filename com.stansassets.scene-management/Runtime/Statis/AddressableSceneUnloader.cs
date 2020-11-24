using System;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace StansAssets.SceneManagement
{
    public class AddressableSceneUnloaderResult
    {
        public string SceneName { get; }
        public AsyncOperationHandle<SceneInstance> AsyncOperationHandle { get; }

        public AddressableSceneUnloaderResult(string sceneName, AsyncOperationHandle<SceneInstance> asyncOperationHandle) {
            SceneName = sceneName;
            AsyncOperationHandle = asyncOperationHandle;
        }
    }

    class AddressableSceneUnloader
    {
        readonly SceneInstance m_SceneInstance;
        readonly string m_SceneName;

        Action<AddressableSceneUnloaderResult> m_Complete;
        AsyncOperationHandle<SceneInstance> m_AsyncOperationHandle;

        public AddressableSceneUnloader(SceneInstance sceneInstance) {
            m_SceneInstance = sceneInstance;
            m_SceneName = string.Copy(m_SceneInstance.Scene.name);
            Assert.IsNotNull(m_SceneName);
        }

        public void Unload(Action<AddressableSceneUnloaderResult> complete) {
            m_Complete = complete;
            var asyncOperationHandle = Addressables.UnloadSceneAsync(m_SceneInstance);
            asyncOperationHandle.Completed += AddressableSceneUnloaded;
        }

        void AddressableSceneUnloaded(AsyncOperationHandle<SceneInstance> asyncOperation) {
            m_AsyncOperationHandle = asyncOperation;
            m_Complete.Invoke(new AddressableSceneUnloaderResult(m_SceneName, m_AsyncOperationHandle));
        }
    }
}
