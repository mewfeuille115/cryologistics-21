namespace Cryologistics21.Common.Application.Clock;

public interface IDateTimeProvider
{
	DateTime UtcNow { get; }
}
