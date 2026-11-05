using Cryologistics21.Modules.Users.Infrastructure.Outbox;
using Quartz;

namespace Cryologistics21.Services.Users.Infrastructure.Outbox;

internal static class ConfigureProcessOutboxJob
{
	internal static void Configure(IQuartzBuilder quartz, int intervalInSeconds)
	{
		string jobName = typeof(ProcessOutboxJob).FullName!;

		quartz.AddJob<ProcessOutboxJob>(job => job.WithIdentity(jobName))
			.AddTrigger(trigger => trigger
				.ForJob(jobName)
				.WithSimpleSchedule(TimeSpan.FromSeconds(intervalInSeconds)));
	}
}
