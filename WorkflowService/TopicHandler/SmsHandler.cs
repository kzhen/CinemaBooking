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
using WorkflowService.Workflows;

namespace WorkflowService.TopicHandler
{
	public class SmsHandler
	{
		private readonly IStateMachineMapper mapper;
		private readonly IBus bus;
    private readonly IWorkflowInstanceRepository instanceRepository;


    public SmsHandler(IStateMachineMapper mapper, IBus bus, IWorkflowInstanceRepository instanceRepository)
		{
			this.mapper = mapper;
			this.bus = bus;
      this.instanceRepository = instanceRepository;
		}

		public void Handle(SmsReceived sms)
		{
      BaseInstance instance = instanceRepository.Peek(sms.PhoneNumber);

			if (instance != null)
			{
				if (this.mapper.MappingExists(sms.Body) && !(instance is ForkInstance))
				{
					ForkInstance fork = new ForkInstance()
					{
						ForkingFromInstance = instance,
						ForkingFromWorkflow = this.mapper.GetStateMachine(instance.GetType()),
						ForkingToKeyword = sms.Body,
						PhoneNumber = sms.PhoneNumber
					};

					var stateMachine = this.mapper.GetStateMachine(fork.GetType());

					stateMachine.RaiseAnEvent(fork, stateMachine.Start, sms.PhoneNumber);

          instanceRepository.Push(sms.PhoneNumber, fork);
				}
				else
				{
					//then raise the SMS received event
					var stateMachine = this.mapper.GetStateMachine(instance.GetType());

					stateMachine.RaiseAnEvent(instance, stateMachine.SMSReceived, sms.Body);

					if (instance.CurrentState == stateMachine.Final)
					{
						instanceRepository.Pop(sms.PhoneNumber);
					}
				}
			}
			else if (this.mapper.MappingExists(sms.Body))
			{
				//make a new state machine instance
				instance = this.mapper.GetStateMachineInstance(sms.Body);
				IWorkflow stateMachine = this.mapper.GetStateMachine(sms.Body);

				stateMachine.RaiseAnEvent(instance, stateMachine.Start, sms.PhoneNumber);
        instanceRepository.Push(sms.PhoneNumber, instance);
			}
			else
			{
				bus.Publish(new SendSms() { PhoneNumber = sms.PhoneNumber, Body = "That command was not recognized." });
			}
		}
	}
}
