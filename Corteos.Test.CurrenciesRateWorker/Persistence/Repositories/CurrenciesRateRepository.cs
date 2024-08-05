using Corteos.Test.CurrenciesRateWorker.Models;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Corteos.Test.CurrenciesRateWorker.Persistence.Repositories
{
    public class CurrenciesRateRepository //: ICurrenciesRateRepository
    {
        private readonly CurrencyDbContext _dbContext;

        public CurrenciesRateRepository(CurrencyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddCurrenciesRate(IEnumerable<CurrencyRateEntity> entities)
        {
            _dbContext.CurrenciesRate.AddRange(entities
                .Where(e => !_dbContext.CurrenciesRate
                    .AsNoTracking()
                    .Contains(e)));

            await _dbContext.SaveChangesAsync();
        }

        public bool IsCurrenciesRateEmpty()
        {
            return !_dbContext.CurrenciesRate.AsNoTracking().Any();
        }

        public bool IsCurrenciesRateActual(DateOnly reqDate)
        {
            return _dbContext.CurrenciesRate.AsNoTracking().Any(e => e.CurrencyRateDate == reqDate);
        }
    }
}
