namespace Corteos.Test.CurrenciesRateWorker.Models
{
    public class CurrencyEntity
    {
        /// <summary>
        /// ������������� �������� ��� ������.
        /// </summary>
        public int ISONumCodeId { get; set; } = 0;
        /// <summary>
        /// ������������� ���������� ��� ������.
        /// </summary>
        public string ISOCharCode { get; set; } = string.Empty;
        /// <summary>
        /// ������������ ������.
        /// </summary>
        public string CurrencyName { get; set; } = string.Empty;
        /// <summary>
        /// ������������ ������ (����������).
        /// </summary>
        public string CurrencyEngName { get; set; } = string.Empty;

        public ICollection<CurrencyRateEntity>? CurrenciesRate { get; set; }
    }
}
