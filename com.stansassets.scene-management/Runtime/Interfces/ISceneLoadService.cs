using System;
using UnityEngine.SceneManagement;

namespace StansAssets.SceneManagement
{
    public interface ISceneLoadService
    {
        void Load<T>(IScenePreloader preloader, string sceneName, Action<Scene, T> onComplete) where T : ISceneManager;
        void Load<T>(string sceneName, Action<Scene, T> onComplete) where T : ISceneManager;
        void Deactivate<T>(string sceneName, Action<T> onComplete) where T : ISceneManager;
        void Unload(string sceneName, Action onComplete);
    }
}
