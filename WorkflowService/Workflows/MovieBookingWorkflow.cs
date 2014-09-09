using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Services;
using WorkflowService.TopicHandler;
using WorkflowService.Wiring;

namespace WorkflowService.Workflows
{
	public class MovieBookingInstance : BaseInstance
	{
		public string PhoneNumber { get; set; }
		public State CurrentState { get; set; }
		public string CinemaKey { get; set; }
		public string MovieKey { get; set; }
		public string SlotKey { get; set; }
	}

	public class MovieBookingWorkflow : BaseStateMachine<MovieBookingInstance> //AutomatonymousStateMachine<MovieBookingInstance>
	{
		private readonly IMovieBookingService movieBookingService;
		public MovieBookingWorkflow(IMovieBookingService service)
		{
			this.movieBookingService = service;

			InstanceState(i => i.CurrentState);

      State(() => WaitingForPayment);
			State(() => WaitingForCinemaSelection);
			State(() => WaitingForMovieSelection);
			State(() => WaitingForSlotSelection);
			State(() => Completed);

			//Event(() => Start);
			//Event(() => SMSReceived);
      Event(() => PaymentConfirmed);
			Event(() => ValidResponse);
			Event(() => InvalidResponse);
			Event(() => MoreSlotsRequested);

			DuringAny(
				When(InvalidResponse).Then(wf => SendUnknownResponse(wf)));

			Initially(When(Start).Then((wf, data) => SendListOfCinemas(wf, data)).TransitionTo(WaitingForCinemaSelection));
			During(WaitingForCinemaSelection,
				When(SMSReceived).Then((wf, data) => ProcessCinemaSelection(wf, data)),
				When(ValidResponse).Then(wf => SendListOfMovies(wf)).TransitionTo(WaitingForMovieSelection)
				);
			During(WaitingForMovieSelection,
				When(SMSReceived).Then((wf, data) => ProcessMovieSelection(wf, data)),
				When(ValidResponse).Then(wf => SendMovieSlots(wf)).TransitionTo(WaitingForSlotSelection)
				);
			During(WaitingForSlotSelection,
				When(SMSReceived, (msg) => !msg.Equals("MORE")).Then((wf, data) => ProcessSlotSelection(wf, data)),
				When(SMSReceived, (msg) => msg.Equals("MORE")).Then((wf) => this.RaiseEvent(wf, MoreSlotsRequested)),
				When(MoreSlotsRequested).Then((wf) => SendMovieSlots(wf)),
				When(ValidResponse).Then(wf=>AskForPayment(wf)).TransitionTo(WaitingForPayment)
				);
      During(WaitingForPayment,
        When(PaymentConfirmed).Then(wf => SendConfirmationCode(wf)).Finalize());
		}

    private void AskForPayment(MovieBookingInstance instance)
    {
      this.movieBookingService.AskForPayment(instance.PhoneNumber);
    }

		private void SendUnknownResponse(MovieBookingInstance instance)
		{
			this.movieBookingService.SendUnknownResponse(instance.PhoneNumber);
		}

		private void SendConfirmationCode(MovieBookingInstance instance)
		{
      this.movieBookingService.SendConfirmation(instance.PhoneNumber);
		}

		private void ProcessSlotSelection(MovieBookingInstance instance, string data)
		{
      instance.SlotKey = data;
      this.RaiseEvent(instance, ValidResponse);
		}

		private void SendMovieSlots(MovieBookingInstance instance)
		{
			this.movieBookingService.SendMovieSlots(instance.PhoneNumber, instance.CinemaKey, instance.MovieKey);
		}

		private void ProcessMovieSelection(MovieBookingInstance instance, string data)
		{
			string movie = this.movieBookingService.GetMovie(data);

			if (string.IsNullOrWhiteSpace(movie))
			{
				this.RaiseEvent(instance, InvalidResponse);
			}
			else
			{
				instance.MovieKey = data;
				this.RaiseEvent(instance, ValidResponse);
			}
		}

		private void SendListOfMovies(MovieBookingInstance instance)
		{
			this.movieBookingService.SendMovieSelection(instance.PhoneNumber, instance.CinemaKey);
		}

		private void ProcessCinemaSelection(MovieBookingInstance instance, string data)
		{
			var selection = this.movieBookingService.GetCinema(data);

			if (string.IsNullOrWhiteSpace(selection))
			{
				this.RaiseEvent(instance, InvalidResponse);
			}
			else
			{
				instance.CinemaKey = data;
				this.RaiseEvent(instance, ValidResponse);
			}
		}

		public void SendListOfCinemas(MovieBookingInstance instance, string phoneNumber)
		{
			instance.PhoneNumber = phoneNumber;

			List<string> cineams = this.movieBookingService.GetListOfCinemas();
			this.movieBookingService.SendCinemaSelection(instance.PhoneNumber, cineams);
		}

		public State WaitingForCinemaSelection { get; set; }
		public State WaitingForMovieSelection { get; set; }
		public State WaitingForSlotSelection { get; set; }
		public State Completed { get; set; }

		//public Event<string> Start { get; set; }
		//public Event<string> SMSReceived { get; set; }
		public Event ValidResponse { get; set; }
		public Event InvalidResponse { get; set; }
		public Event MoreSlotsRequested { get; set; }

    public Event PaymentConfirmed { get; set; }

    public Automatonymous.State WaitingForPayment { get; set; }
  }
}
