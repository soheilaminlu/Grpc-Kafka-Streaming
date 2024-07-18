using DuplexStreaming2;
using Grpc.Core;

namespace GrpcService2.Services
{
    public class NotificationService2 : Notifier2.Notifier2Base
    {
        private readonly ILogger<NotificationService2> _logger;
        public NotificationService2(ILogger<NotificationService2> logger)
        {
            _logger = logger;
        }

        public override async Task SendNotification(IAsyncStreamReader<RequestMessage> requestStream, IServerStreamWriter<ResponseMessage> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
            var request = requestStream.Current;
                var reply = new ResponseMessage
                {
                    Message = $"message recieved from Service 1 {request.Message}",
                };
                await responseStream.WriteAsync(reply);
            }
        }
    }
}
