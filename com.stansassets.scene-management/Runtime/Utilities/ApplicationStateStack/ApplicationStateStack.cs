using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement
{
    public class ApplicationStateStack<T> : IApplicationStateStack<T> where T : Enum
    {
        readonly Dictionary<T, IApplicationState> m_EnumToState = new Dictionary<T, IApplicationState>();
        readonly Dictionary<IApplicationState, T> m_StateToEnum = new Dictionary<IApplicationState, T>();
        readonly ApplicationStateStack m_StatesStack = new ApplicationStateStack();

        public void RegisterState(T key, IApplicationState value)
        {
            m_EnumToState.Add(key, value);
            m_StateToEnum.Add(value, key);
        }

        public T Pop() => m_StateToEnum[m_StatesStack.Pop()];

        public void Set(T state)
        {
            m_StatesStack.Set(m_EnumToState[state]);
        }

        public void Push(T state) => m_StatesStack.Push(m_EnumToState[state]);
    }

    public class ApplicationStateStack
    {
        readonly Stack<IApplicationState> m_StatesStack;

        public ApplicationStateStack()
        {
            m_StatesStack = new Stack<IApplicationState>();
        }

        public void Push(IApplicationState applicationState)
        {
            if(m_StatesStack.Count > 0 && m_StatesStack.Peek() == applicationState)
                return;

            foreach (var state in m_StatesStack)
                state.Pause();

            applicationState.Activate();
            m_StatesStack.Push(applicationState);
        }

        public IApplicationState Pop()
        {
            if (m_StatesStack.Count == 0)
                return null;

            var applicationState = m_StatesStack.Pop();
            applicationState.Deactivate();

            if(m_StatesStack.Count > 0)
                m_StatesStack.Peek().Activate();

            return applicationState;
        }

        public void Set(IApplicationState applicationState)
        {
            if(m_StatesStack.Count > 0 && m_StatesStack.Peek() == applicationState)
                return;

            foreach (var state in m_StatesStack)
                state.Deactivate();

            applicationState.Activate();
            m_StatesStack.Push(applicationState);
        }
    }
}
