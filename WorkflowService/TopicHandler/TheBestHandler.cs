using Automatonymous;
using Domain.Messages;
using Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Services;
using WorkflowService.Wiring;

namespace WorkflowService.TopicHandler
{
	public class TheBestHandler
	{
		private Dictionary<string, BaseInstance> workflowInstances = new Dictionary<string, BaseInstance>();
		private readonly IStateMachineMapper mapper;

		public TheBestHandler(IStateMachineMapper mapper)
		{
			this.mapper = mapper;
		}

    public void Handle(PaymentConfirmed confirmation)
    {
      if (workflowInstances.ContainsKey(confirmation.PhoneNumber))
      {
        Workflows.MovieBookingInstance instance = (Workflows.MovieBookingInstance)workflowInstances[confirmation.PhoneNumber];
        var stateMachine = this.mapper.GetStateMachine(instance.GetType());

        Workflows.MovieBookingWorkflow wf = (Workflows.MovieBookingWorkflow)mapper.GetStateMachine(instance.GetType());
        wf.RaiseEvent(instance, wf.PaymentConfirmed);
      }
    }

		public void Handle(SmsReceived sms)
		{
			if (workflowInstances.ContainsKey(sms.PhoneNumber))
			{
				//then raise the SMS received event
				BaseInstance instance = workflowInstances[sms.PhoneNumber];
				var stateMachine = this.mapper.GetStateMachine(instance.GetType());

				stateMachine.RaiseAnEvent(instance, stateMachine.SMSReceived, sms.Body);
			}
			else if (this.mapper.MappingExists(sms.Body))
			{
				//make a new state machine instance
				BaseInstance instance = this.mapper.GetStateMachineInstance(sms.Body);
				IWorkflow stateMachine = this.mapper.GetStateMachine(sms.Body);

				stateMachine.RaiseAnEvent(instance, stateMachine.Start, sms.PhoneNumber);
				this.workflowInstances[sms.PhoneNumber] = instance;
			}
			else
			{
				//send back an error :-(
			}
		}
	}
}
