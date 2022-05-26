namespace MessageNetwork;

public interface IConsumer
{
    Task ListenAsync(IEnumerable<KeyValuePair<string, Func<Task<string>>>> subscriptions);
    Task ListenAsync(IEnumerable<KeyValuePair<string, Func<Task<string>>>> subscriptions, CancellationToken cancellationToken);
}