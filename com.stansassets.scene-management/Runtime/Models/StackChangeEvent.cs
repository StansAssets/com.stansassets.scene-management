using StansAssets.Foundation.Patterns;
using System;
using System.Collections.Generic;

namespace StansAssets.SceneManagement
{
    public class StackChangeEvent<TEnum> : StackEvent<TEnum> where TEnum : Enum
    {
        static readonly DefaultPool<StackChangeEvent<TEnum>> s_EventsPool = new DefaultPool<StackChangeEvent<TEnum>>();
        
        public StackAction Action { get; private set; }
        
        public static StackChangeEvent<TEnum> GetPooled(StackAction action, TEnum state)
        {
            var e = s_EventsPool.Get();
            e.Action = action;
            e.State = state;

            return e;
        }

        public static void Release(StackChangeEvent<TEnum> stackChangeEvent)
        {
            s_EventsPool.Release(stackChangeEvent);
        }
    }
}
