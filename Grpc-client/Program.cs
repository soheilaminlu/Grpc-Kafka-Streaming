using DuplexStreaming;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc_client.KafkaConsumer;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // ایجاد کانال gRPC
            using var channel = GrpcChannel.ForAddress("http://localhost:5286");
            var client = new Notifier.NotifierClient(channel);
            using var call = client.ChatNotification();

            // خواندن تنظیمات از فایل appsettings.json
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var bootstrapServer = config["kafka:BootstrapServers"];
            var topic = config["kafka:Topic"];

            // ایجاد یک نمونه از KafkaConsumer
            var kafkaConsumer = new KafkaConsumer(bootstrapServer, topic);

            // خواندن پاسخ‌های gRPC
            var responseReaderTask = Task.Run(async () =>
            {
                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var note = call.ResponseStream.Current;
                    Console.WriteLine($"{note.Message}, received at {note.ReceivedAt}");
                }
            });

            // مصرف داده‌ها از Kafka و ارسال آن‌ها به NotifierService
            await kafkaConsumer.ConsumeAsync(async (message) =>
            {
                var request = new NotificationRequest
                {
                    Message = message
                };
                await call.RequestStream.WriteAsync(request);
            }, CancellationToken.None);

            await call.RequestStream.CompleteAsync();
            await responseReaderTask;

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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