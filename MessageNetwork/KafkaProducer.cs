using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MessageNetwork;
public class KafkaProducer : IProducer
{
    private readonly ProducerConfig config;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(string bootstrapServer, string clientId, int? messageSendMaxRetries, ILogger<KafkaProducer> logger)
    {
        config = new ProducerConfig()
        {
            BootstrapServers = bootstrapServer,
            EnableDeliveryReports = true,
            ClientId = clientId,

            // retry settings:
            // Receive acknowledgement from all sync replicas
            Acks = Acks.Leader,
            // Number of times to retry before giving up
            MessageSendMaxRetries = messageSendMaxRetries ?? 3,
            // Duration to retry before next attempt
            RetryBackoffMs = 1000,
            // Set to true if you don't want to reorder messages on retry
            EnableIdempotence = true
        };
        _logger = logger;
    }

    private IProducer<Null, string> CreateProducer()
    {
        return new ProducerBuilder<Null, string>(config)
            .SetKeySerializer(Serializers.Null)
            .SetValueSerializer(Serializers.Utf8)
            .SetLogHandler((_, message) =>
                _logger.LogInformation("Kafka log: {facility}-{level} Message: {message}", message.Facility, message.Level, message.Message))
            .SetErrorHandler(
                (_, e) => {
                    if (e.IsFatal)
                        _logger.LogError("Kafka producer fatal error {code}: {reason}", e.Code, e.Reason);
                    else
                        _logger.LogInformation("Kafka producer nonfatal error {code}: {reason}", e.Code, e.Reason);
                }
            )
            .Build();
    }

    public async Task SendMessageAsync<T>(string queueName, T messageObject)
    {
        using var producer = CreateProducer();
        string json = JsonConvert.SerializeObject(messageObject);

        await producer.ProduceAsync(queueName, new Message<Null, string>() { Value = json });
    }

    public void SendMessage<T>(string queueName, T messageObject)
    {
        using var producer = CreateProducer();
        string json = JsonConvert.SerializeObject(messageObject);

        producer.Produce(queueName, new Message<Null, string>() { Value = json });
    }
}
