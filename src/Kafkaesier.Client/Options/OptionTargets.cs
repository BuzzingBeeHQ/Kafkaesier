namespace Kafkaesier.Client.Options;

[Flags]
internal enum OptionTargets
{
    AdminClient        = 0b001,
    Consumer           = 0b010,
    Producer           = 0b011,
    TopicConfiguration = 0b100
}