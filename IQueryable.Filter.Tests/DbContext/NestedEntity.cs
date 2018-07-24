using IQueryable.Filter.Lib;

namespace IQueryable.Filter.Tests
{
    public class NestedEntity
    {
        public int Id { get; set; }
        [Filter(FieldName = "bar")]
        public int Bar { get; set; }
    }
}