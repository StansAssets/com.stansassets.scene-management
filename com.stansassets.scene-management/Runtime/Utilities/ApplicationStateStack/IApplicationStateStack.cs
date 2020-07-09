using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement
{
    public interface IApplicationStateStack<T> where T : Enum
    {
        void AddDelegate(IApplicationStateStackChanged d);
        void RemoveDelegate(IApplicationStateStackChanged d);

        void Pop();
        void Pop(Action<T> onComplete);

        void Set(T state);
        void Set(T state, Action onComplete);

        void Push(T state);
        void Push(T state, Action onComplete);

        IEnumerable<T> States { get; }
        bool IsBusy { get; }
    }
}
