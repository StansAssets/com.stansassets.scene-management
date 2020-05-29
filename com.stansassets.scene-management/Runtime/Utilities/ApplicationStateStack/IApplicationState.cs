using System;

namespace StansAssets.SceneManagement
{
    public interface IApplicationState
    {
        void ChangeState(StackChangeEvent evt, Action onComplete);
    }
}
