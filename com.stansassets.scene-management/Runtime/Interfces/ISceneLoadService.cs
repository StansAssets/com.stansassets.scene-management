using System;

namespace StansAssets.SceneManagement
{
    public interface ISceneLoadService
    {
        void Load(IScenePreloader preloader, string sceneName, Action<ISceneManager> onComplete);
        void Load(string sceneName, Action<ISceneManager> onComplete);
        void Deactivate(string sceneName, Action<ISceneManager> onComplete);
        void Unload(string sceneName, Action onComplete);
    }
}
