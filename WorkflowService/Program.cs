using Domain.Messages;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			
			TheBestHandler handler = new TheBestHandler(new StateMachineMapper(bus, commonWorkflowService));

			bus.Subscribe<SmsReceived>("workflow-service", handler.Handle);

			Console.ReadKey();
		}
	}
}
