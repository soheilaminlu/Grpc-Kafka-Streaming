using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grpc_client.KafkaConsumer
{
    public class KafkaConsumer
    {
        private readonly ConsumerConfig _config;
        private readonly string _topic;

        public KafkaConsumer(string bootstrapServers, string topic)
        {
            _config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "Soheil-G" + Guid.NewGuid().ToString(),
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
            };
            _topic = topic;
        }

        public async Task ConsumeAsync(Func<string, Task> messageHandler, CancellationToken cancellationToken)
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            consumer.Subscribe(_topic);

            try
            {
                while (true)
                {
                    var consumerResult = consumer.Consume(cancellationToken);
                    var message = consumerResult.Message.Value;
                    await messageHandler(message);
                    consumer.Commit(consumerResult);
                }
            }
            catch (ConsumeException e)
            {
                Console.WriteLine($"Consume error: {e.Error.Reason}");
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
