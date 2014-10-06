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

	public abstract class BaseStateMachine<TInstance> : AutomatonymousStateMachine<TInstance>, IWorkflow
		where TInstance : BaseInstance
	{
		private ICommonWorkflowService commonWorkflowServer;
		public BaseStateMachine(ICommonWorkflowService commonWorkflowService)
		{
			this.commonWorkflowServer = commonWorkflowService;

			InstanceState(i => i.CurrentState);

			Event(() => SMSReceived);
			Event(() => Start);
      Event(() => InvalidResponse);
      Event(() => ValidResponse);

			//Ideally we would setup common event handlers here e.g. InvalidResponse should fire the SendUnknownResponse method
			//this could be done by implementing an AbstractFactory to create the actual State Machine, see http://stackoverflow.com/a/2747280
		}

		protected virtual void SendUnknownResponse(TInstance instance)
		{
			this.commonWorkflowServer.SendUnknownResponse(instance.PhoneNumber);
		}

		public Event<string> SMSReceived { get; set; }
		public Event<string> Start { get; set; }
		public Event InvalidResponse { get; set; }
    public Event ValidResponse { get; set; }

		public void RaiseAnEvent<T>(BaseInstance instance, Event<T> @event, T data)
		{
			this.RaiseEvent(instance as TInstance, @event, data);
		}

		public void RaiseAnEvent(BaseInstance instance, Event @event)
		{
			this.RaiseEvent(instance as TInstance, @event);
		}
	}
}
