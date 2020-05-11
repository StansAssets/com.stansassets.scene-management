using System;

namespace StansAssets.SceneManagement
{
    public interface ISceneDelegate
    {
        void OnSceneLoaded();
        void OnSceneUnload();

        void ActivateScene(Action onComplete);
        void DeactivateScene(Action onComplete);
    }
}
