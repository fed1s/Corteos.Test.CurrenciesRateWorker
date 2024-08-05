using Corteos.Test.CurrenciesRateWorker.Jobs;
using Corteos.Test.CurrenciesRateWorker.Persistence;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.AspNetCore;

namespace Corteos.Test.CurrenciesRateWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddScoped<CurrenciesLibRepository>();
            builder.Services.AddScoped<CurrenciesRateRepository>();

            builder.Services.AddDbContext<CurrencyDbContext>();

            builder.Services.AddQuartz(q =>
            {
                q.AddJobListener<JobListener>();

                var startAppJobKey = new JobKey("Start app");
                q.AddJob<SetCurrenciesJob>(opts => opts
                    .WithIdentity(startAppJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(startAppJobKey)
                    .WithIdentity(startAppJobKey.Name + " trigger")
                    .StartNow()
                );
            });

            builder.Services.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            var host = builder.Build();
            
            using var scope = host.Services.CreateScope();                                      //Применение миграций
            scope.ServiceProvider.GetRequiredService<CurrencyDbContext>().Database.Migrate();
            
            host.Run();
        }
    }
}