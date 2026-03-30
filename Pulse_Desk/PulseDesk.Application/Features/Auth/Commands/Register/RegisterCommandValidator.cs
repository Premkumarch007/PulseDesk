using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator:AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator() {

            RuleFor(x => x.request.FirstName)
                .NotEmpty().WithMessage("FirstName is required")
                .MaximumLength(100).WithMessage("FirstName should not exceed 100 characters");

            RuleFor(x => x.request.LastName)
                .NotEmpty().WithMessage("LastName is required")
                .MaximumLength(100).WithMessage("LastName should not exceed 100 characters");

            RuleFor(x => x.request.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format").MaximumLength(255);

            RuleFor(x => x.request.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number");

            RuleFor(x => x.request.Department)
                .MaximumLength(100)
                .When(x => x.request.Department != null);
        }
    }
}
