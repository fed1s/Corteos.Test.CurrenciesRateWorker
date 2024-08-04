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
            _logger.LogInformation(context.JobDetail.Key.Name + " job was executed.");

            if (jobException == null)
            {
                //Джоба проверки/получения истории курсов валют
                if (context.JobDetail.Key.Name == "Set currencies lib")
                {
                    var setCurrRetroRateJobKey = new JobKey("Set currencies retro rate");

                    var job = JobBuilder.Create<SetCurrenciesRetroRateJob>()
                            .WithIdentity(setCurrRetroRateJobKey)
                            .Build();

                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(setCurrRetroRateJobKey.Name + " trigger")
                        .StartNow()
                        .Build();

                    context.Scheduler.ScheduleJob(job, trigger, cancellationToken);
                }

                //Джоба ежедневного обновления курсов
                if (context.JobDetail.Key.Name == "Set currencies retro rate")
                {
                    var setCurrRateJobKey = new JobKey("Set currencies rate");

                    var job = JobBuilder.Create<SetCurrenciesRateJob>()
                            .WithIdentity(setCurrRateJobKey)
                            .Build();

                    //CRON триггер установлен на ежедневное выполнение в 18:01мск, в т.ч. в нерабочие и праздничные дни
                    //Исходя из информации в faq https://www.cbr.ru/dkp/faq/
                    //ЦБ РФ публикует курс валют на своем официальном сайте в сети Интернет до 18:00 по московскому времени, точное время не регламентировано.
                    //Официальные курсы иностранных валют по отношению к рублю устанавливаются ежедневно
                    //(за исключением нерабочих дней, являющихся выходными и (или) нерабочими праздничными днями),
                    //вступают в силу на следующий календарный день после дня установления.
                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(setCurrRateJobKey.Name + " trigger")
                        .WithCronSchedule("0 01 18 * * ?", x => x                                       //Ежедневное выполнение в 18-01
                            .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")))
                        .Build();

                    context.Scheduler.ScheduleJob(job, trigger, cancellationToken);
                }
            }
            else
            {
                _logger.LogInformation("An exception occured while executing job: " + context.JobDetail.Key.Name);
            }
            return base.JobWasExecuted(context, jobException, cancellationToken);
        }
    }
}