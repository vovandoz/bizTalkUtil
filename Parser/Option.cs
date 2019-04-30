using System.Collections.Generic;
using CommandLine;

namespace Breutil.Parser
{
    class Options
    {
        [Option('d', "db-server", Required = true, HelpText = "Input db name to be processed.")]
        public string DbServerName { get; set; }

        [Option('r', "rule-names", Separator = ';', Required = false, HelpText = "Input rules names to be processed.")]
        public IEnumerable<string> RuleNamesAndVersion { get; set; }

        [Option('m', "mode", Required = true, HelpText = "Input type of mode to be processed.")]
        public string ExportMode { get; set; }

        [Option('f', "folder", Required = false, HelpText = "Input path to folder to be processed.")]
        public string Path { get; set; }

        [Option('n', "db-name", Required = false, Default = "BizTalkRuleEngineDb", HelpText = "Input db name to be processed.")]
        public string DbName { get; set; }

        [Option('w', "overwrite", Required = false, HelpText = "Input flag for write to be processed.")]
        public bool IsOverwrite { get; set; }

        [Option('e', "dependency", Required = false, HelpText = "Input flag for view dependency between rule and vocab to be processed.")]
        public bool Dependecy { get; set; }
    }
}
