using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Services.Users.Application.Abstractions.Data;
using Cryologistics21.Services.Users.Application.Abstractions.Identity;
using Cryologistics21.Services.Users.Domain.Users;

namespace Cryologistics21.Services.Users.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler(
		IIdentityProviderService identityProviderService,
		IUserRepository userRepository,
		IUnitOfWork unitOfWork
	) : ICommandHandler<RegisterUserCommand, Guid>
{
	public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
	{
		Result<string> result = await identityProviderService
			.RegisterUserAsync(
				new UserModel(
					request.Email,
					request.Password,
					request.FirstName,
					request.LastName
				),
				cancellationToken)
			;

		if (result.IsFailure)
		{
			return Result.Failure<Guid>(result.Error);
		}

		var user = User.Create(request.Email, request.FirstName, request.LastName, result.Value);

		userRepository.Insert(user);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success(user.Id);
	}
}
