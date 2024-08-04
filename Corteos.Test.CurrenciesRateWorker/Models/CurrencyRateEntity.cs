namespace Corteos.Test.CurrenciesRateWorker.Models
{
    public class CurrencyRateEntity
    {
        public DateOnly CurrencyRateDate { get; set; } = new DateOnly();

        public int Nominal { get; set; } = 0;

        public decimal Value { get; set; } = 0;

        public int NumCodeId { get; set; } = 0;

        public CurrencyEntity? Currency { get; set; }
    }
}
