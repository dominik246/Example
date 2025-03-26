using MassTransit;

using System.ComponentModel.DataAnnotations;

namespace Example.MassTransit.PasswordRecovery;

public sealed class PasswordRecoveryState : SagaStateMachineInstance
{
    public Guid Id { get; set; }
    public Guid CorrelationId { get; set; }
    public required Guid UserId { get; set; }

    [StringLength(64)]
    public required string CurrentState { get; set; }
    [StringLength(128)]
    public required string Hash { get; set; }
    [StringLength(320)]
    public required string Email { get; set; }
    [StringLength(128)]
    public required string EmailTemplate { get; set; }
    [StringLength(128)]
    public required string EmailSubject { get; set; }
}
