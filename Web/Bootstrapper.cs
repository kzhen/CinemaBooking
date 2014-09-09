namespace Web
{
  using Infrastructure;
  using Nancy;

  public class Bootstrapper : DefaultNancyBootstrapper
  {
    // The bootstrapper enables you to reconfigure the composition of the framework,
    // by overriding the various methods and properties.
    // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper
    protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
    {
      container.Register<IBus, AzureBus>().AsSingleton();
      
      base.ApplicationStartup(container, pipelines);
    }
  }
}