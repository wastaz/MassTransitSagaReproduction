using System;
using Autofac;
using Automatonymous;
using Marten;
using MassTransit;
using MassTransit.MartenIntegration;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Driver;

namespace MassTransitSagaReproduction
{
    public class MassTransitModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<TestProcessManager>()
                .AsSelf()
                .As<MassTransitStateMachine<TestState>>()
                .SingleInstance();
            
            builder.RegisterInstance(DocumentStore.For(x =>
                    x.Connection("host=localhost;port=5432;database=marten;username=postgres;password=postgres")
                ))
                .As<IDocumentStore>()
                .SingleInstance();
            
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

                            
                            
                            var saga =
                                context.Resolve<MassTransitStateMachine<TestState>>();
                            var store = context.Resolve<IDocumentStore>();
                            //var repo = new MongoDbSagaRepository<TestState>("mongodb://127.0.0.1", "sagastorage");
                            var repo = new MartenSagaRepository<TestState>(store);

                            
                            rc.ReceiveEndpoint(host, "mtsagarepro_saga", cfg =>
                            {
                                cfg.StateMachineSaga(saga, repo);
                                cfg.UseInMemoryOutbox();
                            });

                            rc.UseMessageScheduler(new Uri("rabbitmq://localhost/scheduler"));
                        });
                    })
                .SingleInstance()
                .As<IBusControl>()
                .As<IBus>();
        }
    }


}