using Automatonymous;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowServiceTests
{
  internal class MyObserver : IObserver<StateChanged<MyInstance>>
  {
    public bool Completed { get; set; }
    public void OnCompleted()
    {
      Completed = true;
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(StateChanged<MyInstance> value)
    {
      if (value.Current.Name.Equals("Final"))
      {
        OnCompleted();
      }
    }
  }

  internal class MyInstance
  {
    public State CurrentState { get; set; }
  }

  internal class MyStateMachine2 : Automatonymous.AutomatonymousStateMachine<MyInstance>
  {
    public MyStateMachine2()
    {
      Event(() => Start);
      Event(() => Next);

      State(() => Stage1);
      State(() => Stage2);

      Initially(When(Start).TransitionTo(Stage1));
      During(Stage1, When(Next).TransitionTo(Stage2));
      During(Stage2, When(Next).Finalize());
    }

    public Event Start { get; set; }

    public Event Next { get; set; }

    public State Stage1 { get; set; }

    public State Stage2 { get; set; }
  }

  [TestClass]
  public class ObservableTests
  {
    [TestMethod]
    public void Should_Call_Complete()
    {
      MyInstance instance = new MyInstance();
      MyStateMachine2 machine = new MyStateMachine2();

      MyObserver observer = new MyObserver();

      machine.StateChanged.Subscribe(observer);


      machine.RaiseEvent(instance, machine.Start);
      machine.RaiseEvent(instance, machine.Next);
      machine.RaiseEvent(instance, machine.Next);

      Assert.AreEqual("Final", instance.CurrentState.Name);

      Assert.IsTrue(observer.Completed);
    }
  }
}
