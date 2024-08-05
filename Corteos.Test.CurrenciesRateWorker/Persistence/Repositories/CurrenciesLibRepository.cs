using Corteos.Test.CurrenciesRateWorker.Models;
using Microsoft.EntityFrameworkCore;

namespace Corteos.Test.CurrenciesRateWorker.Persistence.Repositories
{
    public class CurrenciesLibRepository
    {
        private readonly CurrencyDbContext _dbContext;
        public CurrenciesLibRepository(CurrencyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Проверка наличия любых данных в библиотеке валют в БД.
        /// </summary>
        /// <returns>true, если данные отсутствуют.</returns>
        public bool IsCurrenciesLibEmpty()
        {
            return !_dbContext.Currencies.AsNoTracking().Any();
        }

        /// <summary>
        /// Проверка наличия или полноты данных в библиотеке валют в БД.
        /// </summary>
        /// <param name="entities">Коллекция валют для сравнения.</param>
        /// <returns>true, если данные отсутствуют или неполные.</returns>
        public bool IsCurrenciesLibEmptyOrChange(IEnumerable<CurrencyEntity> entities)
        {
            return !entities.All(e => _dbContext.Currencies.AsNoTracking().Contains(e));
        }

        /// <summary>
        /// Добавить библиотеку валют в БД.
        /// </summary>
        /// <param name="entities">Коллекция валют для добавления.</param>
        /// <returns></returns>
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
