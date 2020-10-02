using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace StansAssets.SceneManagement
{
    class AddressableSceneUnloader
    {
        readonly SceneInstance m_SceneInstance;
        readonly string m_SceneName;

        Action<AddressableSceneUnloader> m_Complete;
        AsyncOperationHandle<SceneInstance> m_AsyncOperationHandle;

        public AddressableSceneUnloader(SceneInstance sceneInstance)
        {
            m_SceneInstance = sceneInstance;
            m_SceneName = string.Copy(m_SceneInstance.Scene.name);
        }

        public void Unload(Action<AddressableSceneUnloader> complete)
        {
            m_Complete = complete;
            var asyncOperationHandle = Addressables.UnloadSceneAsync(m_SceneInstance);
            asyncOperationHandle.Completed += AddressableSceneUnloaded;
        }

        void AddressableSceneUnloaded(AsyncOperationHandle<SceneInstance> asyncOperation)
        {
            m_AsyncOperationHandle = asyncOperation;
            m_Complete.Invoke(this);
        }

        public string SceneName => m_SceneName;
        public AsyncOperationHandle<SceneInstance> AsyncOperationHandle => m_AsyncOperationHandle;
    }
}
