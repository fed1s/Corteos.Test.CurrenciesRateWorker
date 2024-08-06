namespace Corteos.Test.CurrenciesRateWorker.Models
{
    public class CurrencyEntity
    {
        /// <summary>
        /// Международный числовой код валюты.
        /// </summary>
        public int ISONumCodeId { get; set; } = 0;
        /// <summary>
        /// Международный символьный код валюты.
        /// </summary>
        public string ISOCharCode { get; set; } = string.Empty;
        /// <summary>
        /// Наименование валюты.
        /// </summary>
        public string CurrencyName { get; set; } = string.Empty;
        /// <summary>
        /// Наименование валюты (английское).
        /// </summary>
        public string CurrencyEngName { get; set; } = string.Empty;

        public ICollection<CurrencyRateEntity>? CurrenciesRate { get; set; }
    }
}
