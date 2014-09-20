using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Services;
using WorkflowService.TopicHandler;
using WorkflowService.Workflows;

namespace WorkflowService.Wiring
{
	public interface IStateMachineMapper
	{
		IWorkflow GetStateMachine(Type type);
		IWorkflow GetStateMachine(string keyword);
		BaseInstance GetStateMachineInstance(string keyword);
		bool MappingExists(string keyword);
	}

	public class StateMachineMapper : IStateMachineMapper
	{
		private readonly IBus bus;
		private readonly ICommonWorkflowService commonWorkflowService;

		private readonly List<string> keywords = new List<string>() { "BOOK", "CANCEL", "CHANGE" };
		private readonly Dictionary<string, Func<BaseInstance>> keywordMapper = new Dictionary<string, Func<BaseInstance>>()
		{
			{ "BOOK", () => new Workflows.MovieBookingInstance() },
			{ "CHANGE", () => new AuthenticatedChangeBookingInstance() }
		};
		private readonly Dictionary<string, Func<IBus, ICommonWorkflowService, IWorkflow>> stateMachineKeywordMapper = new Dictionary<string, Func<IBus, ICommonWorkflowService, IWorkflow>>()
		{
			{ "CHANGE", (bus, commonWorkflowService) => new AuthenticatedChangeBookingWorkflow(bus, commonWorkflowService) },
			{ "BOOK", (bus, commonWorkflowService) => new Workflows.MovieBookingWorkflow(new MovieBookingService(bus), commonWorkflowService) }
		};
		private readonly Dictionary<Type, Func<IBus, ICommonWorkflowService, IWorkflow>> stateMachineTypeMapper = new Dictionary<Type, Func<IBus, ICommonWorkflowService, IWorkflow>>()
		{
			{ typeof(AuthenticatedChangeBookingInstance), (bus, commonWorkflowService) => new AuthenticatedChangeBookingWorkflow(bus, commonWorkflowService) },
			{ typeof(Workflows.MovieBookingInstance), (bus, commonWorkflowService) => new Workflows.MovieBookingWorkflow(new MovieBookingService(bus), commonWorkflowService) }
		};
		

		public StateMachineMapper(IBus bus, ICommonWorkflowService commonWorkflowService)
		{
			this.bus = bus;
			this.commonWorkflowService = commonWorkflowService;
		}

		public IWorkflow GetStateMachine(Type type)
		{
			if (stateMachineTypeMapper.ContainsKey(type))
			{
				return stateMachineTypeMapper[type](this.bus, this.commonWorkflowService);
			}

			return null;
		}

		public IWorkflow GetStateMachine(string keyword)
		{
			if (stateMachineKeywordMapper.ContainsKey(keyword))
			{
				return stateMachineKeywordMapper[keyword](this.bus, this.commonWorkflowService);
			}

			return null;
		}

		public BaseInstance GetStateMachineInstance(string keyword)
		{
			if (keywordMapper.ContainsKey(keyword))
			{
				return keywordMapper[keyword]();
			}

			return null;
		}

		public bool MappingExists(string keyword)
		{
			return this.keywords.Contains(keyword);
		}
	}
}
