using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement
{
    public interface IApplicationStateStack<T> : IReadOnlyApplicationStateStack<T> where T : Enum
    {
        void RegisterState(T key, IApplicationState<T> applicationState);
        
        void SetPreprocessAction(Action<StackOperationEvent<T>, Action> preprocessAction);
        void SetPostprocessAction(Action<StackOperationEvent<T>, Action> postprocessAction);

        IApplicationState<T> GetStateFromEnum(T key);
        T GetStateEnum(IApplicationState<T> key);
    }
}
