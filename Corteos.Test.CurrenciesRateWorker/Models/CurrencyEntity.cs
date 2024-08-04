namespace Corteos.Test.CurrenciesRateWorker.Models
{
    public class CurrencyEntity
    {
        public int ISONumCodeId { get; set; } = 0;

        public string ISOCharCode { get; set; } = string.Empty;

        public string CurrencyName { get; set; } = string.Empty;

        public string CurrencyEngName { get; set; } = string.Empty;

        public ICollection<CurrencyRateEntity>? CurrenciesRate { get; set; }
    }
}
