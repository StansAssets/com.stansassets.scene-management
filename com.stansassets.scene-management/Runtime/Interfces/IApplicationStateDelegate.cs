using System;

namespace StansAssets.SceneManagement
{
    public interface IApplicationStateDelegate<T> where T : Enum
    {
        void OnApplicationStateWillChanged(StackChangeEvent<T> eventArg);
        void ApplicationStateChangeProgressChanged(float progress, StackChangeEvent<T> eventArg);
        void ApplicationStateChanged(StackChangeEvent<T> eventArg);
    }
}
