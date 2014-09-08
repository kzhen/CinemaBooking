using Automatonymous;
using Domain.Messages;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Wiring;

namespace WorkflowService.Workflows
{
	internal class AuthenticatedChangeBookingInstance : AuthenticationInstance
	{
		public string BookingKey { get; set; }
	}
	internal class AuthenticatedChangeBookingWorkflow : BaseAuthenticationWorkflow<AuthenticatedChangeBookingInstance> //BaseStateMachine<ChangeBookingInstance>
	{
		private readonly IBus bus;
		public AuthenticatedChangeBookingWorkflow(IBus bus)
			: base(bus)
		{
			this.bus = bus;

			State(() => WaitingForBookingSelection);
			State(() => WaitingForConfirmation);
			State(() => Starting);
			Event(() => ValidResponse);

			During(Starting,
				When(Starting.Enter)
				.Then(wf => SendListOfBookings(wf))
				.TransitionTo(WaitingForBookingSelection));
			During(WaitingForBookingSelection,
				When(SMSReceived).Then((wf, data) => ProcessBookingSelection(wf, data)),
				When(ValidResponse).Then(wf => SendConfirmationQuestion(wf)).TransitionTo(WaitingForConfirmation)
				);
		}

		public override void OnFailedAuthentication(AuthenticatedChangeBookingInstance instance)
		{
			this.bus.Publish<SendSms>(new SendSms() { PhoneNumber = instance.PhoneNumber, Body = "Failed" });
		}

		public override void OnPassedAuthentication(AuthenticatedChangeBookingInstance instance)
		{
			this.TransitionToState(instance, Starting);
		}

		private void SendConfirmationQuestion(AuthenticatedChangeBookingInstance instance)
		{
			this.bus.Publish<SendSms>(new SendSms() { PhoneNumber = instance.PhoneNumber, Body = "Are you sure?" });
		}

		private void ProcessBookingSelection(AuthenticatedChangeBookingInstance wf, string data)
		{
			wf.BookingKey = data;

			this.bus.Publish<SendSms>(new SendSms() { PhoneNumber = wf.PhoneNumber, Body = "You have selected:" + data });

			this.RaiseEvent(wf, ValidResponse);
		}

		private void SendListOfBookings(AuthenticatedChangeBookingInstance wf)
		{
			this.bus.Publish<SendSms>(new SendSms() { PhoneNumber = wf.PhoneNumber, Body = "List of bookings:" });
		}

		public State WaitingForBookingSelection { get; set; }
		public State WaitingForConfirmation { get; set; }
		public State Starting { get; set; }
	}
}
