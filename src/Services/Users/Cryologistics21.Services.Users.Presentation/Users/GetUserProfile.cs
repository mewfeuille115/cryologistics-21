using System.Security.Claims;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Common.Infrastructure.Authentication;
using Cryologistics21.Common.Presentation.Endpoints;
using Cryologistics21.Common.Presentation.Results;
using Cryologistics21.Services.Users.Application.Users.GetUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cryologistics21.Services.Users.Presentation.Users;

internal sealed class GetUserProfile : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("users/profile", async (
			ClaimsPrincipal claims,
			IQueryHandler<GetUserQuery, UserResponse> handler,
			CancellationToken cancellationToken) =>
		{
			Result<UserResponse> result = await handler.Handle(
				new GetUserQuery(claims.GetUserId()),
				cancellationToken);

			return result.Match(Results.Ok, ApiResults.Problem);
		})
		.RequireAuthorization(Permissions.GetUser)
		.WithTags(Tags.Users);
	}
}
