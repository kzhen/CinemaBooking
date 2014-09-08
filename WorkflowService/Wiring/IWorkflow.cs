using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowService.Wiring
{
	public interface IWorkflow
	{
		Event<string> SMSReceived { get; set; }
		Event<string> Start { get; set; }
		void RaiseAnEvent(BaseInstance instance, Event @event);
		void RaiseAnEvent(BaseInstance instance, Event<string> @event, string data);
	}
}
