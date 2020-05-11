using System;

namespace StansAssets.SceneManagement
{
    public interface IScenePreloader
    {
        void FadeIn(Action onComplete);
        void FadeOut(Action onComplete);

        void OnProgress(float progress);
    }
}
