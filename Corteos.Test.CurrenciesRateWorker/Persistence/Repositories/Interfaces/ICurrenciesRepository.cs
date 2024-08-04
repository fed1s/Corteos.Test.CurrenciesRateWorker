using Corteos.Test.CurrenciesRateWorker.Models;

namespace Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces
{
    public interface ICurrenciesRepository
    {
        Task AddCurrenciesLib(IEnumerable<CurrencyEntity> entities);
        bool IsCurrenciesLibEmpty();
    }
}
