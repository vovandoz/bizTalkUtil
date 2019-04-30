using System;
using Microsoft.RuleEngine;

namespace Breutil.Model
{
    public class VocabRecord : BreRecord
    {
        private readonly VocabularyInfo _vocabRecord;

        public VocabRecord(VocabularyInfo vocabRecord)
        {
            _vocabRecord = vocabRecord;
        }
        public VocabularyInfo VocabularyRecord => _vocabRecord;
        public override string Name
        {
            get { return _vocabRecord.Name; }
            set { throw new NotImplementedException(); }
        }

        public override int MajorRevision
        {
            get { return _vocabRecord.MajorRevision; }
            set { throw new NotImplementedException(); }
        }

        public override int MinorRevision
        {
            get { return _vocabRecord.MinorRevision; }
            set { throw new NotImplementedException(); }
        }
    }
}
