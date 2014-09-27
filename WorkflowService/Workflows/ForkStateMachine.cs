using Automatonymous;
using Domain.Messages;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Services;
using WorkflowService.Wiring;

namespace WorkflowService.Workflows
{
	public class ForkInstance : BaseInstance
	{
		public BaseInstance ForkingFromInstance { get; set; }
		public IWorkflow ForkingFromWorkflow { get; set; }
		public string ForkingToKeyword { get; set; }
	}

	public class ForkStateMachine : BaseStateMachine<ForkInstance>
	{
		private readonly IBus bus;
		public ForkStateMachine(ICommonWorkflowService commonWorkflowService, IBus bus)
			: base(commonWorkflowService)
		{
			this.bus = bus;

			State(() => WaitingForConfirmation);

			DuringAny(When(InvalidResponse).Then(wf => SendUnknownResponse(wf)));

			Initially(When(Start).Then((wf, data) => SendAreYouSure(wf, data)).TransitionTo(WaitingForConfirmation));
			During(WaitingForConfirmation,
				When(SMSReceived, msg => msg.Equals("Y")).Then(wf => CancelExisting(wf)).TransitionTo(Final),
				When(SMSReceived, msg => msg.Equals("N")).Then(wf => KeepGoingWithExisting(wf)).TransitionTo(Final)
				);
		}

		private void KeepGoingWithExisting(ForkInstance wf)
		{
			throw new NotImplementedException();
		}

		private void CancelExisting(ForkInstance wf)
		{
			bus.Publish(new SmsReceived() { PhoneNumber = wf.PhoneNumber, Body = wf.ForkingToKeyword });
		}

		private void SendAreYouSure(BaseInstance wf, string data)
		{
			wf.PhoneNumber = data;

			bus.Publish(new SendSms() { PhoneNumber = data, Body = "You were in the middle of something, sure you want to abort? Y/N" });
		}


		public State WaitingForConfirmation { get; set; }
	}
}
