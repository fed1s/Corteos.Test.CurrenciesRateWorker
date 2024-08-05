using Corteos.Test.CurrenciesRateWorker.Models;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces;
using Quartz;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Corteos.Test.CurrenciesRateWorker.Jobs
{
    [DisallowConcurrentExecution]
    public class SetCurrenciesRetroRateJob : IJob
    {
        private readonly ILogger<SetCurrenciesRetroRateJob> _logger;
        private readonly CurrenciesRateRepository _currenciesRateRepository;

        public SetCurrenciesRetroRateJob(ILogger<SetCurrenciesRetroRateJob> logger, CurrenciesRateRepository currenciesRateRepository)
        {
            _logger = logger;
            _currenciesRateRepository = currenciesRateRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Inspect currency rates");

            //Проверяем наличие данных в таблице CurrenciesRate
            if (_currenciesRateRepository.IsCurrenciesRateEmpty())
            {
                _logger.LogInformation("Fetching retro currency rates");
                
                //Корректная обработка кодировки encoding="windows-1251" у возвращаемого XML документа
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                XDocument xml = new();

                //Время приводим к московскому
                DateOnly reqDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

                //Данные за последние 30 дней, при необходимости можно задать не хардкодом, а например из конфигов
                DateOnly retroDate = reqDate.AddDays(-30);

                List<CurrencyRateEntity> currencyRateEntities = new List<CurrencyRateEntity>();

                try
                {
                    while (reqDate > retroDate)
                    {
                        xml = XDocument.Load("https://cbr.ru/scripts/XML_daily.asp?date_req=" + reqDate.ToString("dd\\/MM\\/yyyy"));

                        if (string.Compare(reqDate.ToString("dd.MM.yyyy"), xml.Root.Attribute("Date").Value) != 0)
                        {
                            reqDate = reqDate.AddDays(-1);
                            continue;
                        }

                        var list = xml.Root
                            .Elements("Valute")
                            .Select(cre => new CurrencyRateEntity
                            {
                                CurrencyRateDate = DateOnly.ParseExact(xml.Root.Attribute("Date").Value, "dd.MM.yyyy"),
                                Nominal = int.Parse(cre.Element("Nominal").Value),
                                Value = decimal.Parse(cre.Element("Value").Value),
                                NumCodeId = int.Parse(cre.Element("NumCode").Value)
                            });

                        currencyRateEntities.AddRange(list);
                        reqDate = reqDate.AddDays(-1);
                    }

                    await _currenciesRateRepository.AddCurrenciesRate(currencyRateEntities);
                }
                catch (Exception)
                {
                    _logger.LogInformation("Ошибка получения истории курсов валют");
                    throw;
                }
                _logger.LogInformation("Currency retro fetched");
            }
            else
            {
                _logger.LogInformation("Currency retro already exist");
            }
        }
    }
}
