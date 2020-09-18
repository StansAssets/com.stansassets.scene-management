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

        Action<StackOperationEvent<T>, Action> m_PreprocessAction;
        Action<StackOperationEvent<T>, Action> m_PostprocessAction;

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

        public IApplicationState<T> GetStateFromEnum(T key)
        {
            return m_EnumToState[key];
        }

        public T GetStateEnum(IApplicationState<T> key)
        {
            return m_StateToEnum[key];
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
            var oldStackState = ListPool<T>.Get();
            oldStackState.AddRange(m_StatesStack);

            var newStackState = ListPool<T>.Get();
            newStackState.Add(applicationState);

            var stackSetOperationEvent = StackOperationEvent<T>.GetPooled(StackOperation.Set, applicationState, oldStackState, newStackState);
            Preprocess(stackSetOperationEvent, () =>
            {
                InvokeStateWillChange(stackSetOperationEvent);
                RunStackAction(StackAction.Removed, oldStackState, () =>
                {
                    //oldStackState will be released so we need to make new one
                    oldStackState = ListPool<T>.Get();
                    oldStackState.AddRange(m_StatesStack);

                    var addEvent = StackChangeEvent<T>.GetPooled(StackAction.Added, applicationState);
                    var changeStateRequest = new ProgressListenerRequest();
                    changeStateRequest.OnProgressChange += () => InvokeProgressChange(changeStateRequest.Progress, addEvent);
                    changeStateRequest.OnComplete += () =>
                    {
                        var stackSeComplete = StackOperationEvent<T>.GetPooled(StackOperation.Set, applicationState, oldStackState, newStackState);
                        Postprocess(stackSeComplete, () =>
                        {
                            IsBusy = false;
                            m_StatesStack.Clear();
                            m_StatesStack.Add(applicationState);

                            InvokeStateChanged(stackSeComplete);

                            ListPool<T>.Release(oldStackState);
                            ListPool<T>.Release(newStackState);
                            StackChangeEvent<T>.Release(addEvent);

                            onComplete.Invoke();
                        });
                    };

                    GetStateFromEnum(applicationState).ChangeState(addEvent, changeStateRequest);
                });
            });
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
            var oldStackState = ListPool<T>.Get();
            oldStackState.AddRange(m_StatesStack);

            var newStackState = ListPool<T>.Get();
            newStackState.AddRange(m_StatesStack);
            newStackState.Add(applicationState);

            var pausedSate = m_StatesStack.Last();

            var stateWillChangeEvent = StackOperationEvent<T>.GetPooled(StackOperation.Push, applicationState, oldStackState, newStackState);

            Preprocess(stateWillChangeEvent, () =>
            {
                InvokeStateWillChange(stateWillChangeEvent);
                var pauseEvent = StackChangeEvent<T>.GetPooled(StackAction.Paused, pausedSate);
                var addEvent = StackChangeEvent<T>.GetPooled(StackAction.Added, applicationState);

                var changeStateRequest = new ProgressListenerRequest();
                changeStateRequest.OnProgressChange += () => InvokeProgressChange(changeStateRequest.Progress, pauseEvent);
                changeStateRequest.OnComplete += () =>
                {
                    changeStateRequest = new ProgressListenerRequest();
                    changeStateRequest.OnProgressChange += () => InvokeProgressChange(changeStateRequest.Progress, addEvent);
                    changeStateRequest.OnComplete += () =>
                    {
                        var stackSeComplete = StackOperationEvent<T>.GetPooled(StackOperation.Push, applicationState, oldStackState, newStackState);
                        Postprocess(stackSeComplete, () =>
                        {
                            IsBusy = false;
                            m_StatesStack.Add(applicationState);
                            InvokeStateChanged(stackSeComplete);

                            ListPool<T>.Release(oldStackState);
                            ListPool<T>.Release(newStackState);
                            StackChangeEvent<T>.Release(addEvent);
                            StackChangeEvent<T>.Release(pauseEvent);

                            onComplete.Invoke();
                        });
                    };
                    GetStateFromEnum(applicationState).ChangeState(addEvent, changeStateRequest);
                };

                GetStateFromEnum(pausedSate).ChangeState(pauseEvent, changeStateRequest);
            });
        }

        public void Pop() => Pop(() => { });

        public void Pop([NotNull] Action onComplete)
        {
            Assert.IsFalse(IsBusy);
            Assert.IsNotNull(onComplete);
            if (m_StatesStack.Count == 0)
            {
                throw new Exception("States are empty");
            }

            IsBusy = true;
            var oldStackState = ListPool<T>.Get();
            oldStackState.AddRange(m_StatesStack);

            var removedSate = m_StatesStack.Last();
            var newStackState = ListPool<T>.Get();
            newStackState.AddRange(m_StatesStack);
            newStackState.Remove(removedSate);

            var resumedSate = newStackState.Last();

            var stateWillChangeEvent = StackOperationEvent<T>.GetPooled(StackOperation.Pop, removedSate, oldStackState, newStackState);
            Preprocess(stateWillChangeEvent, () =>
            {
                InvokeStateWillChange(stateWillChangeEvent);

                var removedEvent = StackChangeEvent<T>.GetPooled(StackAction.Removed, removedSate);
                var resumedEvent = StackChangeEvent<T>.GetPooled(StackAction.Resumed, resumedSate);

                var changeStateRequest = new ProgressListenerRequest();
                changeStateRequest.OnProgressChange += () => InvokeProgressChange(changeStateRequest.Progress, removedEvent);
                changeStateRequest.OnComplete += () =>
                {
                    changeStateRequest = new ProgressListenerRequest();
                    changeStateRequest.OnProgressChange += () => InvokeProgressChange(changeStateRequest.Progress, resumedEvent);
                    changeStateRequest.OnComplete += () =>
                    {
                        var stackPopComplete = StackOperationEvent<T>.GetPooled(StackOperation.Pop, removedSate, oldStackState, newStackState);
                        Postprocess(stackPopComplete, () =>
                        {
                            IsBusy = false;
                            m_StatesStack.Remove(removedSate);
                            InvokeStateChanged(stackPopComplete);

                            ListPool<T>.Release(oldStackState);
                            ListPool<T>.Release(newStackState);
                            StackChangeEvent<T>.Release(removedEvent);
                            StackChangeEvent<T>.Release(resumedEvent);

                            onComplete.Invoke();
                        });
                    };

                    GetStateFromEnum(resumedSate).ChangeState(resumedEvent, changeStateRequest);
                };

                GetStateFromEnum(removedSate).ChangeState(removedEvent, changeStateRequest);
            });
        }

        public bool IsCurrent(T applicationState)
        {
            return States.Any() && States.Last().Equals(applicationState);
        }

        public void SetPreprocessAction(Action<StackOperationEvent<T>, Action> preprocessAction)
        {
            m_PreprocessAction = preprocessAction;
        }

        public void SetPostprocessAction(Action<StackOperationEvent<T>, Action> postprocessAction)
        {
            m_PostprocessAction = postprocessAction;
        }

        void Preprocess(StackOperationEvent<T> stackOperationEvent, Action onComplete)
        {
            if (m_PreprocessAction != null)
                m_PreprocessAction.Invoke(stackOperationEvent, onComplete);
            else
                onComplete.Invoke();
        }

        void Postprocess(StackOperationEvent<T> stackOperationEvent, Action onComplete)
        {
            if (m_PostprocessAction != null)
                m_PostprocessAction.Invoke(stackOperationEvent, onComplete);
            else
                onComplete.Invoke();
        }

        void RunStackAction(StackAction action, List<T> stack, Action onComplete)
        {
            if (stack.Count == 0)
            {
                ListPool<T>.Release(stack);
                onComplete.Invoke();
                return;
            }

            var applicationState = stack.Last();
            stack.RemoveAt(stack.Count - 1);

            var changeEvent = StackChangeEvent<T>.GetPooled(action, applicationState);

            var changeStateRequest = new ProgressListenerRequest();
            changeStateRequest.OnProgressChange += () => InvokeProgressChange(changeStateRequest.Progress, changeEvent);
            changeStateRequest.OnComplete += () =>
            {
                RunStackAction(action, stack, onComplete);
                StackChangeEvent<T>.Release(changeEvent);
            };

            GetStateFromEnum(applicationState).ChangeState(changeEvent, changeStateRequest);
        }

        void InvokeStateWillChange(StackOperationEvent<T> eventArg)
        {
            foreach (var subscription in m_Subscriptions)
                subscription.OnApplicationStateWillChanged(eventArg);

            StackOperationEvent<T>.Release(eventArg);
        }

        void InvokeProgressChange(float p, StackChangeEvent<T> eventArg)
        {
            foreach (var subscription in m_Subscriptions)
                subscription.ApplicationStateChangeProgressChanged(p, eventArg);
        }

        void InvokeStateChanged(StackOperationEvent<T> eventArg)
        {
            foreach (var subscription in m_Subscriptions)
                subscription.ApplicationStateChanged(eventArg);

            StackOperationEvent<T>.Release(eventArg);
        }
    }
}
