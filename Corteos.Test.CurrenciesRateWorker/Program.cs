using Corteos.Test.CurrenciesRateWorker.Jobs;
using Corteos.Test.CurrenciesRateWorker.Persistence;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces;
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
                //q.AddJobListener<JobListener>();

                var updCurrLibJobKey = new JobKey("Set currencies");
                //var updCurrLibJobKey = new JobKey("Set currencies lib");
                q.AddJob<SetCurrenciesJob>(opts => opts
                //q.AddJob<SetCurrenciesLibJob>(opts => opts
                    .WithIdentity(updCurrLibJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(updCurrLibJobKey)
                    .WithIdentity(updCurrLibJobKey.Name + " trigger")
                    .StartNow()
                );
            });

            builder.Services.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            var host = builder.Build();
            
            //Применение миграций
            using var scope = host.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<CurrencyDbContext>().Database.Migrate();
            
            host.Run();
        }
    }
}