namespace Example.MassTransit.SendEmail;

public record AbstractMailAddressModel
{
    public required Guid Id { get; set; }
    public required string Subject { get; set; }
    public required string Template { get; set; }
    public required string SendTo { get; set; }
    public required IDictionary<string, string>  Context { get; set; }
}
