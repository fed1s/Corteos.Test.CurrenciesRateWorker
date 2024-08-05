using Corteos.Test.CurrenciesRateWorker.Models;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Corteos.Test.CurrenciesRateWorker.Persistence.Repositories
{
    public class CurrenciesLibRepository //: ICurrenciesRepository
    {
        private readonly CurrencyDbContext _dbContext;
        public CurrenciesLibRepository(CurrencyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool IsCurrenciesLibEmpty()
        {
            return !_dbContext.Currencies.AsNoTracking().Any();
        }

        public bool IsCurrenciesLibEmptyOrChange(IEnumerable<CurrencyEntity> entities)
        {
            //var en = entities.Select(e => _dbContext.Currencies.AsNoTracking().Contains(e)).ToList();
            //var rr = entities.Where(e => !_dbContext.Currencies.AsNoTracking().Contains(e)).ToList();


            return !entities.All(e => _dbContext.Currencies.AsNoTracking().Contains(e));
        }

        public async Task AddCurrenciesLib(IEnumerable<CurrencyEntity> entities)
        {
            _dbContext.Currencies.AddRange(entities
                .Where(e => !_dbContext.Currencies
                    .AsNoTracking()
                    .Contains(e)));

            await _dbContext.SaveChangesAsync();
        }
    }
}
