using AwesomeAssertions;
using Bogus;
using Cryologistics21.Common.Domain;
using Cryologistics21.Services.Users.Application.Users.LoginUser;
using Cryologistics21.Services.Users.Application.Users.RegisterUser;
using Cryologistics21.Services.Users.Domain.Users;
using Cryologistics21.Services.Users.IntegrationTests.Abstractions;

namespace Cryologistics21.Services.Users.IntegrationTests.Users;

public class LoginUserTests : BaseIntegrationTest
{
	public LoginUserTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	public static TheoryData<string, string> InvalidCommands()
	{
		return new TheoryData<string, string>
		{
			{ string.Empty, string.Empty },
			{ string.Empty, "Password123!" },
			{ "test@email.com", string.Empty },
			{ "test@email.com", "Password123!" },
		};
	}

	[Fact]
	public async Task Should_ReturnSuccess_WhenUserExistsAsync()
	{
		// Arrange
		string email = Faker.Internet.Email();
		string password = Faker.Internet.Password();
		var registerUserCommand = new RegisterUserCommand(
			email,
			password,
			Faker.Name.FirstName(),
			Faker.Name.LastName());
		await SendCommandAsync<RegisterUserCommand, Guid>(registerUserCommand);
		var loginUserCommand = new LoginUserCommand(email, password);

		// Act
		Result<LoginUserResponse> result = await SendCommandAsync<LoginUserCommand, LoginUserResponse>(loginUserCommand);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.AccessToken.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task Should_ReturnError_WhenPasswordIsIncorrectAsync()
	{
		// Arrange
		string email = Faker.Internet.Email();
		string password = Faker.Internet.Password();
		await SendCommandAsync<RegisterUserCommand, Guid>(
			new RegisterUserCommand(email, password, Faker.Name.FirstName(), Faker.Name.LastName()));
		var loginUserCommand = new LoginUserCommand(email, $"wrong-{password}");

		// Act
		Result<LoginUserResponse> result = await SendCommandAsync<LoginUserCommand, LoginUserResponse>(loginUserCommand);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(UserErrors.InvalidEmailOrPassword(email));
	}

	[Fact]
	public async Task Should_ReturnError_WhenUserDoesNotExistAsync()
	{
		// Arrange
		string email = Faker.Internet.Email();
		var loginUserCommand = new LoginUserCommand(email, Faker.Internet.Password());

		// Act
		Result<LoginUserResponse> result = await SendCommandAsync<LoginUserCommand, LoginUserResponse>(loginUserCommand);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(UserErrors.InvalidEmailOrPassword(email));
	}

	[Theory]
	[MemberData(nameof(InvalidCommands))]
	public async Task Should_ReturnError_WhenCommandIsNotValidAsync(string email, string password)
	{
		// Act
		var command = new LoginUserCommand(email.ToString(), password);
		Result<LoginUserResponse> result = await SendCommandAsync<LoginUserCommand, LoginUserResponse>(command);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(UserErrors.InvalidEmailOrPassword(email.ToString()));
	}
}
