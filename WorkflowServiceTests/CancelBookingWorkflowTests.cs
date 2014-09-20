using Automatonymous;
using Domain.Messages;
using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Services;
using WorkflowService.Workflows;

namespace WorkflowServiceTests
{
	internal class InMemoryBus : IBus
	{
		private readonly Dictionary<Type, List<object>> subscriptions = new Dictionary<Type, List<object>>();

		private Dictionary<Type, List<object>> publishedMessages;

		public InMemoryBus()
		{
			publishedMessages = new Dictionary<Type, List<object>>();
		}

		public void Subscribe<T>(string subscriptionId, Action<T> onMessage) where T : class
		{
			var messageType = typeof(T);

			if (!subscriptions.ContainsKey(messageType))
			{
				subscriptions.Add(messageType, new List<object>());
			}
			subscriptions[messageType].Add(onMessage);
		}

		public void Publish<T>(T message) where T : class
		{
			var messageType = typeof(T);

			if (subscriptions.ContainsKey(messageType))
			{
				foreach (var action in subscriptions[messageType])
				{
					((Action<T>)action)(message);
				}
			}
			if (!publishedMessages.ContainsKey(messageType))
			{
				publishedMessages.Add(messageType, new List<object>());
			}
			publishedMessages[messageType].Add(message);
		}

		internal List<T1> PublishedMessages<T1>()
		{
			if (publishedMessages.ContainsKey(typeof(T1)))
			{
				return publishedMessages[typeof(T1)].Select(x => (T1)x).ToList();
			}

			return new List<T1>();
		}
	}

	[TestClass]
	public class CancelBookingWorkflowTests
	{
		[TestMethod]
		public void Base_Class_Events_Should_Be_Set()
		{
			InMemoryBus bus = new InMemoryBus();
			ICommonWorkflowService commonWorkflowService = Mock.Of<ICommonWorkflowService>();

			CancelBookingWorkflow wf = new CancelBookingWorkflow(bus, commonWorkflowService);

			Assert.IsNotNull(wf.Start);
			Assert.IsNotNull(wf.SMSReceived);
		}

		[TestMethod]
		public void Should_send_list_of_reservations()
		{
			InMemoryBus bus = new InMemoryBus();
			ICommonWorkflowService commonWorkflowService = Mock.Of<ICommonWorkflowService>();

			CancelBookingInstance instance = new CancelBookingInstance();
			CancelBookingWorkflow wf = new CancelBookingWorkflow(bus, commonWorkflowService);

			wf.RaiseEvent(instance, x => x.Start, "+447901234545");

			Assert.AreEqual(1, bus.PublishedMessages<SendSms>().Count);
			Assert.AreEqual("Please select", bus.PublishedMessages<SendSms>().First().Body);
		}
	}
}
