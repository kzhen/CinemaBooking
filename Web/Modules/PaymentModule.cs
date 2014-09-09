using Infrastructure;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.ModelBinding;
using Domain.Messages;

namespace Web.Modules
{
  public class PaymentModule : NancyModule
  {
    public PaymentModule(IBus bus)
      : base("payment")
    {
      Get["/"] = _ =>
        {
          return View["Payment"];
        };

      Post["/"] = _ =>
      {
        var model = this.Bind<ViewModels.PaymentDetailsViewModel>();

        //ProcessPayment(model);

        PaymentConfirmed msg = new PaymentConfirmed()
        {
          PhoneNumber = model.PhoneNumber
        };

        bus.Publish(msg);

        return HttpStatusCode.OK;
      };
    }
  }
}