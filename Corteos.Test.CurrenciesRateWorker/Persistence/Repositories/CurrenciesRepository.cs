using Corteos.Test.CurrenciesRateWorker.Models;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Corteos.Test.CurrenciesRateWorker.Persistence.Repositories
{
    public class CurrenciesRepository : ICurrenciesRepository
    {
        private readonly CurrencyDbContext _dbContext;
        public CurrenciesRepository(CurrencyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddCurrenciesLib(IEnumerable<CurrencyEntity> entities)
        {
            _dbContext.Currencies.AddRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        public bool IsCurrenciesLibEmpty()
        {
            return !_dbContext.Currencies.AsNoTracking().Any();
        }
    }
}
