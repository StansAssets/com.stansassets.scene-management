using System;

namespace StansAssets.SceneManagement
{
    public interface IApplicationStateDelegate<T> where T : Enum
    {
        void ApplicationStateWillChange(StackOperationEvent<T> e);
        void ApplicationStateChangeProgressUpdated(float progress, StackChangeEvent<T> e);
        void ApplicationStateChanged(StackOperationEvent<T> e);
    }
}
