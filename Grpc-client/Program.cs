using DuplexStreaming;
using Confluent.Kafka;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5286");
            var client = new Notifier.NotifierClient(channel);
            using var call = client.ChatNotification();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var bootstrapServer = config["kafka:BootstrapServers"];
            var topic = config["kafka:Topic"];

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServer,
                GroupId = "Soheil-G",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe(topic);

            try
            {
                while (true)
                {
                    var consumerResult = consumer.Consume(CancellationToken.None);
                    var message = consumerResult.Message.Value;
                    var responseReaderTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext(CancellationToken.None))
                        {
                            var note = call.ResponseStream.Current;
                            Console.WriteLine($"{note.Message}, received at {note.ReceivedAt}");
                        }
                    });
                    foreach (var msg in new[] { "ali", "reza" })
                    {
                        var request = new NotificationRequest
                        {
                            Message = message,
                            From = "kafka",
                            To = message

                        };
                        await call.RequestStream.WriteAsync(request);
                    }
                    await call.RequestStream.CompleteAsync();
                    await responseReaderTask;
                    Console.WriteLine("press to con..........");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

            //        using var channel = GrpcChannel.ForAddress("http://localhost:5286");
            //        var client = new Notifier.NotifierClient(channel);
            //        using var call = client.ChatNotification();

            //        var responseReaderTask = Task.Run(async () =>
            //        {
            //            while (await call.ResponseStream.MoveNext(CancellationToken.None))
            //            {
            //                var note = call.ResponseStream.Current;
            //                Console.WriteLine($"{note.Message}, received at {note.ReceivedAt}");
            //            }
            //        });

            //        foreach (var msg in new[] { "Tom", "Jones" })
            //        {
            //            var request = new NotificationRequest
            //            { Message = $"Hello {msg}", From = "Mom", To = msg };
            //            await call.RequestStream.WriteAsync(request);
            //        }

            //        await call.RequestStream.CompleteAsync();
            //        await responseReaderTask;

            //        Console.WriteLine("Press any key to exit...");
            //        Console.ReadKey();
            //    }
            //}
        

//using DuplexStreaming;
//using Grpc.Net.Client;

//// The port number must match the port of the gRPC server.
//using var channel = GrpcChannel.ForAddress("http://localhost:5295");
//var client = new Notifier.NotifierClient(channel);
//using var call = client.ChatNotification();

//var responseReaderTask = Task.Run(async Task () =>
//{
//    while (await call.ResponseStream.MoveNext(CancellationToken.None))
//    {
//        var note = call.ResponseStream.Current;
//        Console.WriteLine($"{note.Message}, received at {note.ReceivedAt}");
//    }
//});

//foreach (var msg in new[] { "Tom", "Jones" })
//{
//    var request = new NotificationsRequest()
//    { Message = $"Hello {msg}", From = "Mom", To = msg };
//    await call.RequestStream.WriteAsync(request);
//}

//await call.RequestStream.CompleteAsync();
//await responseReaderTask;

//Console.WriteLine("Press any key to exit...");
//Console.ReadKey();