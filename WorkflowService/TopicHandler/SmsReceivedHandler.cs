//using Automatonymous;
//using Domain.Messages;
//using Infrastructure;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using WorkflowService.Services;
//using WorkflowService.Workflows;

//namespace WorkflowService.TopicHandler
//{
//	internal class SmsReceivedHandler
//	{
//		private readonly IBus bus;
//		private ConcurrentDictionary<string, MovieBookingInstance> workflowsInProgress = new ConcurrentDictionary<string, MovieBookingInstance>();

//		public SmsReceivedHandler(IBus bus)
//		{
//			this.bus = bus;
//		}

//		public void Handle(SmsReceived sms)
//		{
//			Console.WriteLine("Received message: {0} {1}", sms.PhoneNumber, sms.Body);

//			if (workflowsInProgress.ContainsKey(sms.PhoneNumber))
//			{
//				MovieBookingService service = new MovieBookingService(bus);
//				MovieBookingWorkflow wf = new MovieBookingWorkflow(service);
//				wf.RaiseEvent(workflowsInProgress[sms.PhoneNumber], wf.SMSReceived, sms.Body);
//			}
//			else if (sms.Body.Equals("BOOK", StringComparison.InvariantCultureIgnoreCase))
//			{
//				MovieBookingService service = new MovieBookingService(bus);
//				MovieBookingWorkflow wf = new MovieBookingWorkflow(service);
//				workflowsInProgress[sms.PhoneNumber] = new MovieBookingInstance();
//				wf.RaiseEvent(workflowsInProgress[sms.PhoneNumber], wf.Start, sms.PhoneNumber);
//			}
//			else
//			{
//				bus.Publish(new SendSms() { PhoneNumber = sms.PhoneNumber, Body = "Unrecognized Command" });
//			}
//		}
//	}
//}
