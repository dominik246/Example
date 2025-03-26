namespace Example.MassTransit.SendEmail;

public abstract record SendEmailCompleted
{
    public Guid Id { get; set; }
}
