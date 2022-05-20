namespace MessageNetwork;

public interface IProducer
{
    void SendMessage<T>(string queueName, T messageObject);
    Task SendMessageAsync<T>(string queueName, T messageObject);
}