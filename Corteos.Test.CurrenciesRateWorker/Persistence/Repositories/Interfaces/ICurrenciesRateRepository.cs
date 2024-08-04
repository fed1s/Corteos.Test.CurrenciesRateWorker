using Corteos.Test.CurrenciesRateWorker.Models;

namespace Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces
{
    public interface ICurrenciesRateRepository
    {
        Task AddCurrenciesRate(IEnumerable<CurrencyRateEntity> entities);
        bool IsCurrenciesRateEmpty();
        bool IsCurrenciesRateActual(DateOnly reqDate);
    }
}
