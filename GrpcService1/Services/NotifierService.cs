
using Google.Protobuf.WellKnownTypes;
using DuplexStreaming;
using Grpc.Core;
namespace GrpcService1.Services;
public class NotifierService : Notifier.NotifierBase
{
    private readonly ILogger<NotifierService> _logger;
    public NotifierService(ILogger<NotifierService> logger)
    {
        _logger = logger;
    }

    public override async Task ChatNotification(IAsyncStreamReader<NotificationRequest>
      requestStream, IServerStreamWriter<NotificationReply> responseStream,
      ServerCallContext context)
    {

        while (await requestStream.MoveNext())
        {
            var request = requestStream.Current;

            var now = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow);
            var reply = new NotificationReply()
            {
                Message = $"{request.Message}",
                ReceivedAt = now
            };

            await responseStream.WriteAsync(reply);
        }
    }
}



//var config = new ConsumerConfig
//            {
//                BootstrapServers = "localhost:9092",
//                GroupId = "soheil-group",
//                AutoOffsetReset = AutoOffsetReset.Earliest
//            };
//            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
//            {
//                consumer.Subscribe("soheil");
//                try
//                {
//                    while (await requestStream.MoveNext(CancellationToken.None))
//                    {
//                        var consumeResult = consumer.Consume(CancellationToken.None);

//                        if (consumeResult != null && consumeResult.Message != null && !string.IsNullOrEmpty(consumeResult.Message.Value))
//                        {

//                            var message = consumeResult.Message.Value;
//                            _logger.LogInformation($"Received message: {message}");

//                            var now = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow);
//                            var reply = new NotificationReply
//                            {
//                                Message = $"message from Kafka {message}",
//                                ReceivedAt = now
//                            };
//                            await responseStream.WriteAsync(reply);
//                        }
//                        else
//                        {
//                            _logger.LogWarning("Received empty or null message from Kafka.");
//                        }
//                    }
//                }       
//                catch (OperationCanceledException ex)
//                {
//                    _logger.LogError(ex, "Consumer operation canceled.");
//                } catch(Exception ex)
//                {
//                    _logger.LogInformation(ex, "Error to consuming Data");
//                } finally
//                {
//                    consumer.Close();
//                }
//            }

//        }
//    }
//}