namespace Corteos.Test.CurrenciesRateWorker.Models
{
    public class CurrencyRateEntity
    {
        /// <summary>
        /// ���� ����� ������.
        /// </summary>
        public DateOnly CurrencyRateDate { get; set; } = new DateOnly();
        /// <summary>
        /// ������� ��������� �����.
        /// </summary>
        public int Nominal { get; set; } = 0;
        /// <summary>
        /// �������� ��������� �����.
        /// </summary>
        public decimal Value { get; set; } = 0;
        /// <summary>
        /// ������������� �������� ��� ������.
        /// </summary>
        public int NumCodeId { get; set; } = 0;

        public CurrencyEntity? Currency { get; set; }
    }
}
