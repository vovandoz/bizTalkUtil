
namespace Breutil.Model
{
    public  class BreRecord
    {
        public virtual int MajorRevision { get; set; }
        public virtual int MinorRevision { get; set; }
        public virtual string Name { get; set; }
        public string FullFileName { get; set; }
        public bool UseMaxVersion { get; set; }
    }
}
