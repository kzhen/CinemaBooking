using Automatonymous;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowService.Wiring
{
  public class BaseStateMachineObserver<T> : IObserver<StateChanged<T>>
    //where T : class
    where T : BaseInstance
  {
    private readonly ILogger logger;
    public BaseStateMachineObserver(ILogger logger)
    {
      this.logger = logger;
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
      Console.WriteLine("Error! " + error.Message);
    }

    public void OnNext(StateChanged<T> value)
    {
      if (value.Current.Name.Equals("Final"))
      {
        OnCompleted();
      }

      this.logger.Information("InstanceId={id} Event=StateChanged Instance={instance} CurrentState={current} PreviousState={previous}", value.Instance.InstanceId, value.Instance.GetType().Name, value.Current.Name, (value.Previous != null ? value.Previous.Name : string.Empty));
    }
  }
}
