using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MessageNetwork;
public class KafkaConsumer : IDisposable, IConsumer
{
    private readonly IConsumer<Null, string> consumer;
    private readonly ILogger<KafkaConsumer> _logger;

    private bool disposedValue;

    public KafkaConsumer(string bootstrapServer, string clientId, ILogger<KafkaConsumer> logger)
    {
        var config = new ConsumerConfig()
        {
            BootstrapServers = bootstrapServer,
            ClientId = clientId,
            EnableAutoCommit = true,
            // retry settings:
            // Receive acknowledgement from all sync replicas
            Acks = Acks.Leader,
        };
        _logger = logger;
        consumer = new ConsumerBuilder<Null, string>(config)
            .SetKeyDeserializer(Deserializers.Null)
            .SetValueDeserializer(Deserializers.Utf8)
            .SetLogHandler((_, message) =>
                _logger.LogInformation("Kafka log: {facility}-{level} Message: {message}", message.Facility, message.Level, message.Message))
            .SetErrorHandler((_, e) => _logger.LogInformation("Kafka producer nonfatal error {code}: {reason}", e.Code, e.Reason))
            .Build();
    }

    public async Task ListenAsync(IEnumerable<KeyValuePair<string, Func<Task<string>>>> subscriptions)
    {
        var topics = from topic in subscriptions
                     select topic.Key;

        IDictionary<string, Func<Task<string>>> _subscriptions = new Dictionary<string, Func<Task<string>>>(subscriptions);

        consumer.Subscribe(topics);

        await Task.Factory.StartNew(() => StartConsumerLoop(_subscriptions), TaskCreationOptions.LongRunning);
    }

    public async Task ListenAsync(IEnumerable<KeyValuePair<string, Func<Task<string>>>> subscriptions, CancellationToken cancellationToken)
    {
        var topics = from topic in subscriptions
                     select topic.Key;

        IDictionary<string, Func<Task<string>>> _subscriptions = new Dictionary<string, Func<Task<string>>>(subscriptions);

        consumer.Subscribe(topics);

        await Task.Factory.StartNew(() => StartConsumerLoop(_subscriptions), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async void StartConsumerLoop(IDictionary<string, Func<Task<string>>> subscriptions)
    {
        while (true)
        {
            try
            {
                var result = consumer.Consume();

                try
                {
                    if (subscriptions.ContainsKey(result.Topic))
                        await subscriptions[result.Topic].Invoke();
                }
                catch (Exception e)
                {
                    // exceptions in de invoke should be logged but shouldn't break the listen loop
                    _logger.LogError("error {message}", e.Message);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                _logger.LogDebug("kafka error {message}", e.Message);

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                _logger.LogDebug("kafka error {message}", e.Message);
                break;
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                consumer.Close(); // Commit offsets and leave the group cleanly.
                consumer.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
