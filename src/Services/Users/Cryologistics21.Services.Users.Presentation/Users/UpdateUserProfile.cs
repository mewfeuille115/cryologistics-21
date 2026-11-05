using System.Security.Claims;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Common.Infrastructure.Authentication;
using Cryologistics21.Common.Presentation.Endpoints;
using Cryologistics21.Common.Presentation.Results;
using Cryologistics21.Services.Users.Application.Users.UpdateUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cryologistics21.Services.Users.Presentation.Users;

internal sealed class UpdateUserProfile : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPut("users/profile", async (
			Request request,
			ClaimsPrincipal claims,
			ICommandHandler<UpdateUserCommand> handler,
			CancellationToken cancellationToken) =>
		{
			Result result = await handler.Handle(
				new UpdateUserCommand(
					claims.GetUserId(),
					request.FirstName,
					request.LastName),
				cancellationToken);

			return result.Match(Results.NoContent, ApiResults.Problem);
		})
		.RequireAuthorization(Permissions.ModifyUser)
		.WithTags(Tags.Users);
	}

	internal sealed class Request
	{
		public string FirstName { get; init; }
		public string LastName { get; init; }
	}
}
