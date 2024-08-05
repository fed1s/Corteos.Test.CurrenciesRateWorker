using Corteos.Test.CurrenciesRateWorker.Models;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories;
using Corteos.Test.CurrenciesRateWorker.Persistence.Repositories.Interfaces;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            DateOnly actualRateDate = DateOnly.Parse(xmlRate.Root.Attribute("Date").Value);

            
            await CheckCurrenciesLibAsync(xmlLib);                                  //Проверяем наличие/изменение библиотеки валют

            if (_currenciesRateRepository.IsCurrenciesRateEmpty())                  //Проверяем наличие курсов валют (первый запуск)
            {
                await SetCurrenciesRetroRateAsync(30);
            }


            _logger.LogInformation("Проверяем актуальность данных курсов валют");

            if (!_currenciesRateRepository.IsCurrenciesRateActual(actualRateDate))  //Проверяем актуальность данных. Если свежих нет - заливаем
            {
                await SetCurrenciesRateAsync(xmlRate);
            }
            else
            {
                _logger.LogInformation("Курсы валют актуальны");
            }
        }



        //Methods
        //привести к нормальному виду комментария

        private async Task CheckCurrenciesLibAsync(XDocument xml)
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
                _logger.LogInformation("Получаем библиотеку валют");

                await _currenciesRepository.AddCurrenciesLib(list);
                
                _logger.LogInformation("Библиотека валют получена");
            }
            _logger.LogInformation("Библиотека валют актуальна");
        }


        private async Task SetCurrenciesRetroRateAsync(int days)
        {
            _logger.LogInformation("Получаем исторические данные курсов валют");

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


        private async Task SetCurrenciesRateAsync(XDocument xml)
        {
            _logger.LogInformation("Получаем актуальные курсы валют");

            var list = xml.Root
                    .Elements("Valute")
                    .Select(cre => new CurrencyRateEntity
                    {
                        CurrencyRateDate = DateOnly.ParseExact(xml.Root.Attribute("Date").Value, "dd.MM.yyyy"),
                        Nominal = int.Parse(cre.Element("Nominal").Value),
                        Value = decimal.Parse(cre.Element("Value").Value),
                        NumCodeId = int.Parse(cre.Element("NumCode").Value)
                    });

            await _currenciesRateRepository.AddCurrenciesRate(list);

            _logger.LogInformation("Актуальные курсы валют получены");
        }


        #region GetXMLData Methods
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
