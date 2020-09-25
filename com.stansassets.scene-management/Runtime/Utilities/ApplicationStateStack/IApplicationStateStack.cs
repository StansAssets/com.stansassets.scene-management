using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement
{
    public interface IApplicationStateStack<T> where T : Enum
    {
        void AddDelegate(IApplicationStateDelegate<T> @delegate);
        void RemoveDelegate(IApplicationStateDelegate<T> @delegate);

        void RegisterState(T key, IApplicationState<T> applicationState);

        bool IsCurrent(T applicationState);

        void SetPreprocessAction(Action<StackOperationEvent<T>, Action> preprocessAction);
        void SetPostprocessAction(Action<StackOperationEvent<T>, Action> postprocessAction);

        IApplicationState<T> GetStateFromEnum(T key);
        T GetStateEnum(IApplicationState<T> key);

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
