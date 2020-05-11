using System;

namespace StansAssets.SceneManagement
{
    public interface IApplicationState
    {
        void Activate(Action onComplete);
        void Pause(Action onComplete);
        void Deactivate(Action onComplete);
    }
}
