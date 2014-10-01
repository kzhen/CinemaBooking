using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Messages;
using WorkflowService.Wiring;

namespace WorkflowService.TopicHandler
{
  public class ForkHandler
  {
    private readonly IWorkflowInstanceRepository instanceRepository;
    private readonly IStateMachineMapper mapper;

    public ForkHandler(IWorkflowInstanceRepository instanceRepository, IStateMachineMapper mapper)
    {
      this.instanceRepository = instanceRepository;
      this.mapper = mapper;
    }
    internal void Handle(ForkFinished msg)
    {
      if (msg.Abort)
      {
        //start new
        instanceRepository.Pop(msg.PhoneNumber);
        var wf = mapper.GetStateMachine(msg.ForkToKeyword);
        var instance = mapper.GetStateMachineInstance(msg.ForkToKeyword);

        wf.RaiseAnEvent(instance, wf.Start, msg.PhoneNumber);

        instanceRepository.Push(msg.PhoneNumber, instance);
      }
    }
  }
}
