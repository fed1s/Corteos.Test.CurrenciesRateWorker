using Corteos.Test.CurrenciesRateWorker.Models;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces;
using Quartz;
using System.Text;
using System.Xml.Linq;

namespace Corteos.Test.CurrenciesRateWorker.Jobs
{
    [DisallowConcurrentExecution]
    public class SetCurrenciesRateJob : IJob
    {
        private readonly ILogger<SetCurrenciesRateJob> _logger;
        private readonly ICurrenciesRateRepository _currenciesRateRepository;

        public SetCurrenciesRateJob(ILogger<SetCurrenciesRateJob> logger, ICurrenciesRateRepository currenciesRateRepository)
        {
            _logger = logger;
            _currenciesRateRepository = currenciesRateRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Inspect actual currency rates");

            //Корректная обработка кодировки encoding="windows-1251" у возвращаемого XML документа
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            XDocument xml = new();

            try
            {
                xml = XDocument.Load("https://cbr.ru/scripts/XML_daily.asp");
            }
            catch (Exception)
            {
                _logger.LogInformation("Ошибка получения курсов валют");
                throw;
            }

            //Проверяем актуальность данных. Если свежих нет - заливаем
            if (!_currenciesRateRepository.IsCurrenciesRateActual(DateOnly.Parse(xml.Root?.Attribute("Date").Value)))
            {
                _logger.LogInformation("Fetch currency rates");

                var list = xml.Root?
                    .Elements("Valute")
                    .Select(cre => new CurrencyRateEntity
                    {
                        CurrencyRateDate = DateOnly.ParseExact(xml.Root?.Attribute("Date").Value, "dd.MM.yyyy"),
                        Nominal = int.Parse(cre.Element("Nominal")?.Value),
                        Value = decimal.Parse(cre.Element("Value")?.Value),
                        NumCodeId = int.Parse(cre.Element("NumCode")?.Value)
                    })
                    .ToList();

                await _currenciesRateRepository.AddCurrenciesRate(list);

                _logger.LogInformation("Сurrency rates fetched");
            }
            else
            {
                _logger.LogInformation("Currency rates actual");
            }
        }
    }
}
