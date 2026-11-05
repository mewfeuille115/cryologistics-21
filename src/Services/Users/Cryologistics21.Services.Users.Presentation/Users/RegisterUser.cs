using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Common.Presentation.Endpoints;
using Cryologistics21.Common.Presentation.Results;
using Cryologistics21.Services.Users.Application.Users.RegisterUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cryologistics21.Services.Users.Presentation.Users;

internal sealed class RegisterUser : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("users/register", async (
			Request request,
			ICommandHandler<RegisterUserCommand, Guid> handler,
			CancellationToken cancellationToken) =>
		{
			Result<Guid> result = await handler.Handle(
				new RegisterUserCommand(
					request.Email,
					request.Password,
					request.FirstName,
					request.LastName),
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
		public string FirstName { get; init; }
		public string LastName { get; init; }
	}
}
