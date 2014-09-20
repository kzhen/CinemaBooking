using Domain.Messages;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowService.Services
{
	public interface ICommonWorkflowService
	{
		void SendUnknownResponse(string phoneNumber);
	}

	public class CommonWorkflowService : ICommonWorkflowService
	{
		private IBus bus;
		public CommonWorkflowService(IBus bus)
		{
			this.bus = bus;
		}
		public void SendUnknownResponse(string phoneNumber)
		{
			bus.Publish(new SendSms() { PhoneNumber = phoneNumber, Body = "That was unrecognized, please try again!" });
		}
	}
}
