using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Common.Presentation.Endpoints;
using Cryologistics21.Common.Presentation.Results;
using Cryologistics21.Services.Users.Application.Users.LoginUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cryologistics21.Services.Users.Presentation.Users;

internal sealed class LoginUser : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("users/login", async (
			Request request,
			ICommandHandler<LoginUserCommand, LoginUserResponse> handler,
			CancellationToken cancellationToken) =>
		{
			Result<LoginUserResponse> result = await handler.Handle(
				new LoginUserCommand(
					request.Email,
					request.Password),
				cancellationToken);
			return result.Match(Results.Ok, ApiResults.Problem);
		})
		.AllowAnonymous()
		.WithTags(Tags.Users);
	}

	internal sealed class Request
	{
		public string Email { get; init; }
		public string Password { get; init; }
	}
}
