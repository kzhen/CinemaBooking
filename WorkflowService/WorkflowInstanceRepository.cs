using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowService.Wiring;

namespace WorkflowService
{
  public interface IWorkflowInstanceRepository 
  {
    void Push(string phoneNumber, BaseInstance fork);
    BaseInstance Peek(string phoneNumber);
    BaseInstance Pop(string phoneNumber);
  }

  public class WorkflowInstanceRepository : IWorkflowInstanceRepository
  {
    private Dictionary<string, Stack<BaseInstance>> workflowInstances = new Dictionary<string, Stack<BaseInstance>>();

    public BaseInstance Peek(string phoneNumber)
    {
      if (workflowInstances.ContainsKey(phoneNumber) && workflowInstances[phoneNumber].Count > 0)
      {
        return workflowInstances[phoneNumber].Peek();
      }
      return null;
    }

    public void Push(string phoneNumber, BaseInstance instance)
    {
      if (!workflowInstances.ContainsKey(phoneNumber))
      {
        workflowInstances.Add(phoneNumber, new Stack<BaseInstance>());
      }
      workflowInstances[phoneNumber].Push(instance);
    }

    public BaseInstance Pop(string phoneNumber)
    {
      if (workflowInstances.ContainsKey(phoneNumber) && workflowInstances[phoneNumber].Count > 0)
      {
        return workflowInstances[phoneNumber].Pop();
      }
      return null;
    }
  }
}
