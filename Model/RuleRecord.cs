using System;
using Microsoft.RuleEngine;

namespace Breutil.Model
{
    public class RuleRecord : BreRecord
    {
        private  RuleSetInfo _ruleSetInfo;

        public RuleRecord(RuleSetInfo ruleSetInfo)
        {
            _ruleSetInfo = ruleSetInfo;
        }

        public RuleSetInfo RuleSetInfo => _ruleSetInfo;

        public override string Name
        {
            get { return _ruleSetInfo.Name; }
            set { throw new NotImplementedException(); }
        }

        public override int MajorRevision
        {
            get { return _ruleSetInfo.MajorRevision; }
            set { throw new NotImplementedException(); }
        }

        public override int MinorRevision
        {
            get { return _ruleSetInfo.MinorRevision; }
            set { throw new NotImplementedException(); }
        }
    }
}
