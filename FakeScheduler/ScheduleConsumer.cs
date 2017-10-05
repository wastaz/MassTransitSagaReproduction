using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common;
using MassTransit;
using MassTransit.Scheduling;
using MassTransit.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FakeScheduler
{
    public class ScheduleConsumer : IConsumer<ScheduleMessage> {
        public async Task Consume(ConsumeContext<ScheduleMessage> context)
        {
            var msg = context.Message;
            string bodyText;
            using (var ms = new MemoryStream())
            using(var bs = context.ReceiveContext.GetBody()) 
            {
                await bs.CopyToAsync(ms);
                bodyText = Encoding.UTF8.GetString(ms.ToArray());
            }

            var envelope = JObject.Parse(bodyText);
            ScheduledTransitionMessage parsedMsg;
            using (var sr = new StringReader(envelope["message"]["payload"].ToString()))
            using (var jtr = new JsonTextReader(sr))
            {
                parsedMsg = JsonMessageSerializer.Deserializer.Deserialize<ScheduledTransitionMessage>(jtr);
                
            }

            var endpoint = await context.GetSendEndpoint(msg.Destination);
            await endpoint.Send(parsedMsg);

            Console.WriteLine($"Immediately sent message: {parsedMsg.Id}");
        }
    }
}