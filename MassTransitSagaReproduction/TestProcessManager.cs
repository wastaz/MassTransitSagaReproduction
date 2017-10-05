using System;
using Automatonymous;
using Common;
using MassTransit.MongoDbIntegration.Saga;

namespace MassTransitSagaReproduction
{
    public class ScheduledTransitionMessageEvent : ScheduledTransitionMessage
    {
        public Guid Id { get; set; }
        
    }
    
    public class TestState : SagaStateMachineInstance, IVersionedSaga
    {
        public Guid Id => CorrelationId;
        public Guid CorrelationId { get; set; }
        public int CurrentState { get; set; }
        public Guid? SchedulerId { get; set; }
        public int Version { get; set; }
    }
    
    public class TestProcessManager : MassTransitStateMachine<TestState>
    {
        public State WaitingForScheduled { get; private set; }
        public State SecondState { get; private set; }
        
        public Event<FirstMessage> StartMachine { get; private set; }
        
        public Schedule<TestState, ScheduledTransitionMessage> ScheduledStateChange { get; private set; }
        
        public TestProcessManager()
        {
            DefineState();
            DefineEvents();
            DefineSchedules();
            
            Initially(
                When(StartMachine)
                    .TransitionTo(WaitingForScheduled)
                    .Schedule(
                        ScheduledStateChange,
                        ctx => new ScheduledTransitionMessageEvent { Id = ctx.Instance.CorrelationId },
                        ctx => TimeSpan.FromSeconds(0)) // The fake scheduler will ignore this and just send it back instantly anyway.
                    );
            
            During(
                WaitingForScheduled,
                When(ScheduledStateChange.Received)
                    .Then(ctx => Console.WriteLine(ctx.Data.Id))
                    .Finalize()
                );
        }
        
        private void DefineState()
        {
            InstanceState(
                x => x.CurrentState, 
                WaitingForScheduled, 
                SecondState
            );
        }
        
        private void DefineEvents()
        {
            Event(() => StartMachine, cfg =>
            {
                cfg.CorrelateById(x => x.Message.Id);
                cfg.SetSagaFactory(x => new TestState
                {
                    CorrelationId = x.Message.Id,
                });
                cfg.InsertOnInitial = true;
            });
        }
        
        private void DefineSchedules()
        {
            Schedule(
                () => ScheduledStateChange,
                x => x.SchedulerId,
                x =>
                {
                    x.Received = e => e.CorrelateById(ctx => ctx.Message.Id);
                });
        }
    }
}