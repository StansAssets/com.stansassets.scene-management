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
    }

    public class ApplicationStateStack
    {
        enum StackCommand
        {
            Pause,
            Deactivate,
        }

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

            InvokeActionsInStack(StackCommand.Pause, () =>
            {
                m_StatesStack.Add(applicationState);
                applicationState.Activate(() =>
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
            applicationState.Deactivate(() =>
            {
                m_StatesStack.Remove(applicationState);
                if (m_StatesStack.Count > 0)
                {
                    m_StatesStack.Last().Activate(() =>
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

            InvokeActionsInStack(StackCommand.Deactivate, () =>
            {
                m_StatesStack.Clear();
                m_StatesStack.Add(applicationState);
                applicationState.Activate(onComplete);
                 OnApplicationStateChanged?.Invoke();
            });
        }

        void InvokeActionsInStack(StackCommand command, Action onComplete, int index = 0)
        {
            if (index >= m_StatesStack.Count)
            {
                onComplete.Invoke();
                return;
            }

            var state = m_StatesStack[index];
            index++;
            switch (command)
            {
                case  StackCommand.Pause:
                    state.Pause(() =>
                    {
                        InvokeActionsInStack(command, onComplete, index);
                    });
                    break;
                case  StackCommand.Deactivate:
                    state.Deactivate(() =>
                    {
                        InvokeActionsInStack(command, onComplete, index);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, $"The {command} is not supported");
            }
        }
    }
}
