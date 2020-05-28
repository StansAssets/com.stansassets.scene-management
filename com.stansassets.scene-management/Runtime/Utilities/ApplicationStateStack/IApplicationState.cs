using System;

namespace StansAssets.SceneManagement
{
    public interface IApplicationState
    {
        void ChangeState(StackAction stackAction, Action onComplete);
    }
}
