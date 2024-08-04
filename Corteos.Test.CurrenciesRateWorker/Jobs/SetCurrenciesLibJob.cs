using Corteos.Test.CurrenciesRateWorker.Models;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces;
using Quartz;
using System.Text;
using System.Xml.Linq;

namespace Corteos.Test.CurrenciesRateWorker.Jobs
{
    [DisallowConcurrentExecution]
    public class SetCurrenciesLibJob : IJob
    {
        private readonly ILogger<SetCurrenciesLibJob> _logger;
        private readonly ICurrenciesRepository _currenciesRepository;

        public SetCurrenciesLibJob(ILogger<SetCurrenciesLibJob> logger, ICurrenciesRepository currenciesRepository)
        {
            _logger = logger;
            _currenciesRepository = currenciesRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Inspect currency lib");

            //Допускаем, что библиотека валют ЦБ стабильна. Грузится в БД единожды.
            if (_currenciesRepository.IsCurrenciesLibEmpty())
            {
                _logger.LogInformation("Fetching currency lib");

                //Корректная обработка кодировки encoding="windows-1251" у возвращаемого XML документа
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                XDocument xml = new();

                try
                {
                    xml = XDocument.Load("https://cbr.ru/scripts/XML_valFull.asp");
                }
                catch (Exception)
                {
                    _logger.LogInformation("Не удалось загрузить XML-словарь валют");
                    throw;
                }

                //В словаре валют присутствуют валюты без значений у элементов, фильтрация where их исключает
                //Валюты без кодов валют: Литовский талон, Item ID="R01436"; Украинский карбованец, Item ID="R01720A"
                var list = xml.Root?
                    .Elements("Item")
                    .Where(i => i.Elements().All(el => el.Value != ""))
                    .Select(ce => new CurrencyEntity
                    {
                        ISONumCodeId = int.Parse(ce.Element("ISO_Num_Code")?.Value),
                        ISOCharCode = ce.Element("ISO_Char_Code")?.Value,
                        CurrencyName = ce.Element("Name")?.Value,
                        CurrencyEngName = ce.Element("EngName")?.Value
                    })
                    .ToList();

                await _currenciesRepository.AddCurrenciesLib(list);

                _logger.LogInformation("Currency lib fetched");
            }
            else
            {
                _logger.LogInformation("Currency lib already exist");
            }
        }
    }
}
