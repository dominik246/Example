using FluentValidation;

namespace Example.AuthApi.Feature.Group.Dtos;

public sealed record DeleteEndpointRequest
{
    [QueryParam]
    public required Guid Id { get; init; }
}

public sealed class DeleteEndpointRequestValidator : Validator<DeleteEndpointRequest>
{
    public DeleteEndpointRequestValidator()
    {
        RuleFor(p => p.Id).NotEmpty().WithMessage("Id has to be set.");
    }
}