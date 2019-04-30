using CommandLine;

namespace Breutil.Parser
{
    public class LogicErrorType : Error
    {
        public LogicErrorType(ErrorType tag) : base(tag)
        {
        }
    }
}
