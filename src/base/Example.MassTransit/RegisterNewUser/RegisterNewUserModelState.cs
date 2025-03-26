using MassTransit;

using System.ComponentModel.DataAnnotations;

namespace Example.MassTransit.RegisterNewUser;

public sealed class RegisterNewUserModelState : SagaStateMachineInstance
{
    public Guid Id { get; set; }
    public Guid CorrelationId { get; set; }

    [StringLength(128)]
    public required string CurrentState { get; set; }
    [StringLength(320)]
    public required string Email { get; set; }
    [StringLength(32)]
    public required string EmailConfirmHash { get; set; }
    [StringLength(128)]
    public required string EmailTemplate { get; set; }
    [StringLength(128)]
    public required string EmailSubject { get; set; }
}
