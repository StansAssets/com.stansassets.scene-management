using System;

namespace StansAssets.SceneManagement
{
    public interface IApplicationState<T> where T : Enum
    {
        void ChangeState(StackChangeEvent<T> evt, IProgressReporter reporter);
    }
}
