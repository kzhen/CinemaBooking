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

			//SmsReceivedHandler handler = new SmsReceivedHandler(bus);
			IMovieBookingService service = new MovieBookingService(bus);
			//BetterHandler handler = new BetterHandler(service);
			TheBestHandler handler = new TheBestHandler(new StateMachineMapper(bus));

			bus.Subscribe<SmsReceived>("workflow-service", handler.Handle);

			Console.ReadKey();
		}
	}
}
