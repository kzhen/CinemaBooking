using Automatonymous;
using Domain.Messages;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Messages;
using WorkflowService.Services;
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
		public AuthenticatedChangeBookingWorkflow(IBus bus, ICommonWorkflowService commonWorkflowService)
			: base(bus, commonWorkflowService)
		{
			this.bus = bus;

			State(() => WaitingForBookingSelection);
			State(() => WaitingForConfirmation);
			State(() => WaitingForSelectionDisambiguation);

			State(() => Starting);

			Event(() => BookingsFound);

			During(Starting,
				When(Starting.Enter).Then(wf => LookupBookings(wf)),
				When(BookingsFound, numFound => numFound == 0).Then(wf => { /*send sorry*/ }).Finalize(),
				When(BookingsFound, numFound => numFound == 1).Then(wf => SendConfirmationQuestion(wf)).TransitionTo(WaitingForConfirmation),
				When(BookingsFound, numFound => numFound > 1).Then(wf => DisambiguateBooking(wf)).TransitionTo(WaitingForSelectionDisambiguation)
				);
			During(WaitingForSelectionDisambiguation,
				When(Continue).Then((wf, data) => TheNextThing(wf, data)).TransitionTo(WaitingForConfirmation));

		}

		private void TheNextThing(AuthenticatedChangeBookingInstance wf, string data)
		{
			wf.BookingKey = data;
		}

		private void DisambiguateBooking(AuthenticatedChangeBookingInstance wf)
		{
			this.bus.Publish<StartDisambiguateMovieBooking>(new StartDisambiguateMovieBooking() { PhoneNumber = wf.PhoneNumber });
		}

		private void LookupBookings(AuthenticatedChangeBookingInstance instance)
		{
			int numBookings = 5;
			this.RaiseEvent(instance, BookingsFound, numBookings);
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
		public State WaitingForSelectionDisambiguation { get; set; }

		public Event<int> BookingsFound { get; set; }
	}
}
