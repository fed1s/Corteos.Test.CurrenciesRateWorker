using Corteos.Test.CurrenciesRateWorker.Models;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories;
using Quartz;
using System.Text;
using System.Xml.Linq;

namespace Corteos.Test.CurrenciesRateWorker.Jobs
{
    
    [DisallowConcurrentExecution]
    public class SetCurrenciesJob : IJob
    {
        private readonly ILogger<SetCurrenciesJob> _logger;
        private readonly CurrenciesLibRepository _currenciesRepository;
        private readonly CurrenciesRateRepository _currenciesRateRepository;

        public SetCurrenciesJob(ILogger<SetCurrenciesJob> logger, CurrenciesLibRepository currenciesRepository, CurrenciesRateRepository currenciesRateRepository)
        {
            _logger = logger;
            _currenciesRepository = currenciesRepository;
            _currenciesRateRepository = currenciesRateRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);          //Корректная обработка кодировки encoding="windows-1251" у возвращаемого XML документа
            XDocument xmlLib = TryGetCurrenciesLibXml();
            XDocument xmlRate = TryGetCurrenciesRateXml();

            await CheckAndSetCurrenciesLibAsync(xmlLib);                            //Проверяем наличие/изменение библиотеки валют

            if (_currenciesRateRepository.IsCurrenciesRateEmpty())                  //Проверяем наличие курсов валют (первый запуск)
            {
                await SetCurrenciesRetroRateAsync(30);                              //Получение исторических данных о курсах, за 30 дней
            }

             await CheckAndSetCurrenciesRateAsync(xmlRate);                         //Проверяем актуальность данных. Если свежих нет, или неполные - заливаем
        }


        #region SetCurrencies Methods
        /// <summary>
        /// Проверка наличия и актуальности данных библиотеки валют в БД.
        /// Если библиотека отсутствует или неполная, её данные будут обновлены.
        /// </summary>
        /// <param name="xml">XML документ с библиотекой валют.</param>
        /// <returns></returns>
        private async Task CheckAndSetCurrenciesLibAsync(XDocument xml)
        {
            _logger.LogInformation("Проверка библиотеки валют");
            
            //В словаре валют присутствуют валюты без значений у элементов, фильтрация where их исключает
            //Валюты без кодов валют: Литовский талон, Item ID="R01436"; Украинский карбованец, Item ID="R01720A"
            var list = xml.Root
                .Elements("Item")
                .Where(i => i.Elements().All(el => el.Value != ""))
                .Select(ce => new CurrencyEntity
                {
                    ISONumCodeId = int.Parse(ce.Element("ISO_Num_Code").Value),
                    ISOCharCode = ce.Element("ISO_Char_Code").Value,
                    CurrencyName = ce.Element("Name").Value,
                    CurrencyEngName = ce.Element("EngName").Value
                });

            if (_currenciesRepository.IsCurrenciesLibEmptyOrChange(list))
            {
                _logger.LogInformation("Библиотека валют отсутствует или неактуальна. Получаем библиотеку валют");

                await _currenciesRepository.AddCurrenciesLib(list);
                
                _logger.LogInformation("Библиотека валют получена");
            }
            _logger.LogInformation("Библиотека валют актуальна");
        }

        /// <summary>
        /// Загрузка в БД исторических данных курсов валют за период в днях.
        /// </summary>
        /// <param name="days">Количество предыдущих дней (включая текущий), за которые необходимы данные.</param>
        /// <returns></returns>
        private async Task SetCurrenciesRetroRateAsync(int days)
        {
            _logger.LogInformation("Данные о курсах валют отсутствуют. Получаем исторические данные курсов валют");

            List<CurrencyRateEntity> currencyRateEntities = [];

            DateOnly reqDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));  //Текущая дата, время московское
            DateOnly retroDate = reqDate.AddDays(-days);                            //Дата, начиная с которой требуется выгрузка курсов

            while (reqDate > retroDate)
            {
                var xml = TryGetCurrenciesRateXml(reqDate);

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

            _logger.LogInformation("Исторические данные курсов валют получены");
        }


        /// <summary>
        /// Проверка и получение актуальных данных курсов валют.
        /// </summary>
        /// <param name="xml">XDocument с курсами валют.</param>
        /// <returns></returns>
        private async Task CheckAndSetCurrenciesRateAsync(XDocument xml)
        {
            _logger.LogInformation("Проверка актуальности курсов валют");

            var list = xml.Root
                    .Elements("Valute")
                    .Select(cre => new CurrencyRateEntity
                    {
                        CurrencyRateDate = DateOnly.ParseExact(xml.Root.Attribute("Date").Value, "dd.MM.yyyy"),
                        Nominal = int.Parse(cre.Element("Nominal").Value),
                        Value = decimal.Parse(cre.Element("Value").Value),
                        NumCodeId = int.Parse(cre.Element("NumCode").Value)
                    });

            if (!_currenciesRateRepository.IsCurrenciesRateActual(list))
            {
                _logger.LogInformation("Курсы валют неактуальны. Получение свежих данных");
                await _currenciesRateRepository.AddCurrenciesRate(list);
                _logger.LogInformation("Актуальные курсы валют получены");
            }
            else
            {
                _logger.LogInformation("Курсы валют актуальны");
            }
        }
        #endregion SetCurrencies Methods


        #region GetXMLData Methods
        /// <summary>
        /// Получение библиотеки курсов валют с сайта ЦБ.
        /// </summary>
        /// <returns>XDocument с библиотекой валют.</returns>
        private XDocument TryGetCurrenciesLibXml()
        {
            try
            {
                return XDocument.Load("https://cbr.ru/scripts/XML_valFull.asp");
            }
            catch (Exception)
            {
                _logger.LogError("Не удалось загрузить XML-словарь валют");
                throw;
            }
        }

        /// <summary>
        /// Получение курсов валют с сайта ЦБ.
        /// </summary>
        /// <returns>XDocument с курсами валют.</returns>
        private XDocument TryGetCurrenciesRateXml()
        {
            try
            {
                return XDocument.Load("https://cbr.ru/scripts/XML_daily.asp");
            }
            catch (Exception)
            {
                _logger.LogError("Не удалось загрузить XML-курс валют");
                throw;
            }
        }

        /// <summary>
        /// Получение курсов валют с сайта ЦБ.
        /// </summary>
        /// <param name="reqDate">Дата, на которую нужны данные курсов валют.</param>
        /// <returns>XDocument с курсами валют.</returns>
        private XDocument TryGetCurrenciesRateXml(DateOnly reqDate)
        {
            try
            {
                return XDocument.Load("https://cbr.ru/scripts/XML_daily.asp?date_req=" + reqDate.ToString("dd\\/MM\\/yyyy"));
            }
            catch (Exception)
            {
                _logger.LogError("Не удалось загрузить XML-курс валют");
                throw;
            }
        }
        #endregion GetXMLData Methods
    }
}
