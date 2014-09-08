using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowService.Wiring
{
	public class BaseInstance
	{
		public State CurrentState { get; set; }
	}

	public abstract class BaseStateMachine<T> : AutomatonymousStateMachine<T>, IWorkflow
		where T : BaseInstance
	{
		public BaseStateMachine()
		{
			InstanceState(i => i.CurrentState);

			Event(() => SMSReceived);
			Event(() => Start);
		}

		public Event<string> SMSReceived { get; set; }
		public Event<string> Start { get; set; }

		public void RaiseAnEvent(BaseInstance instance, Event<string> @event, string data)
		{
			this.RaiseEvent(instance as T, @event, data);
		}

		public void RaiseAnEvent(BaseInstance instance, Event @event)
		{
			this.RaiseEvent(instance as T, @event);
		}
	}
}
