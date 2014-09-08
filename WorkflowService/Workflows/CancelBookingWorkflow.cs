using Automatonymous;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Wiring;

namespace WorkflowService.Workflows
{
	public class CancelBookingInstance : BaseInstance
	{
	}

	public class CancelBookingWorkflow : BaseStateMachine<CancelBookingInstance>, IWorkflow
	{
		private IBus bus;
		public CancelBookingWorkflow(IBus bus)
			: base()
		{
			this.bus = bus;
		}
	}
}
