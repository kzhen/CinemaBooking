using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Services;

namespace WorkflowService.Wiring
{
	public class BaseInstance
	{
		public string PhoneNumber { get; set; }
		public State CurrentState { get; set; }
	}

	public abstract class BaseStateMachine<T> : AutomatonymousStateMachine<T>, IWorkflow
		where T : BaseInstance
	{
		private ICommonWorkflowService commonWorkflowServer;
		public BaseStateMachine(ICommonWorkflowService commonWorkflowService)
		{
			this.commonWorkflowServer = commonWorkflowService;

			InstanceState(i => i.CurrentState);

			Event(() => SMSReceived);
			Event(() => Start);
			Event(() => InvalidResponse);
		}

		protected virtual void SendUnknownResponse(T instance)
		{
			this.commonWorkflowServer.SendUnknownResponse(instance.PhoneNumber);
		}

		public Event<string> SMSReceived { get; set; }
		public Event<string> Start { get; set; }
		public Event InvalidResponse { get; set; }

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
