namespace Application.Common.Interfaces;

public interface IBackgroundJobService
{
    Task EnqueueAsync(string jobName, Func<Task> job);

    Task ScheduleAsync(string jobName, Func<Task> job, TimeSpan delay);

    Task RecurringAsync(string jobName, Func<Task> job, string cronExpression);
}
