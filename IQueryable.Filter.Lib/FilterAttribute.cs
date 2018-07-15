using System;

namespace IQueryable.Filter.Lib
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class FilterAttribute : Attribute
    {
        public string FieldName { get; set; }
    }
}