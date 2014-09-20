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

			//Ideally we would setup common event handlers here e.g. InvalidResponse should fire the SendUnknownResponse method
			//this could be done by implementing an AbstractFactory to create the actual State Machine, see http://stackoverflow.com/a/2747280
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
