namespace IQueryable.Filter.Lib
{
    public class FilterCondition
    {
        public string FieldName { get; set; }
        public object Value { get; set; }
        public FilterPredicates? Predicate { get; set; }
    }
}