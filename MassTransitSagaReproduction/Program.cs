using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using MassTransit;
using Topshelf;
using Topshelf.Autofac;

namespace MassTransitSagaReproduction
{
    public class Program
    {
        private const string ServiceName = "MtSagaReproHost";
        public static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<MassTransitModule>();
            builder.RegisterType<SvcHost>().AsSelf().SingleInstance();
            
            var exitCode = HostFactory.Run(cfg =>
            {
                var container = builder.Build();

                cfg.UseAutofacContainer(container);
                cfg.SetServiceName(ServiceName);
                cfg.SetDisplayName(ServiceName);
                cfg.SetDescription(ServiceName + " Topshelf host");
                cfg.RunAsLocalSystem();
                cfg.Service<SvcHost>(host =>
                {
                    host.ConstructUsingAutofacContainer();
                    host.WhenStarted(async x => await x.StartAsync());
                    host.WhenStopped(x => x.Stop());
                    host.AfterStoppingService(x => container.Dispose());
                });
            });
            return (int)exitCode;
        }
    }

    public class SvcHost
    {
        private readonly IBusControl _busControl;

        public SvcHost(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public async Task StartAsync() => await _busControl.StartAsync();

        public void Stop() => _busControl.Stop();
    }
}