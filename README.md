# MassTransit saga state saving reproduction repo

## Getting started

Run the following to restore packages:

```
.paket/paket.bootstrapper.exe
.paket/paket.exe restore
```

By default I am using Marten for saga storage, but I have also tested this on MongoDB and
observed the same behaviour. (Switch providers in `MassTransitModule` in the `MassTransitSagaReproduction` project.)

For Marten, by default it attempts to connect to `localhost` where there should be a postgres database named `marten` with the default `postgres/postgres` user.
For Mongo, by default it attempts to connect to `localhost` with no credentials and use the database `sagastorage`.

This repo also assumes a RabbitMq running on localhost with `guest/guest` as the username/password. 

## Reproducing

The solution consists of three console programs (and a common project).

`MassTransitSagaReproduction` hosts `TestProcessManager` which is the saga that
we are trying to break.

`FakeScheduler` is a simple program that listens to `MassTransit.Scheduling.ScheduleMessage` and 
instantly returns the payload back to the destination without caring about any timings.

`SendSomeMessages` is a simple console app which will publish ten messages to start
 the saga (and then let you keep publishing ten messages at a time).

Run all three programs at once (I am running them in debug mode), and keep hitting `Y` in the `SendSomeMessages` until
you start seeing `R-FAULT` in the saga console. The fault looks like this

```
R-FAULT rabbitmq://localhost/mtsagarepro_saga N/A Common.ScheduledTransitionMessage MassTransitSagaReproduction.TestState(00:00:00.0506110) The ScheduledStateChange.AnyReceived event is not handled during the Initial state for the TestProcessManager state machine
MOVE rabbitmq://localhost/mtsagarepro_saga N/A rabbitmq://localhost/mtsagarepro_saga_error?bind=true&queue=mtsagarepro_saga_error Fault: The ScheduledStateChange.AnyReceived event is not handled during the Initial state for the TestProcessManager state machine
```


