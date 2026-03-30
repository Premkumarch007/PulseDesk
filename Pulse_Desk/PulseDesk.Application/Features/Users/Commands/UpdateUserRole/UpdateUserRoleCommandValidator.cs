using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Commands.UpdateUserRole
{
    public class UpdateUserRoleCommandValidator:AbstractValidator<UpdateUserRoleCommand>
    {
        private static readonly string[] ValidRoles = { "Admin", "User", "Manager", "Agent" };
        public UpdateUserRoleCommandValidator()
        {
            RuleFor(x => x.UserID).NotEmpty().WithMessage("UserId is required").GreaterThan(0).WithMessage("Invalid User Id");
            RuleFor(x => x.RoleName).NotEmpty().WithMessage("RoleName is required")
                .Must(r => ValidRoles.Contains(r)).WithMessage($"Role must be one of : {string.Join(", ", ValidRoles)}");
        }
    }
}
