using FluentValidation;

namespace Example.AuthApi.Feature.Group.Dtos;

public sealed record CreateGroupRequest(string Name, bool IsAdminGroup, bool ShouldAutoAddSelf, List<Guid> UserIds);

public sealed class CreateGroupRequestValidator : Validator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Name has to be provided.")
            .MaximumLength(50).WithMessage("Name has to be less than {MaxLength}");
    }
}