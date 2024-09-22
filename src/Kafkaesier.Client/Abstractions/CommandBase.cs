namespace Kafkaesier.Client.Abstractions;

public abstract class CommandBase
{
    public virtual TopicConfiguration GetConfiguration()
    {
        return new TopicConfiguration();
    }
}