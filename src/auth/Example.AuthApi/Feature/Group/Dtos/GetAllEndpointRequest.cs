using FluentValidation;

namespace Example.AuthApi.Feature.Group.Dtos;

public sealed record GetAllEndpointRequest(string? SearchString, bool OnlySelf = false, bool OnlyOwned = false);

public sealed class GetAllEndpointRequestValidator : Validator<GetAllEndpointRequest>
{
    public GetAllEndpointRequestValidator()
    {
        When(p => !string.IsNullOrWhiteSpace(p.SearchString), () =>
        {
            RuleFor(p => p.SearchString).NotEmpty().WithMessage("Search String has to be set.");
        });
    }
}