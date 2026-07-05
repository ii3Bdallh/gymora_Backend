namespace Application.Interface.Service.Shared;

public interface IBackgroundJobService
{
    Task EnqueueAsync(string jobName, Func<Task> job);

    Task ScheduleAsync(string jobName, Func<Task> job, TimeSpan delay);

    Task RecurringAsync(string jobName, Func<Task> job, string cronExpression);
}
