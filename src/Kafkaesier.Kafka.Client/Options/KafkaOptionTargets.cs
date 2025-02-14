namespace Kafkaesier.Kafka.Client.Options;

[Flags]
public enum KafkaOptionTargets
{
    AdminClient        = 0b001,
    Consumer           = 0b010,
    Producer           = 0b011,
    TopicConfiguration = 0b100
}