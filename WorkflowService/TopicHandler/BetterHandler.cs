using Automatonymous;
using Domain.Messages;
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
	public class BetterHandler
	{
		public BetterHandler(IMovieBookingService service)
		{
			this.service = service;
		}

		private Dictionary<string, BaseInstance> workflowInstances = new Dictionary<string, BaseInstance>();

		private IWorkflow getStateMachineFromType(Type type)
		{
			if (type == typeof(AuthenticatedChangeBookingInstance))
			{
				return new AuthenticatedChangeBookingWorkflow(null);
			}
			else if (type == typeof(Workflows.MovieBookingInstance))
			{
				return new Workflows.MovieBookingWorkflow(service);
			}

			return null;
		}
		private IWorkflow getStateMachine(string keyword)
		{
			if (keyword.Equals("CHANGE"))
			{
				return new AuthenticatedChangeBookingWorkflow(null);
			}
			else if (keyword.Equals("BOOK"))
			{
				return new Workflows.MovieBookingWorkflow(service);
			}
			else if (keyword.Equals("CANCEL"))
			{
				return new CancelBookingWorkflow(null);
			}

			return null;
		}
		private Func<string, BaseInstance> getNewInstance = (keyword) =>
		{
			if (keyword.Equals("CHANGE"))
			{
				return new AuthenticatedChangeBookingInstance();
			}
			else if (keyword.Equals("BOOK"))
			{
				return new Workflows.MovieBookingInstance();
			}

			return null;
		};

		private Func<string, bool> recognizeKeyword = (keyword) =>
			{
				if (keyword.Equals("CHANGE"))
					return true;
				if (keyword.Equals("BOOK"))
					return true;

				return false;
			};
		private IMovieBookingService service;

		public void Handle(SmsReceived sms)
		{
			if (workflowInstances.ContainsKey(sms.PhoneNumber))
			{
				//then raise the SMS received event
				BaseInstance instance = workflowInstances[sms.PhoneNumber];
				var stateMachine = getStateMachineFromType(instance.GetType());

				stateMachine.RaiseAnEvent(instance, stateMachine.SMSReceived, sms.Body);
			}
			else if (recognizeKeyword(sms.Body))
			{
				//make a new state machine instance
				BaseInstance instance = getNewInstance(sms.Body);
				IWorkflow stateMachine = getStateMachine(sms.Body);

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
