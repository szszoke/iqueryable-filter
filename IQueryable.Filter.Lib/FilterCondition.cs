using System.Collections.Generic;

namespace IQueryable.Filter.Lib
{
    public class FilterCondition
    {
        public string FieldName { get; set; }
        public object Value { get; set; }
        public List<object> Values { get; set; }
        public FilterPredicates? Predicate { get; set; }
    }
}