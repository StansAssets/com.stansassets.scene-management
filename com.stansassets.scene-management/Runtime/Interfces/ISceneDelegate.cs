using System;

namespace StansAssets.SceneManagement
{
    public interface ISceneDelegate
    {
        void ActivateScene(Action onComplete);
        void DeactivateScene(Action onComplete);
    }
}
