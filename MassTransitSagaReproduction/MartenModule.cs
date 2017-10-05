using Autofac;
using Marten;

namespace MassTransitSagaReproduction
{
    public class MartenModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(DocumentStore.For(x =>
                    x.Connection("host=localhost;port=5432;database=marten;username=postgres;password=postgres")
                ))
                .As<IDocumentStore>()
                .SingleInstance();
        }
    }
}