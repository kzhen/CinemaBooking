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
	public class DisambiguateMovieBookingInstance : BaseInstance
	{

		public string Selection { get; set; }
	}
	public class DisambiguateMovieBookingWorkflow : BaseStateMachine<DisambiguateMovieBookingInstance>
	{
		private readonly IBus bus;
		public DisambiguateMovieBookingWorkflow(ICommonWorkflowService commonWorkflowService, IBus bus)
			: base(commonWorkflowService)
		{
			this.bus = bus;

			State(() => WaitingForSelection);

			Event(() => ValidResponse);

			//setup state machine...

			/*
			 * Send list of movies
			 * WaitForValidSelection
			 */
			Initially(When(Start).Then((wf, ph) => SendListOfBookings(wf, ph)).TransitionTo(WaitingForSelection));
			During(WaitingForSelection,
				When(SMSReceived).Then((wf, msg) => ProcessSelection(wf, msg)),
				When(ValidResponse).Then(wf => Success(wf)).Finalize(),
				When(InvalidResponse).Then(wf => SendUnknownResponse(wf)));
		}

		private void Success(DisambiguateMovieBookingInstance wf)
		{
			this.bus.Publish(new DisambiguateMovieBookingFinished() { PhoneNumber = wf.PhoneNumber, MovieId = wf.Selection });
		}

		private void ProcessSelection(DisambiguateMovieBookingInstance wf, string msg)
		{
			List<string> validResponses = new List<string>() { "A", "B", "C" };

			if (validResponses.Contains(msg))
			{
				wf.Selection = msg;
				this.RaiseEvent(wf, ValidResponse);
			}
			else
			{
				this.RaiseEvent(wf, InvalidResponse);
			}
		}

		private void SendListOfBookings(DisambiguateMovieBookingInstance wf, string ph)
		{
			bus.Publish(new SendSms()
			{
				PhoneNumber = wf.PhoneNumber,
				Body = "Please select a booking, A,B,C"
			});
		}

		public Automatonymous.State WaitingForSelection { get; set; }

		public Event ValidResponse { get; set; }
	}
}
