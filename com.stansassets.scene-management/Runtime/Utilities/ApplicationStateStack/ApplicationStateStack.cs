using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StansAssets.Foundation.Patterns;
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

        public bool IsBusy => m_StatesStack.IsBusy;
    }

    public class ApplicationStateStack
    {
        readonly List<IApplicationState> m_StatesStack;

        List<IApplicationState> m_OldStackState;
        List<IApplicationState> m_NewStackState;

        public event Action OnApplicationStateChanged;
        public bool IsBusy { get; private set; }

        public ApplicationStateStack()
        {
            m_StatesStack = new List<IApplicationState>();
        }

        public void Push(IApplicationState applicationState) => Push(applicationState, () => { });

        public void Push(IApplicationState applicationState, [NotNull] Action onComplete)
        {
            Assert.IsFalse(IsBusy);
            Assert.IsNotNull(onComplete);
            if (m_StatesStack.Count > 0 && m_StatesStack[0] == applicationState)
            {
                onComplete.Invoke();
                return;
            }

            IsBusy = true;
            m_OldStackState = ListPool<IApplicationState>.Get();
            m_OldStackState.AddRange(m_StatesStack);

            m_NewStackState = ListPool<IApplicationState>.Get();
            m_NewStackState.AddRange(m_StatesStack);
            m_NewStackState.Add(applicationState);
            var pauseEvent = StackChangeEvent.GetPooled(StackAction.Paused, m_OldStackState, m_NewStackState);

            InvokeActionsInStack(pauseEvent, () =>
            {
                StackChangeEvent.Release(pauseEvent);
                var addEvent = StackChangeEvent.GetPooled(StackAction.Added, m_OldStackState, m_NewStackState);
                applicationState.ChangeState(addEvent,() =>
                {
                    m_StatesStack.Add(applicationState);

                    StackChangeEvent.Release(addEvent);
                    ListPool<IApplicationState>.Release(m_OldStackState);
                    ListPool<IApplicationState>.Release(m_NewStackState);

                    onComplete.Invoke();
                    OnApplicationStateChanged?.Invoke();

                    IsBusy = false;
                });
            });
        }

        public void Pop() => Pop(state => { });

        public void Pop([NotNull] Action<IApplicationState> onComplete)
        {
            Assert.IsFalse(IsBusy);
            Assert.IsNotNull(onComplete);
            if (m_StatesStack.Count == 0)
            {
                onComplete.Invoke(null);
                return;
            }

            IsBusy = true;
            m_OldStackState = ListPool<IApplicationState>.Get();
            m_OldStackState.AddRange(m_StatesStack);

            var applicationState = m_StatesStack.Last();
            m_NewStackState = ListPool<IApplicationState>.Get();
            m_NewStackState.AddRange(m_StatesStack);
            m_NewStackState.Add(applicationState);

            var removedEvent = StackChangeEvent.GetPooled(StackAction.Removed, m_OldStackState, m_NewStackState);
            applicationState.ChangeState(removedEvent,() =>
            {
                m_StatesStack.Remove(applicationState);
                StackChangeEvent.Release(removedEvent);
                if (m_StatesStack.Count > 0)
                {
                    var resumedEvent = StackChangeEvent.GetPooled(StackAction.Resumed, m_OldStackState, m_NewStackState);
                    m_StatesStack.Last().ChangeState(resumedEvent, () =>
                    {
                        StackChangeEvent.Release(resumedEvent);
                        ListPool<IApplicationState>.Release(m_OldStackState);
                        ListPool<IApplicationState>.Release(m_NewStackState);
                        onComplete.Invoke(applicationState);
                        OnApplicationStateChanged?.Invoke();

                        IsBusy = false;
                    });
                }
                else
                {
                    ListPool<IApplicationState>.Release(m_OldStackState);
                    ListPool<IApplicationState>.Release(m_NewStackState);
                    onComplete.Invoke(applicationState);
                    OnApplicationStateChanged?.Invoke();

                    IsBusy = false;
                }
            });
        }

        public void Set(IApplicationState applicationState) => Set(applicationState, () => { });

        public void Set(IApplicationState applicationState,  [NotNull] Action onComplete)
        {
            Assert.IsFalse(IsBusy);
            Assert.IsNotNull(onComplete);
            if (m_StatesStack.Count == 1 && m_StatesStack[0] == applicationState)
            {
                onComplete.Invoke();
                return;
            }

            IsBusy = true;
            m_OldStackState = ListPool<IApplicationState>.Get();
            m_OldStackState.AddRange(m_StatesStack);

            m_NewStackState = ListPool<IApplicationState>.Get();
            m_NewStackState.Add(applicationState);
            var removedEvent = StackChangeEvent.GetPooled(StackAction.Removed, m_OldStackState, m_NewStackState);

            InvokeActionsInStack(removedEvent, () =>
            {
                StackChangeEvent.Release(removedEvent);
                var addEvent = StackChangeEvent.GetPooled(StackAction.Added, m_OldStackState, m_NewStackState);
                applicationState.ChangeState(addEvent, () =>
                {
                    StackChangeEvent.Release(addEvent);
                    ListPool<IApplicationState>.Release(m_OldStackState);
                    ListPool<IApplicationState>.Release(m_NewStackState);

                    m_StatesStack.Clear();
                    m_StatesStack.Add(applicationState);
                    onComplete.Invoke();
                    OnApplicationStateChanged?.Invoke();

                    IsBusy = false;
                });
            });
        }

        public IEnumerable<IApplicationState> States => m_StatesStack;

        void InvokeActionsInStack(StackChangeEvent stackChangeEvent, Action onComplete, int index = 0)
        {
            if (index >= m_StatesStack.Count)
            {
                onComplete.Invoke();
                return;
            }

            var state = m_StatesStack[index];
            index++;

            state.ChangeState(stackChangeEvent, () =>
            {
                InvokeActionsInStack(stackChangeEvent, onComplete, index);
            });
        }
    }
}
