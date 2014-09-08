Cinema Booking
=============
This is an example solution to demonstrate working with [Automatonymous](https://github.com/MassTransit/Automatonymous) State Machines.

The solution has two endpoint projects:

* WorkflowService
* SmsReceiverService

These two projects communicate via Azure Service Bus Topics. The `WorkflowService` subscribes to SmsReceived messages and publishes SendSms messages. The `SmsReceiverService` is a console app that allows you to 'pretend' to be a mobile phone and simulate sending and receiving SMS messages. 

To get started Build the solution then start debugging, Visual Studio should launch both projects. To get started with the SmsReceiverService type in: 

    !PH [then a phonenumber]

From now on anything you type in will trigger the console app to publish an SmsReceived message, which will then be picked up by the WorkflowService.

### Reusable Components ###

All the StateMachines in the WorkflowService inherit from the abstract base class `BaseStateMachine<T>`, which inherits from `AutomatonymousStateMachine<T>` and implements `IWorkflow`.

There are several advantages of having the StateMachines inherit from the `BaseStateMachine<T>`:

* Shared Events (Start, SMSReceived)
* Can interact with a StateMachine without having to know the concrete type
* etc
 
The `BaseStateMachine<T>` also implements the `IWorkflow` interface, this is what provides the ability to interact with the StateMachine without know the concrete type.

### Wiring it up ###

...
