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
	internal class ChangeBookingInstance : BaseInstance
	{
		public string PhoneNumber { get; set; }

		public string BookingKey { get; set; }
	}
	internal class ChangeBookingWorkflow : BaseStateMachine<ChangeBookingInstance>
	{
		private readonly IBus bus;
		public ChangeBookingWorkflow(IBus bus)
			: base()
		{
			this.bus = bus;

			State(() => WaitingForBookingSelection);

			Event(() => ValidResponse);

			Initially(When(Start)
				.Then((wf, phoneNumber) => wf.PhoneNumber = phoneNumber)
				.Then(wf => SendListOfBookings(wf))
				.TransitionTo(WaitingForBookingSelection));
			During(WaitingForBookingSelection,
				When(SMSReceived).Then((wf, data) => ProcessBookingSelection(wf, data)),
				When(ValidResponse).Then(wf => SendConfirmationQuestion(wf)).TransitionTo(WaitingForConfirmation)
				);
		}

		private void SendConfirmationQuestion(ChangeBookingInstance instance)
		{
			this.bus.Publish<SendSms>(new SendSms() { PhoneNumber = instance.PhoneNumber, Body = "Are you sure?" });
		}

		private void ProcessBookingSelection(ChangeBookingInstance wf, string data)
		{
			wf.BookingKey = data;

			this.bus.Publish<SendSms>(new SendSms() { PhoneNumber = wf.PhoneNumber, Body = "You have selected:" + data });

			this.RaiseEvent(wf, ValidResponse);
		}

		private void SendListOfBookings(ChangeBookingInstance wf)
		{
			this.bus.Publish<SendSms>(new SendSms() { PhoneNumber = wf.PhoneNumber, Body = "List of bookings:" });
		}

		public State WaitingForBookingSelection { get; set; }
		public State WaitingForConfirmation { get; set; }

		public Event ValidResponse { get; set; }
	}
}
