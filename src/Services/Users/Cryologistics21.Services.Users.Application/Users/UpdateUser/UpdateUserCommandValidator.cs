using FluentValidation;

namespace Cryologistics21.Services.Users.Application.Users.UpdateUser;

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
	public UpdateUserCommandValidator()
	{
		RuleFor(c => c.UserId).NotEmpty();
		RuleFor(c => c.FirstName).NotEmpty();
		RuleFor(c => c.LastName).NotEmpty();
	}
}
