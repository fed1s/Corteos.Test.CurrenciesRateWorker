using Quartz;
using Quartz.Listener;

namespace Corteos.Test.CurrenciesRateWorker.Jobs
{
    public class JobListener : JobListenerSupport
    {
        private readonly ILogger<JobListener> _logger;
        public override string Name => "JobListener";

        public JobListener(ILogger<JobListener> logger)
        {
            _logger = logger;
        }

        public override Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return base.JobExecutionVetoed(context, cancellationToken);
        }

        public override Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return base.JobToBeExecuted(context, cancellationToken);
        }

        public override Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(context.JobDetail.Key.Name + " job выполнена");

            if (jobException == null)
            {
                if(context.Trigger.Key.Name == "Start app trigger")
                {
                    var setCurrenciesRateJobKey = new JobKey("Set currencies rate");

                    var job = JobBuilder.Create<SetCurrenciesJob>()
                            .WithIdentity(setCurrenciesRateJobKey)
                            .Build();

                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(setCurrenciesRateJobKey.Name + " trigger")
                        .WithCronSchedule("0 01 18 * * ?", x => x                                       //Ежедневное выполнение в 18-01
                            .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")))
                        .StartNow()
                        .Build();

                    context.Scheduler.ScheduleJob(job, trigger, cancellationToken);
                    _logger.LogInformation("Выполнение повторяющиейся задачи добавлено в очередь");
                }
            }
            else
            {
                _logger.LogError("An exception occured while executing job: " + context.JobDetail.Key.Name);
                _logger.LogInformation("Exception occured. Refire Start app job in 5 minutes");


                var refireStartAppJobKey = new JobKey("Start app");
                
                var job = JobBuilder.Create<SetCurrenciesJob>()
                            .WithIdentity(refireStartAppJobKey)
                            .Build();

                var trigger = TriggerBuilder.Create()
                        .WithIdentity(refireStartAppJobKey.Name + " trigger")
                        .StartAt(DateTimeOffset.UtcNow.AddMinutes(5))
                        .StartNow()
                        .Build();

                context.Scheduler.ScheduleJob(job, trigger, cancellationToken);
            }
            return base.JobWasExecuted(context, jobException, cancellationToken);
        }
    }
}