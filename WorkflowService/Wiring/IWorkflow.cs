using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkflowService.Wiring
{
  public interface IWorkflow
  {
    Event<string> SMSReceived { get; }
    Event<string> Start { get; }
    Event<string> Continue { get; }
    State Final { get; }
    State Initial { get; }
    void RaiseAnEvent(BaseInstance instance, Event @event);
    void RaiseAnEvent<T>(BaseInstance instance, Event<T> @event, T data);
  }
}
