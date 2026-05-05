using Cryologistics21.Common.Application.Clock;

namespace Cryologistics21.Common.Infrastructure.Clock;

public sealed class DateTimeProvider : IDateTimeProvider
{
	public DateTime UtcNow => DateTime.UtcNow;
}
