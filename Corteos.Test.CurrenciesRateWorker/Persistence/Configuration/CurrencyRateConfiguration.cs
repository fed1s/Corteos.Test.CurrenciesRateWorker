using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Corteos.Test.CurrenciesRateWorker.Models;

namespace Corteos.Test.CurrenciesRateWorker.Persistence.Configuration
{
    public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRateEntity>
    {
        public void Configure(EntityTypeBuilder<CurrencyRateEntity> builder)
        {
            builder
                .HasKey(c => new { c.NumCodeId, c.CurrencyRateDate });

            builder
                .HasOne(c => c.Currency)
                .WithMany(cc => cc.CurrenciesRate)
                .HasForeignKey(c => c.NumCodeId);
        }
    }
}
