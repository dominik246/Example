namespace Example.MassTransit.SendEmail;

public abstract record SendEmailFailed
{
    public Guid Id { get; set; }
    public string Reason { get; set; } = default!;
}