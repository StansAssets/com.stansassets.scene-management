using NUnit.Framework;
using StansAssets.SceneManagement;
using System;
using System.Linq;

namespace Tests
{
    public enum TestEnum { V1, V2, V3, V4 }

    public class TestApplicationStateV1 : IApplicationState<TestEnum>
    {
        public void ChangeState(StackChangeEvent<TestEnum> evt, IProgressReporter reporter)
        {
            reporter.UpdateProgress(.3f);
            reporter.UpdateProgress(.6f);
            reporter.UpdateProgress(.9f);
            reporter.SetDone();
        }
    }

    public class TestApplicationStateV2 : IApplicationState<TestEnum>
    {
        public void ChangeState(StackChangeEvent<TestEnum> evt, IProgressReporter reporter)
        {
            reporter.SetDone();
        }
    }

    public class ApplicationStateStackTest
    {
        [Test]
        public void TestSet()
        {
            var stack = new ApplicationStateStack<TestEnum>();
            var state = new TestApplicationStateV1();

            stack.RegisterState(TestEnum.V1, state);
            stack.Set(TestEnum.V1);

            Assert.IsFalse(stack.IsBusy);
            Assert.AreEqual(stack.States.Count(), 1);
            Assert.IsTrue(stack.IsCurrent(TestEnum.V1));
        }

        [Test]
        public void TestPush()
        {
            var stack = new ApplicationStateStack<TestEnum>();
            var state = new TestApplicationStateV1();

            stack.RegisterState(TestEnum.V1, state);
            stack.Push(TestEnum.V1);

            Assert.IsFalse(stack.IsBusy);
            Assert.AreEqual(stack.States.Count(), 1);
            Assert.IsTrue(stack.IsCurrent(TestEnum.V1));
        }

        [Test]
        public void TestPushWithMuchStates()
        {
            var stack = new ApplicationStateStack<TestEnum>();
            var state1 = new TestApplicationStateV1();
            var state2 = new TestApplicationStateV2();

            stack.RegisterState(TestEnum.V1, state1);
            stack.RegisterState(TestEnum.V2, state2);
            stack.Set(TestEnum.V1);
            stack.Push(TestEnum.V2); 

            Assert.IsFalse(stack.IsBusy);
            Assert.AreEqual(stack.States.Count(), 2);
            Assert.IsTrue(stack.IsCurrent(TestEnum.V2));
        }

        [Test]
        public void TestPopWithError()
        {
            Assert.Throws<Exception>(() =>
            {
                var stack = new ApplicationStateStack<TestEnum>();

                stack.Pop();
            });
        }

        [Test]
        public void TestPopWithoutError()
        {
            Assert.DoesNotThrow(() =>
            {
                var stack = new ApplicationStateStack<TestEnum>();
                stack.RegisterState(TestEnum.V1, new TestApplicationStateV1());
                stack.Set(TestEnum.V1);

                stack.Pop();

                Assert.IsFalse(stack.IsBusy);
                Assert.AreEqual(stack.States.Count(),0);
                Assert.IsFalse(stack.IsCurrent(TestEnum.V1));
            });
        }
    }
}
