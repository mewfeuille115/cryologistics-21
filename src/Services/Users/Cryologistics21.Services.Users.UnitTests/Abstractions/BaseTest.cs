using Bogus;
using Cryologistics21.Common.Domain;

namespace Cryologistics21.Services.Users.UnitTests.Abstractions;

public abstract class BaseTest
{
	protected static readonly Faker Faker = new();

	public static T AssertDomainEventWasPublished<T>(Entity entity)
		where T : IDomainEvent
	{
		T? domainEvent = entity.DomainEvents.OfType<T>().SingleOrDefault();

		if (domainEvent is null)
		{
			throw new Exception($"{typeof(T).Name} was not published");
		}

		return domainEvent;
	}
}
