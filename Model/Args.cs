using System.Collections.Generic;
using CommandLine;

namespace Breutil.Model
{
    public class Args
    {
        public string ServerName { get; set; }
        public string DbName { get; set; }
        public Mode Mode { get; set; }
        public List<Error> Errors { get; set; }
        public string[] RuleNames { get; set; }
        public string Path { get; set; }
        public bool IsSuccess => Errors.Count == 0;
        public bool IsOverwrite { get; set; }
        public bool IsDependecy { get; set; }
        public Args()
        {
            Errors = new List<Error>();
        }
    }
}
