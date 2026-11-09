using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Bogus;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Services.Users.Infrastructure.Database;
using Cryologistics21.Services.Users.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cryologistics21.Services.Users.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public class BaseIntegrationTest : IDisposable
{
	protected static readonly Faker Faker = new();
	private readonly IServiceScope _scope;
	protected readonly HttpClient HttpClient;
	private readonly KeyCloakOptions _options;
	protected readonly UsersDbContext DbContext;

	protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
	{
		_scope = factory.Services.CreateScope();
		HttpClient = factory.CreateClient();
		_options = _scope.ServiceProvider.GetRequiredService<IOptions<KeyCloakOptions>>().Value;
		DbContext = _scope.ServiceProvider.GetRequiredService<UsersDbContext>();
	}

	protected async Task<Result<TResult>> SendCommandAsync<TCommand, TResult>(TCommand command)
		where TCommand : ICommand<TResult>
	{
		ICommandHandler<TCommand, TResult> handler = _scope.ServiceProvider
			.GetRequiredService<ICommandHandler<TCommand, TResult>>();

		return await handler.Handle(command, CancellationToken.None);
	}

	public async Task<Result> SendCommandAsync<TCommand>(TCommand command)
		where TCommand : ICommand
	{
		ICommandHandler<TCommand> handler = _scope.ServiceProvider
			.GetRequiredService<ICommandHandler<TCommand>>();

		return await handler.Handle(command, CancellationToken.None);
	}

	protected async Task<Result<TResult>> SendQueryAsync<TQuery, TResult>(TQuery query)
		where TQuery : IQuery<TResult>
	{
		IQueryHandler<TQuery, TResult> handler = _scope.ServiceProvider
			.GetRequiredService<IQueryHandler<TQuery, TResult>>();

		return await handler.Handle(query, CancellationToken.None);
	}

	protected async Task<string> GetAccessTokenAsync(string email, string password)
	{
		using var client = new HttpClient();

		var authRequestParameters = new KeyValuePair<string, string>[]
		{
			new("client_id", _options.PublicClientId),
			new("scope", "openid"),
			new("grant_type", "password"),
			new("username", email),
			new("password", password)
		};

		using var authRequestContent = new FormUrlEncodedContent(authRequestParameters);

		using var authRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_options.TokenUrl));
		authRequest.Content = authRequestContent;

		using HttpResponseMessage authorizationResponse = await client.SendAsync(authRequest);

		authorizationResponse.EnsureSuccessStatusCode();

		AuthToken authToken = await authorizationResponse.Content.ReadFromJsonAsync<AuthToken>();

		return authToken!.AccessToken;
	}

	internal sealed class AuthToken
	{
		[JsonPropertyName("access_token")]
		public string AccessToken { get; init; }
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_scope.Dispose();
		}
	}
}
