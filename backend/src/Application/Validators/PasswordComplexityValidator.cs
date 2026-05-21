using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class PasswordComplexityValidator : AbstractValidator<string>
{
    public PasswordComplexityValidator()
    {
        RuleFor(password => password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must include at least 8 characters, uppercase, lowercase, digit, and special character.");
    }
}

public sealed class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Password)
            .SetValidator(new PasswordComplexityValidator());
    }
}

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.NewPassword)
            .SetValidator(new PasswordComplexityValidator());

        RuleFor(command => command.VerificationCode)
            .NotEmpty()
            .Matches("^[0-9]{6}$")
            .WithMessage("Verification code must be a 6-digit number.");
    }
}

public sealed class RequestPasswordResetCodeCommandValidator : AbstractValidator<RequestPasswordResetCodeCommand>
{
    public RequestPasswordResetCodeCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(c => c.Password)
            .SetValidator(new PasswordComplexityValidator());
    }
}

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
