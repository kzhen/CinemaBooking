using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Messages;
using WorkflowService.Wiring;
using WorkflowService.Workflows;

namespace WorkflowService.TopicHandler
{
	public class DisambiguateMovieBookingHandler
	{
		private WorkflowInstanceRepository instanceRepository;
		private IStateMachineMapper stateMachineMapper;

		public DisambiguateMovieBookingHandler(WorkflowInstanceRepository instanceRepository, Wiring.IStateMachineMapper stateMachineMapper)
		{
			this.instanceRepository = instanceRepository;
			this.stateMachineMapper = stateMachineMapper;
		}

		public void Handle(StartDisambiguateMovieBooking message)
		{
			var instance = new DisambiguateMovieBookingInstance()
			{
				PhoneNumber = message.PhoneNumber
			};

			var stateMachine = stateMachineMapper.GetStateMachine(instance);

			stateMachine.RaiseAnEvent(instance, stateMachine.Start, message.PhoneNumber);

			instanceRepository.Push(message.PhoneNumber, instance);
		}

		public void Handle(DisambiguateMovieBookingFinished message)
		{
			var instance = this.instanceRepository.Peek(message.PhoneNumber);

			var stateMachine = stateMachineMapper.GetStateMachine(instance);

			stateMachine.RaiseAnEvent(instance, stateMachine.Continue, message.MovieId);
		}
	}
}
