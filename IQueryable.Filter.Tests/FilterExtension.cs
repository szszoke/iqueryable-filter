using System.Collections.Generic;
using System.Linq;
using IQueryable.Filter.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Xunit;

namespace IQueryable.Filter.Tests
{
    public class FilterExtension
    {
        [Fact]
        public void Contains()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "Contains_Database")
                .Options;

            using (var context = new TestDbContext(options))
            {
                var entities = context.FilterEntities
                    .Filter(new List<FilterCondition>
                    {
                        new FilterCondition
                        {
                            FieldName = "stringField",
                            Value = 1,
                            Predicate = FilterPredicates.Contains,
                        },
                    })
                    .ToList();

                Assert.Empty(entities);
            }
        }

        [Fact]
        public void StartsWith()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "StartsWith_Database")
                .Options;

            using (var context = new TestDbContext(options))
            {
                var entities = context.FilterEntities
                    .Filter(new List<FilterCondition>
                    {
                        new FilterCondition
                        {
                            FieldName = "stringField",
                            Value = 1,
                            Predicate = FilterPredicates.StartsWith,
                        },
                        new FilterCondition
                        {
                            FieldName = "intField",
                            Value = 1,
                            Predicate = FilterPredicates.StartsWith,
                        },
                    })
                    .ToList();

                Assert.Empty(entities);
            }
        }
    }
}