
using Google.Protobuf.WellKnownTypes;
using Confluent.Kafka;
using DuplexStreaming;
using Grpc.Core;
using System.ComponentModel;
using System.Diagnostics.Metrics;



namespace GrpcService1.Services
{
    public class NotifierService : Notifier.NotifierBase
    {
        public override async Task ChatNotification(IAsyncStreamReader<NotificationRequest> requestStream, IServerStreamWriter<NotificationReply> responseStream, ServerCallContext context)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe("data-sender");
                try
                {
                    while (true)
                    {
                        var consumeResult = consumer.Consume(CancellationToken.None);
                        var message = consumeResult.Message.Value;
                        Console.WriteLine($"Kafka Message is: {message}");
                        var request = new NotificationRequest
                        {
                            Message = message ,
                            From = "Kafka" ,
                            To = "client"
                        
                        }; 

                        var now = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow);
                        var reply = new NotificationReply
                        {
                            Message = $"message from Kafka {message}",
                            ReceivedAt = now
                        };
                        await responseStream.WriteAsync(reply);
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }


        }
    }
}


//while (await requestStream.MoveNext())
//{
//    var request = requestStream.Current;
//    var now = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow);
//    var reply = new NotificationReply
//    {
//        Message = $"Hi {request.From} you send message for {request.To}",
//        ReceivedAt = now
//    };
//    await responseStream.WriteAsync(reply);

//}