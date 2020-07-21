using StansAssets.Foundation.Patterns;
using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement
{
    public class StackChangeEvent<T> where T : Enum
    {
        static readonly DefaultPool<StackChangeEvent<T>> s_EventsPool = new DefaultPool<StackChangeEvent<T>>();

        public StackAction Action { get; private set; }
        public IReadOnlyList<T>  OldStackValue { get; private set; }
        public IReadOnlyList<T>  NewStackValue { get; private set; }

        public static StackChangeEvent<T> GetPooled(StackAction action, IReadOnlyList<T> oldStackValue, IReadOnlyList<T> newStackValue)
        {
            var e = s_EventsPool.Get();
            e.Action = action;
            e.OldStackValue = oldStackValue;
            e.NewStackValue = newStackValue;

            return e;
        }

        public static void Release(StackChangeEvent<T> stackChangeEvent)
        {
            s_EventsPool.Release(stackChangeEvent);
        }
    }
}
