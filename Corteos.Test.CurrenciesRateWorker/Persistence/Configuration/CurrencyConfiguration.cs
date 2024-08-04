using Corteos.Test.CurrenciesRateWorker.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Corteos.Test.CurrenciesRateWorker.Persistence.Configuration
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<CurrencyEntity>
    {
        public void Configure(EntityTypeBuilder<CurrencyEntity> builder)
        {
            builder
                .HasKey(c => c.ISONumCodeId);

            builder
                .Property(c => c.ISONumCodeId)
                .ValueGeneratedNever();

            builder
                .HasMany(cc => cc.CurrenciesRate)
                .WithOne(c => c.Currency)
                .HasForeignKey(c => c.NumCodeId);
        }
    }
}
