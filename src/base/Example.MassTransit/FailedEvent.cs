namespace Example.MassTransit;

public abstract class FailedEvent
{
    public Guid Id { get; set; }
    public string Reason { get; set; } = default!;
}