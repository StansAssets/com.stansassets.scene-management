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
        readonly Dictionary<T, IApplicationState<T>> m_EnumToState = new Dictionary<T, IApplicationState<T>>();
        readonly Dictionary<IApplicationState<T>, T> m_StateToEnum = new Dictionary<IApplicationState<T>, T>();

        readonly List<T> m_StatesStack;
        readonly List<IApplicationStateDelegate<T>> m_Subscriptions;

        List<T> m_OldStackState;
        List<T> m_NewStackState;

        public ApplicationStateStack()
        {
            m_StatesStack = new List<T>();
            m_Subscriptions = new List<IApplicationStateDelegate<T>>();
        }

        public bool IsBusy { get; private set; }

        public IEnumerable<T> States => m_StatesStack;

        public void AddDelegate(IApplicationStateDelegate<T> d)
        {
            m_Subscriptions.Add(d);
        }

        public void RemoveDelegate(IApplicationStateDelegate<T> d)
        {
            for (int i = m_Subscriptions.Count - 1; i >= 0; i--)
            {
                if (m_Subscriptions[i] != d)
                    continue;
                m_Subscriptions.RemoveAt(i);
                break;
            }
        }

        public void RegisterState(T key, IApplicationState<T> value)
        {
            m_EnumToState.Add(key, value);
            m_StateToEnum.Add(value, key);
        }

        public void Push(T applicationState) => Push(applicationState, () => { });
        public void Push(T applicationState, [NotNull] Action onComplete)
        {
            Assert.IsFalse(IsBusy);
            Assert.IsNotNull(onComplete);
            if (m_StatesStack.Count > 0 && m_StatesStack[0].Equals(applicationState))
            {
                onComplete.Invoke();
                return;
            }

            IsBusy = true;
            m_OldStackState = ListPool<T>.Get();
            m_OldStackState.AddRange(m_StatesStack);

            m_NewStackState = ListPool<T>.Get();
            m_NewStackState.AddRange(m_StatesStack);
            m_NewStackState.Add(applicationState);

            var pauseEvent = StackChangeEvent<T>.GetPooled(StackAction.Paused, m_OldStackState, m_NewStackState);
            var addEvent = StackChangeEvent<T>.GetPooled(StackAction.Added, m_OldStackState, m_NewStackState);

            var groupReq = new GroupRequest(2);
            groupReq.Done += onComplete.Invoke;

            var pauseReq = new GroupRequest(m_OldStackState.Count);
            var addReq = new Request();

            pauseReq.ProgressChange += p => InvokeProgressChange(groupReq.Progress, pauseEvent);
            pauseReq.Done += () =>
            {
                StackChangeEvent<T>.Release(pauseEvent);
                InvokeStateChanged(pauseEvent);

                InvokeStateWillChange(addEvent);
                m_EnumToState[applicationState].ChangeState(addEvent, addReq);
            };

            addReq.ProgressChange += p => InvokeProgressChange(groupReq.Progress, addEvent);
            addReq.Done += () =>
            {
                m_StatesStack.Add(applicationState);

                ListPool<T>.Release(m_OldStackState);
                ListPool<T>.Release(m_NewStackState);

                InvokeStateChanged(addEvent);
                StackChangeEvent<T>.Release(addEvent);

                IsBusy = false;
            };

            groupReq.AddRequest(pauseReq);
            groupReq.AddRequest(addReq);

            InvokeStateWillChange(pauseEvent);
            InvokeChangeActionInStack(pauseEvent, pauseReq);
        }

        public void Pop() => Pop(applicationState => { });
        public void Pop([NotNull] Action<T> onComplete)
        {
            Assert.IsFalse(IsBusy);
            Assert.IsNotNull(onComplete);
            if (m_StatesStack.Count == 0)
            {
                throw new Exception("States are empty");
            }

            IsBusy = true;
            m_OldStackState = ListPool<T>.Get();
            m_OldStackState.AddRange(m_StatesStack);

            var applicationState = m_StatesStack.Last();
            m_NewStackState = ListPool<T>.Get();
            m_NewStackState.AddRange(m_StatesStack);
            m_NewStackState.Remove(applicationState);

            var removedEvent = StackChangeEvent<T>.GetPooled(StackAction.Removed, m_OldStackState, m_NewStackState);
            var resumedEvent = StackChangeEvent<T>.GetPooled(StackAction.Resumed, m_OldStackState, m_NewStackState);

            var group = new GroupRequest(2);
            group.Done += () => onComplete.Invoke(applicationState);

            var removeReq = new Request();
            var resumeReq = new GroupRequest(m_NewStackState.Count);

            removeReq.ProgressChange += p => InvokeProgressChange(group.Progress, removedEvent);
            removeReq.Done += () =>
            {
                m_StatesStack.Remove(applicationState);

                InvokeStateChanged(removedEvent);
                StackChangeEvent<T>.Release(removedEvent);

                InvokeStateWillChange(resumedEvent);
                if (m_StatesStack.Count > 0)
                    m_EnumToState[m_StatesStack.Last()].ChangeState(resumedEvent, resumeReq);
                else
                    resumeReq.SetDone();
            };

            resumeReq.ProgressChange += p => InvokeProgressChange(group.Progress, resumedEvent);
            resumeReq.Done += () =>
            {
                ListPool<T>.Release(m_OldStackState);
                ListPool<T>.Release(m_NewStackState);

                InvokeStateChanged(resumedEvent);

                IsBusy = false;
            };

            group.AddRequest(removeReq);
            group.AddRequest(resumeReq);

            InvokeStateWillChange(removedEvent);
            m_EnumToState[applicationState].ChangeState(removedEvent, removeReq);
        }

        public void Set(T applicationState) => Set(applicationState, () => { });
        public void Set(T applicationState, [NotNull] Action onComplete)
        {
            Assert.IsFalse(IsBusy);
            Assert.IsNotNull(onComplete);
            if (m_StatesStack.Count == 1 && m_StatesStack[0].Equals(applicationState))
            {
                onComplete.Invoke();
                return;
            }

            IsBusy = true;
            m_OldStackState = ListPool<T>.Get();
            m_OldStackState.AddRange(m_StatesStack);

            m_NewStackState = ListPool<T>.Get();
            m_NewStackState.Add(applicationState);

            var removedEvent = StackChangeEvent<T>.GetPooled(StackAction.Removed, m_OldStackState, m_NewStackState);
            var addEvent = StackChangeEvent<T>.GetPooled(StackAction.Added, m_OldStackState, m_NewStackState);

            var groupReq = new GroupRequest(2);
            groupReq.Done += onComplete.Invoke;

            var removeReq = new GroupRequest(m_OldStackState.Count);
            var addReq = new GroupRequest(m_NewStackState.Count);

            removeReq.ProgressChange += p => InvokeProgressChange(groupReq.Progress, removedEvent);
            removeReq.Done += () =>
            {
                InvokeStateChanged(removedEvent);
                StackChangeEvent<T>.Release(removedEvent);

                InvokeStateWillChange(addEvent);
                m_EnumToState[applicationState].ChangeState(addEvent, addReq);
            };

            addReq.ProgressChange += p => InvokeProgressChange(groupReq.Progress, addEvent);
            addReq.Done += () =>
            {

                ListPool<T>.Release(m_OldStackState);
                ListPool<T>.Release(m_NewStackState);

                m_StatesStack.Clear();
                m_StatesStack.Add(applicationState);

                InvokeStateChanged(addEvent);
                StackChangeEvent<T>.Release(addEvent);

                IsBusy = false;
            };

            groupReq.AddRequest(removeReq);
            groupReq.AddRequest(addReq);

            InvokeStateWillChange(removedEvent);
            InvokeChangeActionInStack(removedEvent, removeReq);
        }

        public bool IsCurrent(T applicationState)
        {
            return States.Any() && States.Last().Equals(applicationState);
        }

        void InvokeChangeActionInStack(StackChangeEvent<T> stackChangeEvent, GroupRequest groupReq, int index = 0)
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

            m_EnumToState[state].ChangeState(stackChangeEvent, request);
        }

        void InvokeStateWillChange(StackChangeEvent<T> eventArg)
        {
            foreach (var subscription in m_Subscriptions)
                subscription.OnApplicationStateWillChanged(eventArg);
        }

        void InvokeProgressChange(float p, StackChangeEvent<T> eventArg)
        {
            foreach (var subscription in m_Subscriptions)
                subscription.ApplicationStateChangeProgressChanged(p, eventArg);
        }

        void InvokeStateChanged(StackChangeEvent<T> eventArg)
        {
            foreach (var subscription in m_Subscriptions)
                subscription.ApplicationStateChanged(eventArg);
        }
    }
}
