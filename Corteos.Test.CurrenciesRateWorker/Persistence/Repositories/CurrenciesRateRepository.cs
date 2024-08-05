using Corteos.Test.CurrenciesRateWorker.Models;
using Microsoft.EntityFrameworkCore;

namespace Corteos.Test.CurrenciesRateWorker.Persistence.Repositories
{
    public class CurrenciesRateRepository
    {
        private readonly CurrencyDbContext _dbContext;

        public CurrenciesRateRepository(CurrencyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Добавить в БД коллекцию уникальных данных о курсах валют.
        /// </summary>
        /// <param name="entities">Коллекция курсов валют.</param>
        /// <returns></returns>
        public async Task AddCurrenciesRate(IEnumerable<CurrencyRateEntity> entities)
        {
            _dbContext.CurrenciesRate.AddRange(entities
                .Where(e => !_dbContext.CurrenciesRate
                    .AsNoTracking()
                    .Contains(e)));

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Проверка наличия любых данных о курсах валют в БД.
        /// </summary>
        /// <returns>true, если данные в БД отсутствуют.</returns>
        public bool IsCurrenciesRateEmpty()
        {
            return !_dbContext.CurrenciesRate.AsNoTracking().Any();
        }

        /// <summary>
        /// Проверка актуальности данных о курсах валют в БД.
        /// </summary>
        /// <param name="entities">Коллекция данных о курсах валют.</param>
        /// <returns>false, если данные о курсах валют неполные.</returns>
        public bool IsCurrenciesRateActual(IEnumerable<CurrencyRateEntity> entities)
        {
            return entities.All(e => _dbContext.CurrenciesRate.AsNoTracking().Contains(e));
        }
    }
}
