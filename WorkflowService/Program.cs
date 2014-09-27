using Domain.Messages;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Messages;
using WorkflowService.Services;
using WorkflowService.TopicHandler;
using WorkflowService.Wiring;

namespace WorkflowService
{
	class Program
	{
		static void Main(string[] args)
		{
			IBus bus = new AzureBus();

			IMovieBookingService movieBookingService = new MovieBookingService(bus);
			ICommonWorkflowService commonWorkflowService = new CommonWorkflowService(bus);
      IStateMachineMapper stateMachineMapper = new StateMachineMapper(bus, commonWorkflowService);
      WorkflowInstanceRepository instanceRepository = new WorkflowInstanceRepository();

			SmsHandler smsHandler = new SmsHandler(stateMachineMapper, bus, instanceRepository);
      ForkHandler forkHandler = new ForkHandler(instanceRepository, stateMachineMapper);

			bus.Subscribe<SmsReceived>("workflow-service", smsHandler.Handle);
      bus.Subscribe<ForkFinished>("workflow-service", forkHandler.Handle);

			Console.ReadKey();
		}
	}
}
