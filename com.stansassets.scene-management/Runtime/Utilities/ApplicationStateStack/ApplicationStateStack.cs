using JetBrains.Annotations;
using StansAssets.Foundation.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace StansAssets.SceneManagement
{
    public class ApplicationStateStack<T> : IApplicationStateStack<T> where T : Enum
    {
        readonly Dictionary<T, IApplicationState> m_EnumToState = new Dictionary<T, IApplicationState>();
        readonly Dictionary<IApplicationState, T> m_StateToEnum = new Dictionary<IApplicationState, T>();
        readonly ApplicationStateStack m_StatesStack = new ApplicationStateStack();

        public ApplicationStateStack() { }

        public void AddDelegate(IApplicationStateStackChanged d)
        {
            m_StatesStack.AddDelegate(d);
        }

        public void RemoveDelegate(IApplicationStateStackChanged d)
        {
            m_StatesStack.RemoveDelegate(d);
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
        public void Set(T applicationState, [NotNull] Action onComplete) => m_StatesStack.Set(m_EnumToState[applicationState], onComplete);

        public bool IsCurrent(T applicationState)
        {
            return States.Any() && States.Last().Equals(applicationState);
        }

        public IEnumerable<T> States
        {
            get => m_StatesStack.States.Select(applicationState => m_StateToEnum[applicationState]);
        }

        public bool IsBusy => m_StatesStack.IsBusy;
    }

    public class ApplicationStateStack
    {
        public IEnumerable<IApplicationState> States => m_StatesStack;
        readonly List<IApplicationState> m_StatesStack;

        public IEnumerable<IApplicationStateStackChanged> Subscriptions;
        readonly List<IApplicationStateStackChanged> m_Subscriptions;

        List<IApplicationState> m_OldStackState;
        List<IApplicationState> m_NewStackState;

        public bool IsBusy { get; private set; }

        public ApplicationStateStack()
        {
            m_StatesStack = new List<IApplicationState>();
            m_Subscriptions = new List<IApplicationStateStackChanged>();
        }

        public void AddDelegate(IApplicationStateStackChanged d)
        {
            m_Subscriptions.Add(d);
        }

        public void RemoveDelegate(IApplicationStateStackChanged d)
        {
            for (int i = m_Subscriptions.Count - 1; i >= 0; i--)
            {
                if (m_Subscriptions[i] != d)
                    continue;
                m_Subscriptions.RemoveAt(i);
                break;
            }
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
            var addEvent = StackChangeEvent.GetPooled(StackAction.Added, m_OldStackState, m_NewStackState);

            var groupReq = new GroupRequest(2);
            groupReq.Done += onComplete.Invoke;

            var pauseReq = new GroupRequest(m_OldStackState.Count);
            var addReq = new Request();

            pauseReq.ProgressChange += p => InvokeProgressChange(groupReq.Progress, pauseEvent);
            pauseReq.Done += () =>
            {
                StackChangeEvent.Release(pauseEvent);
                InvokeStateChanged(pauseEvent);

                InvokeStateWillChange(addEvent);
                applicationState.ChangeState(addEvent, addReq);
            };

            addReq.ProgressChange += p => InvokeProgressChange(groupReq.Progress, addEvent);
            addReq.Done += () =>
            {
                m_StatesStack.Add(applicationState);

                ListPool<IApplicationState>.Release(m_OldStackState);
                ListPool<IApplicationState>.Release(m_NewStackState);

                InvokeStateChanged(addEvent);
                StackChangeEvent.Release(addEvent);

                IsBusy = false;
            };

            groupReq.AddRequest(pauseReq);
            groupReq.AddRequest(addReq);

            InvokeStateWillChange(pauseEvent);
            InvokeChangeActionInStack(pauseEvent, pauseReq);
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
            m_NewStackState.Remove(applicationState);

            var removedEvent = StackChangeEvent.GetPooled(StackAction.Removed, m_OldStackState, m_NewStackState);
            var resumedEvent = StackChangeEvent.GetPooled(StackAction.Resumed, m_OldStackState, m_NewStackState);

            var group = new GroupRequest(2);
            group.Done += () => onComplete.Invoke(applicationState);

            var removeReq = new Request();
            var resumeReq = new GroupRequest(m_NewStackState.Count);

            removeReq.ProgressChange += p => InvokeProgressChange(group.Progress, removedEvent);
            removeReq.Done += () =>
            {
                m_StatesStack.Remove(applicationState);

                InvokeStateChanged(removedEvent);
                StackChangeEvent.Release(removedEvent);

                InvokeStateWillChange(resumedEvent);
                if (m_StatesStack.Count > 0)
                    m_StatesStack.Last().ChangeState(resumedEvent, resumeReq);
                else
                    resumeReq.SetDone();
            };

            resumeReq.ProgressChange += p => InvokeProgressChange(group.Progress, resumedEvent);
            resumeReq.Done += () =>
            {
                ListPool<IApplicationState>.Release(m_OldStackState);
                ListPool<IApplicationState>.Release(m_NewStackState);

                InvokeStateChanged(resumedEvent);

                IsBusy = false;
            };

            group.AddRequest(removeReq);
            group.AddRequest(resumeReq);

            InvokeStateWillChange(removedEvent);
            applicationState.ChangeState(removedEvent, removeReq);
        }

        public void Set(IApplicationState applicationState) => Set(applicationState, () => { });

        public void Set(IApplicationState applicationState, [NotNull] Action onComplete)
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
            var addEvent = StackChangeEvent.GetPooled(StackAction.Added, m_OldStackState, m_NewStackState);

            var groupReq = new GroupRequest(2);
            groupReq.Done += onComplete.Invoke;

            var removeReq = new GroupRequest(m_OldStackState.Count);
            var addReq = new GroupRequest(m_NewStackState.Count);

            removeReq.ProgressChange += p => InvokeProgressChange(groupReq.Progress, removedEvent);
            removeReq.Done += () =>
            {
                InvokeStateChanged(removedEvent);
                StackChangeEvent.Release(removedEvent);

                InvokeStateWillChange(addEvent);
                applicationState.ChangeState(addEvent, addReq);
            };

            addReq.ProgressChange += p => InvokeProgressChange(groupReq.Progress, addEvent);
            addReq.Done += () =>
            {

                ListPool<IApplicationState>.Release(m_OldStackState);
                ListPool<IApplicationState>.Release(m_NewStackState);

                m_StatesStack.Clear();
                m_StatesStack.Add(applicationState);

                InvokeStateChanged(addEvent);
                StackChangeEvent.Release(addEvent);

                IsBusy = false;
            };

            groupReq.AddRequest(removeReq);
            groupReq.AddRequest(addReq);

            InvokeStateWillChange(removedEvent);
            InvokeChangeActionInStack(removedEvent, removeReq);
        }

        void InvokeChangeActionInStack(StackChangeEvent stackChangeEvent, GroupRequest groupReq, int index = 0)
        {
            if (index >= m_StatesStack.Count)
            {
                groupReq.SetDone();
                return;
            }

            var state = m_StatesStack[index];
            index++;

            var request = new Request();
            request.Done += () => InvokeChangeActionInStack(stackChangeEvent, groupReq, index);
            groupReq.AddRequest(request);

            state.ChangeState(stackChangeEvent, request);
        }

        void InvokeStateWillChange(StackChangeEvent eventArg)
        {
            foreach (var subscription in m_Subscriptions)
            {
                subscription.OnApplicationStateWillChanged(eventArg);
            }
        }

        void InvokeProgressChange(float p, StackChangeEvent eventArg)
        {
            foreach (var subscription in m_Subscriptions)
            {
                subscription.ApplicationStateChangeProgressChanged(p, eventArg);
            }
        }

        void InvokeStateChanged(StackChangeEvent eventArg)
        {
            foreach (var subscription in m_Subscriptions)
            {
                subscription.ApplicationStateChanged(eventArg);
            }
        }
    }
}
