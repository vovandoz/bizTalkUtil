using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Breutil.Model;
using Microsoft.RuleEngine;

namespace Breutil.Helpers
{
    public static class RuleReadWriter
    {
        public delegate void RuleReadWriterHandler(string errorMsq);

        public static event RuleReadWriterHandler Error;

        /// <summary>
        /// Исключает  Default словари(Predicates, Functions и т.д.) не предназначенные для Экспорта
        /// </summary>
        /// <param name="vocab"></param>
        /// <returns></returns>
        private static bool IsIgnorVocab(VocabularyInfo vocab)
        {
            var appSettings = ConfigurationManager.AppSettings;
            if (appSettings.Count == 0)
                Error?.Invoke("appSetings пуст");
            else
            {
                var settingsStr = appSettings["ignorVocab"];
                var settings = settingsStr.Split(';');
                foreach (var setting in settings)
                {
                    var nameAndVersion = setting.Split(',');
                    var name = nameAndVersion[0];
                    var version = nameAndVersion[1].Split('.');
                    var majorV = int.Parse(version[0]);
                    var minorV = int.Parse(version[1]);
                    if ((name == vocab.Name) & (majorV == vocab.MajorRevision) & (minorV == vocab.MinorRevision))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Вспомогательный метод для поиска правила максимальной версии
        /// </summary>
        /// <param name="tempRecord"></param>
        /// <param name="ruleSetsFromDb"></param>
        /// <returns></returns>
        public static RuleSetInfo FindMaxVersionForRule(BreRecord tempRecord, RuleSetInfo[] ruleSetsFromDb)
        {
            var subset =
                ruleSetsFromDb
                    .Where(_ => _.Name == tempRecord.Name)
                    .OrderByDescending(_ => _.MajorRevision)
                    .ThenByDescending(_ => _.MinorRevision)
                    .FirstOrDefault(); //если не нашел вернется null
            return subset;
        }

        /// <summary>
        /// Вспомогательный метод для поиска словаря указанного без версии
        /// </summary>
        /// <param name="tempRecord"></param>
        /// <param name="vocabularySets"></param>
        /// <returns></returns>
        public static List<VocabularyInfo> FindVocabWhenVersionNotDefined(BreRecord tempRecord,
            VocabularyInfo[] vocabularySets)
        {
            var searchedVocab = vocabularySets.Where(_ => _.Name.Equals(tempRecord.Name)).ToList();
            return searchedVocab.Count == 0 ? null : searchedVocab;
        }

        /// <summary>
        /// Перед Import в БД делит на rule/vocab фаилы с диска.Имена фаилов представляют rule-names из CMD.Возвращает инстанс BreRecord на основе найденых rule/vocab.Возвращает null,если xml не rule и не vocab
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static BreRecord SplitForRead(string fileName)
        {
            if (!File.Exists(Path.Combine(fileName)))
                Error?.Invoke($"Фаил {fileName} не существует");
            var xml = new XElement(XElement.Load(fileName));
            var xmlElements = xml.Elements().ToList();
            foreach (var element in xmlElements)
            {
                var tagName = element.Name.LocalName;
                if (tagName.Equals("ruleset"))
                    return new RuleRecord(null) { FullFileName = fileName };

                if (tagName.Equals("vocabulary"))
                    return new VocabRecord(null) { FullFileName = fileName };
            }
            return null;
        }

        /// <summary>
        /// Заполняет  List BreRecords при Export All
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static List<BreRecord> CreateFileForWriteAll(string serverName, string dbName)
        {
            var records = new List<BreRecord>();
            var ruleSetsFromDb = RuleAndVocabDbAdapter.GetRuleSetInfo(serverName, dbName);
            foreach (var rule in ruleSetsFromDb)
                records.Add(new RuleRecord(rule));

            var vocabularySetsFromDb = RuleAndVocabDbAdapter.GetVocabularyInfoSet(serverName, dbName);
            foreach (var vocabulary in vocabularySetsFromDb)
            {
                if (IsIgnorVocab(vocabulary)) continue;
                records.Add(new VocabRecord(vocabulary));
            }

            return records;
        } //List<BreRecord> records-заполненяется rule/vocab из БД

        /// <summary>
        /// Создает инстанс BreRecord на основе ruleNames из CMD.Далее этот инстанс используется для поиска и создания RuleRecord или VocabRecord в методе CreateFileForWriteList()
        /// </summary>
        /// <param name="ruleName"></param>
        /// <returns></returns>
        public static BreRecord CreateRecord(string ruleName)
        {
            var nameAndVersion = ruleName.Split(',');
            if (nameAndVersion.Length == 1)
            {
                var tempBreRecord = new BreRecord()
                {
                    Name = ruleName,
                    UseMaxVersion = true
                };
                return tempBreRecord;
            }
            else
            {
                var version = nameAndVersion[1].Split('.');
                var tempBreRecord = new BreRecord() //version[0]-majorVersion,[1]-minorVersion
                {
                    Name = nameAndVersion[0],
                    MajorRevision = int.Parse(version[0]), //TODO поверять валидность версий
                    MinorRevision = int.Parse(version[1]),
                    FullFileName = $"{nameAndVersion[0]}-{version[0]}.{version[1]}.xml"
                };
                return tempBreRecord;
            }
        }

        /// <summary>
        ///При Export List находит rule или vocab В БД.Возвращает инстанс BreRecord предсятавляющий найденый rule/vocab 
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="dbName"></param>
        /// <param name="tempRecord"></param>
        public static List<BreRecord> CreateFileForWriteList(string serverName, string dbName, BreRecord tempRecord) // заполняет lIst рапарсенных rule-names
        {
            var foundedRuleOrVocab = new List<BreRecord>();
            var ruleSetsFromDb = RuleAndVocabDbAdapter.GetRuleSetInfo(serverName, dbName);
            var vocabularySetsFromDb = RuleAndVocabDbAdapter.GetVocabularyInfoSet(serverName, dbName);
            if (tempRecord.UseMaxVersion) //для rule ведется поиск максимальной версии на основе переданного имени(версия не указана).При удачном поиске из БД вернется единственная запись-макс.версия для этого правила.
            {                             //Для vocab ведется поиск  на основе переданного имени, не макс.версии,а всех версий этого vocab.При удачном поиске из БД вернется от 1 до n версий данного словаря

                var rule = FindMaxVersionForRule(tempRecord, ruleSetsFromDb); //сначала ищем совпадение имени переданного в ЦМД в массиве rule из БД.Совпалений нет?return null
                if (rule == null)
                {
                    var searchedVocab = FindVocabWhenVersionNotDefined(tempRecord, vocabularySetsFromDb); // продолжаем поиск в списке vocab из БД.Совпадений нет?Error
                    if (searchedVocab == null)
                        Error?.Invoke(
                            $@"rule/vocab переданное в CMD {tempRecord.Name} не найдено в БД"); //либо правило\словарь реально отсутствует, либо введено с ошибками

                    foreach (var file in searchedVocab)
                    {
                        foundedRuleOrVocab.Add(new VocabRecord(file));
                    }
                }
                else foundedRuleOrVocab.Add(new RuleRecord(rule));//совпадения есть-запись  записываем rule в List
            }

            else //для rule и vocab ведестся поиск конкретной записи по имени и версии. 
            {
                var fileIsFound = false;
                foreach (var rule in ruleSetsFromDb)
                {
                    var fullRuleName =
                        $"{rule.Name}-{rule.MajorRevision}.{rule.MinorRevision}.xml";
                    if (fullRuleName == tempRecord.FullFileName) //сравниваются имена из БД с именами из ЦМД
                    {
                        foundedRuleOrVocab.Add(new RuleRecord(rule));
                        fileIsFound = true;
                        break;
                    }

                    foreach (var vocab in vocabularySetsFromDb)
                    {
                        var fullVocabName =
                            $"{vocab.Name}-{vocab.MajorRevision}.{vocab.MinorRevision}.xml";
                        if (fullVocabName == tempRecord.FullFileName)
                        {
                            foundedRuleOrVocab.Add(new VocabRecord(vocab));
                            fileIsFound = true;
                            break;
                        }
                    }
                }

                if (!fileIsFound)
                    Error?.Invoke($@"rule/vocab переданное в CMD {tempRecord.Name} не найдено в БД");
            }
            return foundedRuleOrVocab;
        }

        public static BreRecord CreateFileForReadList(string ruleName)
        {
            var record = SplitForRead(Path.Combine(ruleName));
            if (record == null)
                Error?.Invoke($@"фаил {ruleName} не является rule или vocab");
            return record;
        }

        public static List<BreRecord> CreateFileForReadAll(string path)
        {
            var records = new List<BreRecord>();
            if (!path.Contains("*"))
                Error?.Invoke(@"не указана маска");
            var pathAndMask = path.Split('*');
            var files = Directory.GetFiles(Path.Combine(pathAndMask[0]), @"*" + pathAndMask[1]);
            if (files.Length == 0)
                Error?.Invoke(@"в указаной дериктории отсутствуют необходимые фаилы");

            foreach (var fileName in files)
            {
                var record = SplitForRead(fileName);
                if (record != null)
                    records.Add(record);
                else Error?.Invoke($@"фаил {fileName} не является rule или vocab");
            }
            return records;
        }

        /// <summary>
        /// Import в БД
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="dbName"></param>
        /// <param name="records"></param>
        public static void Read(string serverName, string dbName, List<BreRecord> records)
        {
            var deploymentDriver = RuleAndVocabDbAdapter.GetDeploymentDriver(serverName, dbName);
            foreach (var record in records) //при записи в БД необходимо сначала записывать vocab. List<BreRecord> records - это parsed.BreRecods
                if (record is VocabRecord)
                    deploymentDriver.ImportAndPublishFileRuleStore(record.FullFileName);
            foreach (var record in records)
                if (record is RuleRecord)
                    deploymentDriver.ImportAndPublishFileRuleStore(record.FullFileName);
        }

        /// <summary>
        /// Export из БД
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="dbName"></param>
        /// <param name="records"></param>
        /// <param name="path"></param>
        /// <param name="isOverwrite"></param>
        public static void Write(string serverName, string dbName, List<BreRecord> records, string path, bool isOverwrite)
        {
            var deploymentDriver = RuleAndVocabDbAdapter.GetDeploymentDriver(serverName, dbName);
            foreach (var record in records) //если флаг isOverwrite = false, проверяем на сущ-ие фаила на диске
            {
                if (!isOverwrite)
                    if (File.Exists(Path.Combine(path, $"{record.Name}-{record.MajorRevision}.{record.MinorRevision}.xml")))
                        Error?.Invoke($@"фаил {record.Name}-{record.MajorRevision}.{record.MinorRevision} уже существует");
            }

            foreach (var record in records)
            {
                if (record is RuleRecord)
                {
                    var rule = (RuleRecord)record;
                    deploymentDriver.ExportRuleSetToFileRuleStore(rule.RuleSetInfo, Path.Combine(path, $"{record.Name}-{record.MajorRevision}.{record.MinorRevision}.xml"));
                }

                else
                {
                    var vocab = (VocabRecord)record;
                    deploymentDriver.ExportVocabularyToFileRuleStore(vocab.VocabularyRecord, Path.Combine(path, $"{record.Name}-{record.MajorRevision}.{record.MinorRevision}.xml"));
                }
            }
        }
    }
}

