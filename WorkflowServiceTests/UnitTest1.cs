using Automatonymous;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkflowService.Workflows;
using WorkflowService.Services;
using Moq;

namespace WorkflowServiceTests
{
	[TestClass]
	public class MovieBookingWorkflowTests
	{
		private MovieBookingWorkflow sut; //system-under-test
		private IMovieBookingService service;
		private ICommonWorkflowService commonWorkflowService;

		[TestInitialize]
		public void TestInitialize()
		{
			service = Mock.Of<IMovieBookingService>();
			commonWorkflowService = Mock.Of<ICommonWorkflowService>();

			sut = new MovieBookingWorkflow(service, commonWorkflowService);
		}

		[TestMethod]
		public void TestMethod1()
		{
			//Arrange
			string phoneNumber = "+441234567";
			MovieBookingInstance instance = new MovieBookingInstance();

			//Act
			sut.RaiseEvent(instance, sut.Start, phoneNumber);
			sut.RaiseEvent(instance, sut.SMSReceived, "");

			//Assert
			Assert.AreEqual("WaitingForCinemaSelection", instance.CurrentState.Name);
		}
	}
}
