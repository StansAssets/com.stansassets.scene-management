using System;

namespace StansAssets.SceneManagement
{
    public interface IApplicationStateStack<T> where T : Enum
    {
        T Pop();
        void Set(T state);
        void Push(T state);
    }
}
