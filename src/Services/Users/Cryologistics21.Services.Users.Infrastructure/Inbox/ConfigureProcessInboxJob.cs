using Cryologistics21.Modules.Users.Infrastructure.Inbox;
using Quartz;

namespace Cryologistics21.Services.Users.Infrastructure.Inbox;

internal static class ConfigureProcessInboxJob
{
	internal static void Configure(IQuartzBuilder quartz, int intervalInSeconds)
	{
		string jobName = typeof(ProcessInboxJob).FullName!;

		quartz.AddJob<ProcessInboxJob>(job => job.WithIdentity(jobName))
			.AddTrigger(trigger => trigger
				.ForJob(jobName)
				.WithSimpleSchedule(TimeSpan.FromSeconds(intervalInSeconds)));
	}
}
