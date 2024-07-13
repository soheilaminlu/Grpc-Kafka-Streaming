using DuplexStreaming;
using Grpc.Net.Client;
using Confluent.Kafka;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
           using var channel = GrpcChannel.ForAddress("http://localhost:5286");
           var client = new Notifier.NotifierClient(channel);
            using var call = client.ChatNotification();
            var responseReaderTask = Task.Run(async Task () =>
            {
                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var note = call.ResponseStream.Current;
                    Console.WriteLine($"recieved kafka message {note.Message} created At {note.ReceivedAt}");

                }
                var config = new ConsumerConfig
                {
                    BootstrapServers = "localhost:9092",
                    GroupId = "consumer-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

            });
       

        }
    }
}

//    using var call = client.ChatNotification();
//    var responseReaderTask = Task.Run(async Task () =>
//    {
//        while (await call.ResponseStream.MoveNext(CancellationToken.None))
//        {
//            var note = call.ResponseStream.Current;
//            Console.WriteLine($"{note.Message}, received at {note.ReceivedAt}");
//        }
//    });
//    foreach (var msg in new[] {"tom" , "jones"})
//    {
//        var request = new NotificationRequest
//        {
//             Message = $"Hello {msg}", From = "Mom", To = msg
//        };
//        await call.RequestStream.WriteAsync(request);
//    }
//    await call.RequestStream.CompleteAsync();
//    await responseReaderTask;


//    Console.WriteLine("Press any key to exit...");
//    Console.ReadKey();