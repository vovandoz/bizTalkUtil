using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using Breutil.Model;

namespace Breutil.Helpers
{
    public class RuleEngineDb//TODO рефакторинг всего класса
    {
        public static void ShowAllRuleAndVocab(bool depend)
        {
            var dp = ConfigurationManager.AppSettings["provider"];
            var cnstr = ConfigurationManager.AppSettings["cnStr"];
            var df = DbProviderFactories.GetFactory(dp);

            using (var cn = df.CreateConnection())
            {
                if (cn != null)
                {
                    cn.ConnectionString = cnstr;
                    cn.Open();
                }

                if (depend)
                {
                    var cmd = df.CreateCommand();
                    if (cmd != null)
                    {
                        cmd.Connection = cn;
                        cmd.CommandText =
                            "select r.strName, r.nMajor, r.nMinor,v.strName, v.nMajor, v.nMinor from re_ruleset as r inner join re_ruleset_to_vocabulary_links as rv on r.nRuleSetID = rv.nReferingRuleset inner join re_vocabulary as v on v.nVocabularyID = rv.nVocabularyID order by r.nRuleSetID desc";
                        using (var dr = cmd.ExecuteReader())
                        {
                            var recList = new List<dynamic>();
                            while (dr.Read())
                            {
                                dynamic rec = new
                                {
                                    RuleName = (string)dr[0],
                                    RuleMajorVersion = dr[1],
                                    RuleMinorVersion = dr[2],
                                    VocabName = dr[3],
                                    VocabMajorVer = dr[4],
                                    VocabMinorVer = dr[5]
                                };
                                recList.Add(rec);
                            }

                            var groupedRecordsList = recList.GroupBy(_ => _.RuleName.ToString() + " "
                                                                                                + _.RuleMajorVersion
                                                                                                    .ToString() + "."
                                                                                                + _.RuleMinorVersion
                                                                                                    .ToString());
                            foreach (var group in groupedRecordsList)
                            {
                                Console.WriteLine($"{group.Key}");
                                foreach (var row in group)
                                {
                                    Console.WriteLine(
                                        $"\t*{row.VocabName.ToString()} {row.VocabMajorVer.ToString()}.{row.VocabMinorVer.ToString()}");
                                }
                            }

                        }
                    }
                }
                else
                {
                    var cmd = df.CreateCommand();
                    if (cmd != null)
                    {
                        cmd.Connection = cn;
                        cmd.CommandText =
                            "select r.strName, r.nMajor, r.nMinor from re_ruleset as r";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Console.WriteLine($"{dr[0]} {dr[1]}.{dr[2]}");
                            }

                            Console.WriteLine("*************************************");
                        }

                        cmd.CommandText =
                            "select v.strName, v.nMajor, v.nMinor from re_vocabulary as v";
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Console.WriteLine($"{dr[0]} {dr[1]}.{dr[2]}");
                            }
                        }
                    }
                }
            }
            Console.ReadLine();
        }

        public static void ShowListRuleAndVocab(List<BreRecord> rules, bool depend) //список правил и словарей
        {

            var dp = ConfigurationManager.AppSettings["provider"];
            var cnstr = ConfigurationManager.AppSettings["cnStr"];
            var df = DbProviderFactories.GetFactory(dp);

            using (var cn = df.CreateConnection())
            {
                if (cn != null)
                {
                    cn.ConnectionString = cnstr;
                    cn.Open();
                }

                if (depend) //отображать с зависимостями-правило и связанные с ним словари 
                {
                    var vocabAndNotFound = new Dictionary<string, List<string>>();
                    var cmd = df.CreateCommand();
                    if (cmd != null)
                    {
                        cmd.Connection = cn;
                        cmd.CommandText =
                            "select r.strName, r.nMajor, r.nMinor,v.strName, v.nMajor, v.nMinor from re_ruleset as r inner join re_ruleset_to_vocabulary_links as rv " +
                            "on r.nRuleSetID = rv.nReferingRuleset inner join re_vocabulary as v on v.nVocabularyID = rv.nVocabularyID order by r.nRuleSetID desc";
                        var recList = new List<dynamic>(); //Запрошена таблица правил и связанных с ними словарей
                        using (var dr = cmd.ExecuteReader())
                        {

                            while (dr.Read())
                            {
                                dynamic rec = new
                                {
                                    RuleName = (string)dr[0],
                                    RuleMajorVersion = dr[1],
                                    RuleMinorVersion = dr[2],
                                    VocabName = dr[3],
                                    VocabMajorVer = dr[4],
                                    VocabMinorVer = dr[5]
                                };
                                recList.Add(rec);
                            }
                        }

                        var groupedRecordsList = recList.GroupBy(_ => _.RuleName.ToString() + " "
                                                                                                + _.RuleMajorVersion
                                                                                                    .ToString() + "."
                                                                                                + _.RuleMinorVersion
                                                                                                    .ToString()); //формируется таблица-"правило связанные с ним словари"
                        vocabAndNotFound.Add("vocab", new List<string>());
                        vocabAndNotFound.Add("NOT FOUND", new List<string>());
                        foreach (var rule in rules)
                        {

                            var temp = groupedRecordsList.FirstOrDefault(_ => _.Key == $"{rule.Name} {rule.MajorRevision}.{rule.MinorRevision}"); //поиск входит ли преданное правило\словарь в итоговую таблицу
                            if (temp != null) // входит
                            {
                                Console.WriteLine($"{temp.Key}");
                                foreach (var row in temp)
                                {
                                    Console.WriteLine(
                                        $"\t*{row.VocabName.ToString()} {row.VocabMajorVer.ToString()}.{row.VocabMinorVer.ToString()}");
                                }
                            }
                            else //не входит.Ищем в таблице всех возможных словарей
                            {
                                cmd.CommandText = "select v.strName, v.nMajor, v.nMinor from re_vocabulary as v";
                                var rows = new List<string>();
                                using (var dr = cmd.ExecuteReader())
                                {
                                    while (dr.Read())
                                    {

                                        rows.Add($"{dr[0]} {dr[1]}.{dr[2]}");
                                    }
                                }

                                var listVocab = vocabAndNotFound["vocab"];
                                var listNotFound = vocabAndNotFound["NOT FOUND"];
                                if (rows.Contains($"{rule.Name} {rule.MajorRevision}.{rule.MinorRevision}")) // найдено в словарях
                                    listVocab.Add($"{rule.Name} {rule.MajorRevision}.{rule.MinorRevision}");
                                else
                                    listNotFound.Add($"{rule.Name} {rule.MajorRevision}.{rule.MinorRevision}"); //не найдено нигде
                            }
                        }
                    }
                    Console.WriteLine("*************************************");
                    foreach (var record in vocabAndNotFound)
                    {
                        var key = record.Key;
                        var value = record.Value;
                        foreach (var temp in value)
                        {
                            Console.WriteLine($"{temp}---{key}");
                        }
                    }
                }
                else //Отображает сначала ВСЕ переданные правила,затем ВСЕ словари
                {
                    var temp = new StringBuilder();
                    foreach (var rule in rules)
                    {
                        temp.Append($"(strName = '{rule.Name}' and nMajor={rule.MajorRevision} and nMinor={rule.MinorRevision})or");
                    }
                    var queryText = temp.ToString();
                    queryText = queryText.Substring(0, queryText.Length - 2);
                    var recList = new List<string>();
                    var cmd = df.CreateCommand();
                    if (cmd != null)
                    {
                        cmd.Connection = cn;
                        cmd.CommandText =
                            "select strName, nMajor, nMinor from re_ruleset where" + queryText;

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var rec = $"{dr[0]} {dr[1]}.{dr[2]}";
                                recList.Add(rec);
                                Console.WriteLine($"{dr[0]} {dr[1]}.{dr[2]}");
                            }
                            Console.WriteLine("*************************************");
                        }
                        cmd.CommandText = " select strName, nMajor, nMinor from re_vocabulary where" + queryText;
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var rec = $"{dr[0]} {dr[1]}.{dr[2]}";
                                recList.Add(rec);
                                Console.WriteLine($"{dr[0]} {dr[1]}.{dr[2]}");
                            }
                            Console.WriteLine("*************************************");
                        }
                        var strRule = new List<string>();
                        foreach (var rule in rules)
                        {
                            strRule.Add($"{rule.Name} {rule.MajorRevision}.{rule.MinorRevision}");
                        }
                        var notFoundItem = strRule.Except(recList);
                        foreach (var item in notFoundItem)
                        {
                            Console.WriteLine($"{item}---NOT FOUND");
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    }
}

