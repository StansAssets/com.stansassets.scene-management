using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement
{
    public interface IReadOnlyApplicationStateStack<T> where T : Enum
    {
        bool IsCurrent(T applicationState);
        
        void AddDelegate(IApplicationStateDelegate<T> @delegate);
        void RemoveDelegate(IApplicationStateDelegate<T> @delegate);
        
        void Pop();
        void Pop(Action onComplete);

        void Set(T state);
        void Set(T state, Action onComplete);

        void Push(T state);
        void Push(T state, Action onComplete);

        IEnumerable<T> States { get; }
        bool IsBusy { get; }
    }
}
