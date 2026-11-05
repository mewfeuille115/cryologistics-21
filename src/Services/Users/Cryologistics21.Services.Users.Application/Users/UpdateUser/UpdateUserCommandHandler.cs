using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Services.Users.Application.Abstractions.Data;
using Cryologistics21.Services.Users.Domain.Users;

namespace Cryologistics21.Services.Users.Application.Users.UpdateUser;

internal sealed class UpdateUserCommandHandler(
		IUserRepository userRepository,
		IUnitOfWork unitOfWork
	) : ICommandHandler<UpdateUserCommand>
{
	public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
	{
		User? user = await userRepository.GetAsync(request.UserId, cancellationToken);

		if (user is null)
		{
			return Result.Failure(UserErrors.NotFound(request.UserId));
		}

		user.Update(request.FirstName, request.LastName);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}
