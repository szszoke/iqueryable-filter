using System;
using System.Collections.Generic;
using System.Linq;
using IQueryable.Filter.Lib;
using Microsoft.EntityFrameworkCore;
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

        [Fact]
        public void Nested()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "Nested_Database")
                .Options;

            using (var context = new TestDbContext(options))
            {
                context.FilterEntities.Add(new FilterEntity
                {
                    IntField = 1,
                    StringField = "",
                    DoubleField = 1.0,
                    DateTimeField = DateTime.Now,
                    NestedEntity = new NestedEntity {
                        Bar = 1,
                    }
                });
                context.FilterEntities.Add(new FilterEntity
                {
                    IntField = 2,
                    StringField = "",
                    DoubleField = 2.0,
                    DateTimeField = DateTime.Now,
                    NestedEntity = new NestedEntity {
                        Bar = 2,
                    }
                });

                context.SaveChanges();

                var entities = context.FilterEntities
                    .Filter(new List<FilterCondition>
                    {
                        new FilterCondition
                        {
                            FieldName = "foo.bar",
                            Value = 1,
                            Predicate = FilterPredicates.Equal,
                        },
                    })
                    .ToList();

                Assert.Single(entities);
            }
        }

        [Fact]
        public void OneOf()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "OneOfNested_Database")
                .Options;

            using (var context = new TestDbContext(options))
            {
                context.FilterEntities.Add(new FilterEntity
                {
                    IntField = 1,
                    StringField = "1",
                    DoubleField = 1.0,
                    DateTimeField = DateTime.Now,
                });
                context.FilterEntities.Add(new FilterEntity
                {
                    IntField = 2,
                    StringField = "2",
                    DoubleField = 2.0,
                    DateTimeField = DateTime.Now,
                });
                context.FilterEntities.Add(new FilterEntity
                {
                    IntField = 3,
                    StringField = "3",
                    DoubleField = 3.0,
                    DateTimeField = DateTime.Now,
                });

                context.SaveChanges();

                var entities = context.FilterEntities
                    .Filter(new List<FilterCondition>
                    {
                        new FilterCondition
                        {
                            FieldName = "stringField",
                            Values = new List<object> { 1, 2 },
                            Predicate = FilterPredicates.OneOf,
                        },
                    })
                    .ToList();

                Assert.Equal(2, entities.Count);
            }
        }

        [Fact]
        public void OneOfNested()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "OneOfNested_Database")
                .Options;

            using (var context = new TestDbContext(options))
            {
                context.FilterEntities.Add(new FilterEntity
                {
                    IntField = 1,
                    StringField = "",
                    DoubleField = 1.0,
                    DateTimeField = DateTime.Now,
                    NestedEntity = new NestedEntity {
                        Bar = 1,
                    }
                });
                context.FilterEntities.Add(new FilterEntity
                {
                    IntField = 2,
                    StringField = "",
                    DoubleField = 2.0,
                    DateTimeField = DateTime.Now,
                    NestedEntity = new NestedEntity {
                        Bar = 2,
                    }
                });
                context.FilterEntities.Add(new FilterEntity
                {
                    IntField = 3,
                    StringField = "",
                    DoubleField = 3.0,
                    DateTimeField = DateTime.Now,
                    NestedEntity = new NestedEntity {
                        Bar = 3,
                    }
                });

                context.SaveChanges();

                var entities = context.FilterEntities
                    .Filter(new List<FilterCondition>
                    {
                        new FilterCondition
                        {
                            FieldName = "foo.bar",
                            Values = new List<object> { 1, 2 },
                            Predicate = FilterPredicates.OneOf,
                        },
                    })
                    .ToList();

                Assert.Equal(2, entities.Count);
            }
        }
    }
}