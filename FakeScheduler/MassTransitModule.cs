using System;
using Autofac;
using MassTransit;

namespace FakeScheduler
{
    public class MassTransitModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(
                    context =>
                    {
                        return Bus.Factory.CreateUsingRabbitMq(rc =>
                        {
                            var host = rc.Host(new Uri("rabbitmq://localhost/"), h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                            rc.ReceiveEndpoint(host, "scheduler", cfg =>
                            {
                                cfg.LoadFrom(context);
                            });
                        });
                    })
                .SingleInstance()
                .As<IBusControl>()
                .As<IBus>();
            
            builder.RegisterConsumers(ThisAssembly);
        }
    }
}