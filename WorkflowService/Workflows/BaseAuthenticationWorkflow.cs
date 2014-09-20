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
	public class AuthenticationInstance : BaseInstance
	{
	}

	public abstract class BaseAuthenticationWorkflow<T> : BaseStateMachine<T>
		where T : AuthenticationInstance
	{
		private readonly IBus bus;
		public BaseAuthenticationWorkflow(IBus bus, ICommonWorkflowService commonWorkflowService)
			: base(commonWorkflowService)
		{
			this.bus = bus;

			State(() => WaitingForSurname);
			State(() => WaitingForDob);
			State(() => Authorized);
			State(() => Unauthorized);

			Event(() => ValidResponse);
			Event(() => InvalidResponse);

			Initially(
				When(Start)
				.Then((wf, phoneNumber) => wf.PhoneNumber = phoneNumber)
				.Then(wf => AskForSurname(wf)).TransitionTo(WaitingForSurname)
				);
			During(WaitingForSurname,
				When(SMSReceived).Then((wf, data) => ProcessSurname(wf, data)),
				When(ValidResponse).Then(wf => AskForDob(wf)).TransitionTo(WaitingForDob),
				When(InvalidResponse).TransitionTo(Unauthorized)
				);
			During(WaitingForDob,
				When(SMSReceived).Then((wf, data) => ProcessDob(wf, data)),
				When(ValidResponse).TransitionTo(Authorized),
				When(InvalidResponse).TransitionTo(Unauthorized)
				);

			During(Authorized, When(Authorized.Enter).Then(wf => OnPassedAuthentication(wf)));
			During(Unauthorized, When(Unauthorized.Enter).Then(wf => OnFailedAuthentication(wf)));
		}

		private void AskAgain(T wf)
		{
			this.bus.Publish(new SendSms() { PhoneNumber = wf.PhoneNumber, Body = "Unrecognized." });
		}
		private void ProcessDob(T wf, string data)
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				this.RaiseEvent(wf, InvalidResponse);
			}
			else
			{
				this.RaiseEvent(wf, ValidResponse);
			}
		}

		private void AskForDob(T instance)
		{
			this.bus.Publish(new SendSms() { PhoneNumber = instance.PhoneNumber, Body = "Please send your date of birth" });
		}

		private void ProcessSurname(T wf, string data)
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				this.RaiseEvent(wf, InvalidResponse);
			}
			else
			{
				this.RaiseEvent(wf, ValidResponse);
			}
		}

		private void AskForSurname(T instance)
		{
			this.bus.Publish(new SendSms() { PhoneNumber = instance.PhoneNumber, Body = "Please send your surname" });
		}

		public abstract void OnFailedAuthentication(T instance);
		public abstract void OnPassedAuthentication(T instance);

		public State WaitingForSurname { get; set; }
		public State WaitingForDob { get; set; }
		public State Authorized { get; set; }
		public State Unauthorized { get; set; }

		public Event ValidResponse { get; set; }
		public Event InvalidResponse { get; set; }
	}
}
