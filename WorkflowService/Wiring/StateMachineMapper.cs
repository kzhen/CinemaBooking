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
		private readonly List<string> keywords = new List<string>() { "BOOK", "CANCEL", "CHANGE" };
		private readonly Dictionary<string, Func<BaseInstance>> keywordMapper = new Dictionary<string, Func<BaseInstance>>()
		{
			{ "BOOK", () => new Workflows.MovieBookingInstance() },
			{ "CHANGE", () => new AuthenticatedChangeBookingInstance() }
		};
		private readonly Dictionary<string, Func<IBus, IWorkflow>> stateMachineKeywordMapper = new Dictionary<string, Func<IBus, IWorkflow>>()
		{
			{ "CHANGE", (bus) => new AuthenticatedChangeBookingWorkflow(bus) },
			{ "BOOK", (bus) => new Workflows.MovieBookingWorkflow(new MovieBookingService(bus)) }
		};
		private readonly Dictionary<Type, Func<IBus, IWorkflow>> stateMachineTypeMapper = new Dictionary<Type, Func<IBus, IWorkflow>>()
		{
			{ typeof(AuthenticatedChangeBookingInstance), (bus) => new AuthenticatedChangeBookingWorkflow(bus) },
			{ typeof(Workflows.MovieBookingInstance), (bus) => new Workflows.MovieBookingWorkflow(new MovieBookingService(bus)) }
		};

		public StateMachineMapper(IBus bus)
		{
			this.bus = bus;
		}

		public IWorkflow GetStateMachine(Type type)
		{
			if (stateMachineTypeMapper.ContainsKey(type))
			{
				return stateMachineTypeMapper[type](this.bus);
			}

			return null;
		}

		public IWorkflow GetStateMachine(string keyword)
		{
			if (stateMachineKeywordMapper.ContainsKey(keyword))
			{
				return stateMachineKeywordMapper[keyword](this.bus);
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
