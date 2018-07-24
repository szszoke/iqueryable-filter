using System;
using IQueryable.Filter.Lib;

namespace IQueryable.Filter.Tests
{
    public class FilterEntity
    {
        public int Id { get; set; }

        [Filter(FieldName = "intField")]
        public int IntField { get; set; }

        [Filter(FieldName = "stringField")]
        public string StringField { get; set; }
        [Filter(FieldName = "doubleField")]
        public double DoubleField { get; set; }
        [Filter(FieldName = "dateTimeField")]
        public DateTime DateTimeField { get; set; }
        [Filter(FieldName = "foo")]
        public NestedEntity NestedEntity { get; set; }
    }
}