using AwesomeAssertions;
using Cryologistics21.Services.Users.Domain.Users;
using Cryologistics21.Services.Users.UnitTests.Abstractions;

namespace Cryologistics21.Services.Users.UnitTests.Users;

public class UserTests : BaseTest
{
	[Fact]
	public void Create_ShouldReturnUser()
	{
		// Act
		var user = User.Create(
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName(),
			Guid.NewGuid().ToString());

		// Assert
		user.Should().NotBeNull();
	}

	[Fact]
	public void Create_ShouldReturnUser_WithMemberRole()
	{
		// Act
		var user = User.Create(
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName(),
			Guid.NewGuid().ToString());

		// Assert
		user.Roles.Single().Should().Be(Role.Member);
	}

	[Fact]
	public void Create_ShouldRaiseDomainEvent_WhenUserCreated()
	{
		// Act
		var user = User.Create(
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName(),
			Guid.NewGuid().ToString());

		// Assert
		UserRegisteredDomainEvent domainEvent =
			AssertDomainEventWasPublished<UserRegisteredDomainEvent>(user);

		domainEvent.UserId.Should().Be(user.Id);
	}

	[Fact]
	public void Create_ShouldSetAllProperties()
	{
		// Arrange
		string email = Faker.Internet.Email();
		string firstName = Faker.Name.FirstName();
		string lastName = Faker.Name.LastName();
		string identityId = Guid.NewGuid().ToString();

		// Act
		var user = User.Create(email, firstName, lastName, identityId);

		// Assert
		user.Id.Should().NotBe(Guid.Empty);
		user.Email.Should().Be(email);
		user.FirstName.Should().Be(firstName);
		user.LastName.Should().Be(lastName);
		user.IdentityId.Should().Be(identityId);
	}

	[Fact]
	public void Create_ShouldRaiseDomainEvent_WithCorrectUserId()
	{
		// Act
		var user = User.Create(
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName(),
			Guid.NewGuid().ToString());

		// Assert
		user.DomainEvents.Should().HaveCount(1);
		UserRegisteredDomainEvent domainEvent =
			AssertDomainEventWasPublished<UserRegisteredDomainEvent>(user);
		domainEvent.UserId.Should().Be(user.Id);
	}

	[Fact]
	public void Update_ShouldRaiseDomainEvent_WhenUserUpdated()
	{
		// Arrange
		var user = User.Create(
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName(),
			Guid.NewGuid().ToString());

		// Act
		user.Update(user.LastName, user.FirstName);

		// Assert
		UserProfileUpdatedDomainEvent domainEvent =
			AssertDomainEventWasPublished<UserProfileUpdatedDomainEvent>(user);

		domainEvent.UserId.Should().Be(user.Id);
		domainEvent.FirstName.Should().Be(user.FirstName);
		domainEvent.LastName.Should().Be(user.LastName);
	}

	[Fact]
	public void Update_ShouldNotRaiseDomainEvent_WhenUserNotUpdated()
	{
		// Arrange
		var user = User.Create(
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName(),
			Guid.NewGuid().ToString());

		user.ClearDomainEvents();

		// Act
		user.Update(user.FirstName, user.LastName);

		// Assert
		user.DomainEvents.Should().BeEmpty();
	}

	[Fact]
	public void Update_ShouldUpdateFirstName_WhenOnlyFirstNameChanges()
	{
		// Arrange
		var user = User.Create(
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName(),
			Guid.NewGuid().ToString());

		string newFirstName = Faker.Name.FirstName();
		string lastNameBeforeUpdate = user.LastName;

		// Act
		user.Update(newFirstName, user.LastName);

		// Assert
		user.FirstName.Should().Be(newFirstName);
		user.LastName.Should().Be(lastNameBeforeUpdate);
	}

	[Fact]
	public void Update_ShouldUpdateLastName_WhenOnlyLastNameChanges()
	{
		// Arrange
		var user = User.Create(
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName(),
			Guid.NewGuid().ToString());

		string firstNameBeforeUpdate = user.FirstName;
		string newLastName = Faker.Name.LastName();

		// Act
		user.Update(user.FirstName, newLastName);

		// Assert
		user.FirstName.Should().Be(firstNameBeforeUpdate);
		user.LastName.Should().Be(newLastName);
	}

	[Fact]
	public void Update_ShouldUpdateBothNames_WhenBothChange()
	{
		// Arrange
		var user = User.Create(
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName(),
			Guid.NewGuid().ToString());

		string newFirstName = Faker.Name.FirstName();
		string newLastName = Faker.Name.LastName();

		// Act
		user.Update(newFirstName, newLastName);

		// Assert
		user.FirstName.Should().Be(newFirstName);
		user.LastName.Should().Be(newLastName);
	}
}
