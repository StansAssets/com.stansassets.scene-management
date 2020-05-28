using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace StansAssets.SceneManagement
{
    public class ApplicationStateStack<T> : IApplicationStateStack<T> where T : Enum
    {
        readonly Dictionary<T, IApplicationState> m_EnumToState = new Dictionary<T, IApplicationState>();
        readonly Dictionary<IApplicationState, T> m_StateToEnum = new Dictionary<IApplicationState, T>();
        readonly ApplicationStateStack m_StatesStack = new ApplicationStateStack();

        public event Action OnApplicationStateChanged;

        public ApplicationStateStack()
        {
            m_StatesStack.OnApplicationStateChanged += () =>
            {
                OnApplicationStateChanged?.Invoke();
            };
        }

        public void RegisterState(T key, IApplicationState value)
        {
            m_EnumToState.Add(key, value);
            m_StateToEnum.Add(value, key);
        }

        public void Push(T applicationState) => Push(applicationState, () => { });
        public void Push(T applicationState, [NotNull] Action onComplete) => m_StatesStack.Push(m_EnumToState[applicationState], onComplete);

        public void Pop() => Pop(applicationState => { });
        public void Pop([NotNull] Action<T> onComplete) => m_StatesStack.Pop(applicationState =>
        {
            onComplete.Invoke(m_StateToEnum[applicationState]);
        });


        public void Set(T applicationState) => Set(applicationState, () => { });
        public void Set(T applicationState,  [NotNull] Action onComplete) =>  m_StatesStack.Set(m_EnumToState[applicationState], onComplete);

        public bool IsCurrent(T applicationState)
        {
            return States.Any() && States.Last().Equals(applicationState);
        }

        public IEnumerable<T> States
        {
            get { return m_StatesStack.States.Select(applicationState => m_StateToEnum[applicationState]); }
        }
    }

    public class ApplicationStateStack
    {
        public event Action OnApplicationStateChanged;

        readonly IList<IApplicationState> m_StatesStack;

        public ApplicationStateStack()
        {
            m_StatesStack = new List<IApplicationState>();
        }

        public void Push(IApplicationState applicationState) => Push(applicationState, () => { });

        public void Push(IApplicationState applicationState, [NotNull] Action onComplete)
        {
            Assert.IsNotNull(onComplete);
            if (m_StatesStack.Count > 0 && m_StatesStack[0] == applicationState)
            {
                onComplete.Invoke();
                return;
            }

            InvokeActionsInStack(StackAction.Paused, () =>
            {
                m_StatesStack.Add(applicationState);
                applicationState.ChangeState(StackAction.Added,() =>
                {
                    onComplete.Invoke();
                    OnApplicationStateChanged?.Invoke();
                });
            });
        }

        public void Pop() => Pop(state => { });

        public void Pop([NotNull] Action<IApplicationState> onComplete)
        {
            Assert.IsNotNull(onComplete);
            if (m_StatesStack.Count == 0)
            {
                onComplete.Invoke(null);
                return;
            }

            var applicationState = m_StatesStack.Last();
            applicationState.ChangeState(StackAction.Removed,() =>
            {
                m_StatesStack.Remove(applicationState);
                if (m_StatesStack.Count > 0)
                {
                    m_StatesStack.Last().ChangeState(StackAction.Resumed, () =>
                    {
                        onComplete.Invoke(applicationState);
                        OnApplicationStateChanged?.Invoke();
                    });
                }
                else
                {
                    onComplete.Invoke(applicationState);
                    OnApplicationStateChanged?.Invoke();
                }
            });
        }

        public void Set(IApplicationState applicationState) => Set(applicationState, () => { });

        public void Set(IApplicationState applicationState,  [NotNull] Action onComplete)
        {
            if (m_StatesStack.Count == 1 && m_StatesStack[0] == applicationState)
            {
                onComplete.Invoke();
                return;
            }

            InvokeActionsInStack(StackAction.Removed, () =>
            {
                m_StatesStack.Clear();
                m_StatesStack.Add(applicationState);
                applicationState.ChangeState(StackAction.Added, onComplete);
                OnApplicationStateChanged?.Invoke();
            });
        }

        public IEnumerable<IApplicationState> States => m_StatesStack;

        void InvokeActionsInStack(StackAction stackAction, Action onComplete, int index = 0)
        {
            if (index >= m_StatesStack.Count)
            {
                onComplete.Invoke();
                return;
            }

            var state = m_StatesStack[index];
            index++;

            state.ChangeState(stackAction, () =>
            {
                InvokeActionsInStack(stackAction, onComplete, index);
            });
        }
    }
}
