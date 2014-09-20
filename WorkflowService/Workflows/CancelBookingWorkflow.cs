using Automatonymous;
using Domain.Messages;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Services;
using WorkflowService.Wiring;

namespace WorkflowService.Workflows
{
	public class CancelBookingInstance : BaseInstance
	{
	}

	public class CancelBookingWorkflow : BaseStateMachine<CancelBookingInstance>, IWorkflow
	{
		private IBus bus;
		public CancelBookingWorkflow(IBus bus, ICommonWorkflowService commonWorkflowService)
			: base(commonWorkflowService)
		{
			this.bus = bus;

			Initially(When(Start).Then((wf, data) => LookupReservations(wf, data)));
		}

		private void LookupReservations(CancelBookingInstance wf, string data)
		{
			bus.Publish<SendSms>(new SendSms { PhoneNumber = data, Body = "Please select" });
		}
	}
}
