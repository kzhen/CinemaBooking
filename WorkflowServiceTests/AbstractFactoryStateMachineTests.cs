using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Automatonymous;

namespace WorkflowServiceTests
{
	public class CommonInstance
	{
		public State CurrentState { get; set; }
		public bool WasSet { get; set; }
	}

	public abstract class CommonStateMachine<T> : AutomatonymousStateMachine<T>
		where T : CommonInstance
	{
		public CommonStateMachine()
		{
			InstanceState(i => i.CurrentState);

			Event(() => ParentEvent);

		}

		public static TChild Create<TChild>(Func<TChild> childGetter)
			where TChild : CommonStateMachine<T>
		{
			var child = childGetter();
			child.Initialize();
			return child;
		}

		protected virtual void Initialize()
		{
			DuringAny(When(ParentEvent).Then(wf => DoParentEvent(wf)));
		}

		protected virtual void DoParentEvent(T wfInstnace)
		{
			wfInstnace.WasSet = true;
		}

		public Event ParentEvent { get; set; }
	}

	public class MyStateMachine : CommonStateMachine<CommonInstance>
	{
		public MyStateMachine()
		{
			State(() => SomeState);

			Initially(When(Initial.Enter).TransitionTo(SomeState));
			During(SomeState,
				When(SomeState.Enter).Then(wf => DoStuff(wf)));
		}

		public void DoStuff(CommonInstance wf)
		{
			this.RaiseEvent(wf, ParentEvent);
		}

		public State SomeState { get; set; }
	}

	[TestClass]
	public class AbstractFactoryStateMachineTests
	{
		[TestMethod]
		public void DuringAny_event_defined_in_parent_should_work()
		{
			CommonInstance instance = new CommonInstance();
			MyStateMachine sut = MyStateMachine.Create(() => new MyStateMachine());

			sut.RaiseEvent(instance, sut.Initial.Enter);

			Assert.AreEqual("SomeState", instance.CurrentState.Name);
			Assert.IsTrue(instance.WasSet);
		}
	}
}
