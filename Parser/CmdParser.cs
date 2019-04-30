using System;
using System.Collections.Generic;
using System.Linq;
using Breutil.Model;
using CommandLine;

namespace Breutil.Parser
{
    public class CmdParser
    {
        public static Args Parse(string[] args)
        {
            var parsed = new Args();//объект,собирающий распарсенные данные
            var res = CommandLine.Parser.Default.ParseArguments<Options>(args)//метод внешней библиотеки производящий парсинг аргументов из CMD
                .WithNotParsed(errors => { parsed.Errors.AddRange(errors); });
            if (res is NotParsed<Options>)//если парсинг с ошибками - вывод в консоль сообщения об ошибке сгенерированый внешней библиотекой
                return parsed;
            var result = (Parsed<Options>)res;
            var mode = result.Value.ExportMode.ToLower();
            parsed.ServerName = result.Value.DbServerName;
            parsed.DbName = result.Value.DbName;
            parsed.Path = result.Value.Path;
            parsed.IsOverwrite = result.Value.IsOverwrite;
            parsed.IsDependecy = result.Value.Dependecy;
            parsed.RuleNames = result.Value.RuleNamesAndVersion.ToArray();
            Mode typeOfMode;
            if (Enum.TryParse(mode, true, out typeOfMode))//определение Mode
                parsed.Mode = typeOfMode;
            else
            {
                parsed.Errors.Add(new LogicErrorType(ErrorType.BadFormatTokenError));
                return parsed;
            }
            if (parsed.Path != null)
                parsed.Path = $@"{parsed.Path.ToLower()}";
            else
            {
                if (!(new List<Mode>() { Mode.ImportList, Mode.ShowList, Mode.ShowAll }.Contains(parsed.Mode)))
                {
                    parsed.Errors.Add(new LogicErrorType(ErrorType.BadVerbSelectedError));
                    return parsed;
                }
            }
            if (typeOfMode == Mode.ExportList || typeOfMode == Mode.ImportList || typeOfMode == Mode.ShowList)
            {
                if (result.Value.RuleNamesAndVersion.ToArray().Length == 0)
                {
                    parsed.Errors.Add(new LogicErrorType(ErrorType.UnknownOptionError));
                    return parsed;
                }
            }
            return parsed;
        }
    }
}
