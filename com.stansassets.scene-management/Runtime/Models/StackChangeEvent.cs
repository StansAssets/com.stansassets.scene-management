using System.Collections.Generic;
using StansAssets.Foundation.Patterns;

namespace StansAssets.SceneManagement
{
    public class StackChangeEvent
    {
        static readonly DefaultPool<StackChangeEvent> s_EventsPool = new DefaultPool<StackChangeEvent>();

        StackAction m_Action;
        IReadOnlyList<IApplicationState> m_OldStackValue;
        IReadOnlyList<IApplicationState> m_NewStackValue;

        public static StackChangeEvent GetPooled(StackAction action, IReadOnlyList<IApplicationState> oldStackValue, IReadOnlyList<IApplicationState> newStackValue)
        {
            var e = s_EventsPool.Get();
            e.m_Action = action;
            e.m_OldStackValue = oldStackValue;
            e.m_NewStackValue = newStackValue;

            return e;
        }

        public static void Release(StackChangeEvent stackChangeEvent)
        {
            s_EventsPool.Release(stackChangeEvent);
        }
    }
}
