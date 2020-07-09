using StansAssets.Foundation.Patterns;
using System.Collections.Generic;

namespace StansAssets.SceneManagement
{
    public class StackChangeEvent
    {
        static readonly DefaultPool<StackChangeEvent> s_EventsPool = new DefaultPool<StackChangeEvent>();

        public StackAction Action { get; private set; }
        public IReadOnlyList<IApplicationState>  OldStackValue { get; private set; }
        public IReadOnlyList<IApplicationState>  NewStackValue { get; private set; }

        public static StackChangeEvent GetPooled(StackAction action, IReadOnlyList<IApplicationState> oldStackValue, IReadOnlyList<IApplicationState> newStackValue)
        {
            var e = s_EventsPool.Get();
            e.Action = action;
            e.OldStackValue = oldStackValue;
            e.NewStackValue = newStackValue;

            return e;
        }

        public static void Release(StackChangeEvent stackChangeEvent)
        {
            s_EventsPool.Release(stackChangeEvent);
        }
    }
}
