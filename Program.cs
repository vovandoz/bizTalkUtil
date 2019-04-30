using System;
using System.Collections.Generic;
using Breutil.Helpers;
using Breutil.Model;
using Breutil.Parser;
using CommandLine;


namespace Breutil
{
    public class Program
    {
        public static void ShowMsqError(string msg)
        {
            Console.WriteLine(msg);
            Console.ReadLine();
            Environment.Exit(1);
        }
        private static void Main(string[] args)
        {
            var files = new List<BreRecord>();
            RuleReadWriter.Error += ShowMsqError;
            var parseArgs = CmdParser.Parse(args);
            if (!parseArgs.IsSuccess)
            {
                foreach (var error in parseArgs.Errors)
                {
                    switch (error.Tag)
                    {
                        case ErrorType.UnknownOptionError:
                            foreach (var err in parseArgs.Errors)
                            {
                                if (!(err is LogicErrorType)) continue;
                                Console.WriteLine("отсутствует --rule-names");
                                Console.ReadLine();
                            }
                            break;
                        case ErrorType.BadFormatTokenError:
                            Console.WriteLine("Ошибка имени значения ключа --mode");
                            Console.ReadLine();
                            break;
                        case ErrorType.BadVerbSelectedError:
                            Console.WriteLine("не указан путь");
                            Console.ReadLine();
                            break;
                        default:
                            throw new Exception("Неизвестная ошибка");
                    }
                }
            }
            switch (parseArgs.Mode)
            {
                case Mode.ExportAll:
                    files = RuleReadWriter.CreateFileForWriteAll(parseArgs.ServerName, parseArgs.DbName);
                    RuleReadWriter.Write(parseArgs.ServerName, parseArgs.DbName, files, parseArgs.Path, parseArgs.IsOverwrite);
                    break;
                case Mode.ExportList:
                    foreach (var name in parseArgs.RuleNames)
                    {
                        var fileList = RuleReadWriter.CreateFileForWriteList(parseArgs.ServerName, parseArgs.DbName, RuleReadWriter.CreateRecord(name));
                        foreach (var file in fileList)
                            files.Add(file);
                    }
                    RuleReadWriter.Write(parseArgs.ServerName, parseArgs.DbName, files, parseArgs.Path, parseArgs.IsOverwrite);
                    break;
                case Mode.ImportAll:
                    RuleReadWriter.Read(parseArgs.ServerName, parseArgs.DbName, RuleReadWriter.CreateFileForReadAll(parseArgs.Path));
                    break;
                case Mode.ImportList:
                    foreach (var name in parseArgs.RuleNames)
                    {
                        files.Add(RuleReadWriter.CreateFileForReadList(name));
                    }
                    RuleReadWriter.Read(parseArgs.ServerName, parseArgs.DbName, files);
                    break;
                case Mode.ShowList:
                    foreach (var name in parseArgs.RuleNames)
                    {
                        files.Add(RuleReadWriter.CreateRecord(name));
                    }
                    RuleEngineDb.ShowListRuleAndVocab(files, parseArgs.IsDependecy);
                    break;
                case Mode.ShowAll:
                    RuleEngineDb.ShowAllRuleAndVocab(parseArgs.IsDependecy);
                    break;
            }
        }
    }
}
