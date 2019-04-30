using System.Collections.Generic;
using System.IO;
using Breutil.Helpers;
using Breutil.Model;
using Breutil.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommandLine;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Breutil.Tests //TODO Добавить тесты отображающие корректную работу программы
{
    [TestClass]
    public class CmdParserTest
    {
        [TestCleanup]
        public void TestCleanUp()
        {
            File.Delete(@"..\..\..\inputData\Policy.Test-1.0.xml");
            File.Delete(@"..\..\..\inputData\Policy.Test-1.1.xml");
            File.Delete(@"..\..\..\inputData\PolicyAA-1.0.xml");
            File.Delete(@"..\..\..\inputData\voc1-1.0.xml");
            File.Delete(@"..\..\..\inputData\voc2-1.0.xml");
        }

        [TestMethod]
        public void TestCommandLineParserWithotRequiredArgumentsError()//MissingRequiredOptionError
        {
            var args = new[] { "--db-server", "name", "--folder", "c://..." };
            var result = CmdParser.Parse(args);
            Assert.AreEqual(result.IsSuccess, false);

            var args2 = new[] { "--mode", "import-all", "--folder", "c://..." };
            result = CmdParser.Parse(args2);
            Assert.AreEqual(result.IsSuccess, false);

            var args3 = new[] { "--mode", "export-list" };
            result = CmdParser.Parse(args3);
            Assert.AreEqual(result.IsSuccess, false);
        }

        [TestMethod]
        public void TestCommandLineParserWrongNameArgumentsError()//UnknownOptionError
        {
            var args = new[] { "--db-server", "name", "--mode", "export-list", "--rule-na", "--folder", "c://..." };
            var result = CmdParser.Parse(args);
            Assert.AreEqual(result.IsSuccess, false);

            var args2 = new[] { "--db-server", "name", "--mod", "export-list", "--rule-names", "name,version;name", "--folder", "c://..." };
            var result2 = CmdParser.Parse(args2);
            Assert.AreEqual(result2.IsSuccess, false);
        }

        [TestMethod]
        public void TestCommandLineParserWithoutValueForRequiredArgumentsError()//MissingRequiredOptionError + MissingValueOptionError
        {
            var args = new[] { "--db-server", "name", "--mode" };
            var result = CmdParser.Parse(args);
            Assert.AreEqual(result.IsSuccess, false);
        }

        [TestMethod]
        public void TestCommandLineParserWithoutValueForArgumentError()
        {
            var args = new[] { "--db-server", "name", "--mode", "import-list", "--rule-names" };
            var result = CmdParser.Parse(args);
            Assert.AreEqual(result.IsSuccess, false);
        }
        [TestMethod]
        public void TestCommandLineParserWithoutRuleNamesError()
        {
            var args = new[] { "--db-server", "name", "--mode", "import-list", "--folder", "c://..." };
            var result = CmdParser.Parse(args);
            Assert.AreEqual(result.Errors.Contains(new LogicErrorType(ErrorType.UnknownOptionError)), true);

            var args2 = new[] { "--db-server", "name", "--mode", "export-list", "--folder", "c://..." };
            var result2 = CmdParser.Parse(args2);
            Assert.AreEqual(result2.Errors.Contains(new LogicErrorType(ErrorType.UnknownOptionError)), true);
        }

        [TestMethod]
        public void TestCommandLineParserWrongNameForExportModeValueError()
        {
            var args = new[] { "--db-server", "name", "--mode", "export-al", "--folder", "c://..." };
            var result = CmdParser.Parse(args);
            Assert.AreEqual(result.IsSuccess, false);

        }

        [TestMethod]
        public void TestCommandLineParserWithRequiredArguments()
        {
            var args = new[] { "--mode", "export-all", "--db-server", "name" };
            var result = CmdParser.Parse(args);
            var same = new Args
            {
                ServerName = "name",
                Mode = Mode.ExportAll
            };
            Assert.AreEqual(result.Mode, same.Mode);
            Assert.AreEqual(result.ServerName, same.ServerName);
            Assert.AreEqual(result.RuleNames.Length == 0, same.RuleNames.Length == 0);
        }

        [TestMethod]
        public void TestCommandLineParserWithAllArgumentstAndExampleOfWorkingExportList()
        {
            var args = new[] { "--m", "export-list", "--db-server","WRK-003", "--r", "aa,1.0;bb", "--f", @"c:\temp", "--w" };
            var result = CmdParser.Parse(args);
            var same = new Args
            {
                Mode = Mode.ExportList,
            };
            same.RuleNames.Add(new BreRecord()
            {
                FullFileName = $"aa-{1}.{0}.xml"
            });
            same.RuleNames.Add(new BreRecord()
            {
                UseMaxVersion = true
            });
            same.ServerName = "name";
            same.DbName = "BizTalkRuleEngineDb";
            same.Path = @"c:\temp\";
            same.IsOverwrite = true;
            Assert.AreEqual(result.ServerName, same.ServerName);
            Assert.AreEqual(result.DbName, same.DbName);
            Assert.AreEqual(result.Mode, same.Mode);
            Assert.AreEqual(result.Errors.Count == 0, same.Errors.Count == 0);
            Assert.AreEqual(result.Path, same.Path);
            Assert.AreEqual(result.IsOverwrite, same.IsOverwrite);
            for (var i = 0; i < result.RuleNames.Count; i++)
            {
                Assert.AreEqual(result.RuleNames[i].Name, same.RuleNames[i].Name);
                Assert.AreEqual(result.RuleNames[i].MajorRevision, same.RuleNames[i].MajorRevision);
                Assert.AreEqual(result.RuleNames[i].MinorRevision, same.RuleNames[i].MinorRevision);
                Assert.AreEqual(result.RuleNames[i].UseMaxVersion, same.RuleNames[i].UseMaxVersion);
                Assert.AreEqual(result.RuleNames[i].FullFileName, same.RuleNames[i].FullFileName);
            }
        }

        [TestMethod]
        public void TestCommandLineParserExampleOfWorkingOfImportList()
        {
            var args = new[] { "--m", "import-list", "--db-server", "WRK-003", "--r", @"C:\Users\v.michailov\RealProjects\biztalkutilites\outputData\file.xml" };
            var result = CmdParser.Parse(args);
            var same = new Args
            {
                Mode = Mode.ImportList,
            };
            same.RuleNames.Add(new BreRecord()
            {
                FullFileName = @"C:\temp\file.xml"
            });
            same.RuleNames.Add(new BreRecord()
            {
                FullFileName = @"C:\temp\file2.xml"
            });
            same.ServerName = "WRK-003";
            same.DbName = "BizTalkRuleEngineDb";
            Assert.AreEqual(result.ServerName, same.ServerName);
            Assert.AreEqual(result.DbName, same.DbName);
            Assert.AreEqual(result.Mode, same.Mode);
            Assert.AreEqual(result.Errors.Count == 0, same.Errors.Count == 0);
            Assert.AreEqual(result.Path, same.Path);
            Assert.AreEqual(result.IsOverwrite, same.IsOverwrite);
            for (var i = 0; i < result.RuleNames.Count; i++)
            {
                Assert.AreEqual(result.RuleNames[i].Name, same.RuleNames[i].Name);
                Assert.AreEqual(result.RuleNames[i].MajorRevision, same.RuleNames[i].MajorRevision);
                Assert.AreEqual(result.RuleNames[i].MinorRevision, same.RuleNames[i].MinorRevision);
                Assert.AreEqual(result.RuleNames[i].UseMaxVersion, same.RuleNames[i].UseMaxVersion);
                Assert.AreEqual(result.RuleNames[i].FullFileName, same.RuleNames[i].FullFileName);
            }
        }

        [TestMethod]
        public void TestCommandLineParserUsingMaxVersion()
        {
            var args = new[] { "--mode", "export-list", "--db-server", "name", "--rule-names", "rName1" };
            var result = CmdParser.Parse(args);
            var same = new Args
            {
                Mode = Mode.ExportList,
                BreRecords = new List<BreRecord>()
            };
            var rule = new BreRecord()
            {
                UseMaxVersion = true
            };
            same.RuleNames.Add(rule);
            Assert.AreEqual(result.Mode, same.Mode);
            for (var i = 0; i < result.RuleNames.Count; i++)
            {
                Assert.AreEqual(result.RuleNames[i].UseMaxVersion, same.RuleNames[i].UseMaxVersion);
            }
        }
        [TestMethod]
        public void TestCommandLineParserWithShortCommand()
        {
            var args = new[] { "--m", "export-list", "--d", "WRK-003", "--f", @"..\..\..\inputData", "--r", "voc1", "--n", "BizTalkRuleEngineDb", "--w" };
            var result = CmdParser.Parse(args);
            RuleReadWriter.Write(result.ServerName, result.DbName, result.RuleNames, result.Path, TODO);
            Assert.AreEqual(result.IsSuccess, true);
            File.Delete(@"..\..\..\inputData\voc1-1.0.xml");
        }
        [TestMethod]
        public void TestRuleReadWriterWriteWithExportAll()
        {
            var parsed = new Args
            {
                ServerName = "WRK-003",
                DbName = "BizTalkRuleEngineDb",
                Mode = Mode.ExportAll,
                IsOverwrite = true,
                Path = @"C:\Users\v.michailov\RealProjects\biztalkutilites\inputData"
            };
            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\Policy.Test-1.0.xml"));
            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\Policy.Test-1.1.xml"));
            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\PolicyAA-1.0.xml"));
            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\voc1-1.0.xml"));
            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\voc2-1.0.xml"));

            RuleReadWriter.Write(parsed.ServerName, parsed.DbName, parsed.RuleNames, parsed.Path, TODO);
            var timeBefore1 = File.GetLastAccessTime(@"..\..\..\inputData\Policy.Test-1.0.xml");
            var timeBefore2 = File.GetLastAccessTime(@"..\..\..\inputData\voc2-1.0.xml");


            RuleReadWriter.Write(parsed.ServerName, parsed.DbName, parsed.RuleNames, parsed.Path, TODO);
            var timeAfter1 = File.GetLastAccessTime(@"..\..\..\inputData\Policy.Test-1.0.xml");
            var timeAfter2 = File.GetLastAccessTime(@"..\..\..\inputData\voc2-1.0.xml");

            Assert.AreNotEqual(timeBefore1.Millisecond, timeAfter1.Millisecond);
            Assert.AreNotEqual(timeBefore2.Millisecond, timeAfter2.Millisecond);

            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\Policy.Test-1.0.xml"));
            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\Policy.Test-1.1.xml"));
            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\PolicyAA-1.0.xml"));
            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\voc1-1.0.xml"));
            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\voc2-1.0.xml"));

        }
        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void TestRuleReadWriterWriteWithExportAllOverwriteFalse()
        {
            BreRecord.Error += str => //СОБЫТИЕ НАСТУПАЕТ  при попытке перезаписи
            {
                throw new IOException();
            };
            var parsed = new Args
            {
                ServerName = "WRK-003",
                DbName = "BizTalkRuleEngineDb",
                Mode = Mode.ExportAll,
                IsOverwrite = false,    //изменен флаг перезаписи
                Path = @"C:\Users\v.michailov\RealProjects\biztalkutilites\inputData"
            };

            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\Policy.Test-1.0.xml"));
            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\Policy.Test-1.1.xml"));
            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\PolicyAA-1.0.xml"));
            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\voc1-1.0.xml"));
            Assert.AreEqual(false, File.Exists(@"..\..\..\inputData\voc2-1.0.xml"));

            RuleReadWriter.Write(parsed.ServerName, parsed.DbName, parsed.RuleNames, parsed.Path, TODO);

            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\Policy.Test-1.0.xml"));
            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\Policy.Test-1.1.xml"));
            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\PolicyAA-1.0.xml"));
            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\voc1-1.0.xml"));
            Assert.AreEqual(true, File.Exists(@"..\..\..\inputData\voc2-1.0.xml"));

            RuleReadWriter.Write(parsed.ServerName, parsed.DbName, parsed.RuleNames, parsed.Path, TODO);
        }
    }
}
