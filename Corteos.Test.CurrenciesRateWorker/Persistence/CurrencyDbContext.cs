using Corteos.Test.CurrenciesRateWorker.Models;
using Corteos.Test.CurrenciesRateWorker.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Corteos.Test.CurrenciesRateWorker.Persistence
{
    public class CurrencyDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public CurrencyDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString(nameof(CurrencyDbContext)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
            modelBuilder.ApplyConfiguration(new CurrencyRateConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<CurrencyEntity> Currencies { get; set; }
        public DbSet<CurrencyRateEntity> CurrenciesRate { get; set; }
    }
}
