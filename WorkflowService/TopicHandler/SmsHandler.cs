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
	public class SmsHandler
	{
		private Dictionary<string, BaseInstance> workflowInstances = new Dictionary<string, BaseInstance>();
		private readonly IStateMachineMapper mapper;
		private readonly IBus bus;

		public SmsHandler(IStateMachineMapper mapper, IBus bus)
		{
			this.mapper = mapper;
			this.bus = bus;
		}

		public void Handle(SmsReceived sms)
		{
			if (workflowInstances.ContainsKey(sms.PhoneNumber))
			{
				//then raise the SMS received event
				BaseInstance instance = workflowInstances[sms.PhoneNumber];
				var stateMachine = this.mapper.GetStateMachine(instance.GetType());

				stateMachine.RaiseAnEvent(instance, stateMachine.SMSReceived, sms.Body);

				if (instance.CurrentState == stateMachine.Final)
				{
					workflowInstances.Remove(sms.PhoneNumber);
				}
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
				bus.Publish(new SendSms() { PhoneNumber = sms.PhoneNumber, Body = "That command was not recognized." });
			}
		}
	}
}
