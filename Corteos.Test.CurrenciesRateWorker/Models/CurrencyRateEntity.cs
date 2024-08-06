namespace Corteos.Test.CurrenciesRateWorker.Models
{
    public class CurrencyRateEntity
    {
        /// <summary>
        /// Дата курса валюты.
        /// </summary>
        public DateOnly CurrencyRateDate { get; set; } = new DateOnly();
        /// <summary>
        /// Номинал обменного курса.
        /// </summary>
        public int Nominal { get; set; } = 0;
        /// <summary>
        /// Значение обменного курса.
        /// </summary>
        public decimal Value { get; set; } = 0;
        /// <summary>
        /// Международный числовой код валюты.
        /// </summary>
        public int NumCodeId { get; set; } = 0;

        public CurrencyEntity? Currency { get; set; }
    }
}
