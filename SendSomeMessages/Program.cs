using System;
using Common;
using MassTransit;

namespace SendSomeMessages
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting bus...");

            var bus = Bus.Factory.CreateUsingRabbitMq(rc =>
            {
                var host = rc.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });

            bus.Start();

            Console.WriteLine("<<Press ENTER to start...>>");
            Console.ReadLine();
            while (true)
            {
                for (int i = 0; i < 10; ++i)
                {
                    var id = Guid.NewGuid();
                    bus.Publish(new FirstMessageMsg { Id = id });
                    Console.WriteLine($"Published: {id}");
                }

                if (!ShouldContinue())
                {
                    break;
                }
            }
            Console.WriteLine("<<Press ENTER to quit...>>");
            Console.ReadLine();
            
            bus.Stop();
        }

        private static bool ShouldContinue()
        {
            while(true) {
                Console.WriteLine("Send 10 more? (y/n) ");
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Y:
                        return true;
                    case ConsoleKey.N:
                        return false;
                }
            }
        }
    }

    public class FirstMessageMsg : FirstMessage
    {
        public Guid Id { get; set; }
    }
}