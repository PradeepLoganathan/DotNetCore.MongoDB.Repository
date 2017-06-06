using System;
using System.Collections.Generic;
using DotNetCore.MongoDB.Repository.Tracking;

namespace DotNetCore.MongoDB.Repository.Tests
{
    public static class TestData
    {
        public static TestEntity InitialEntity { get; set; } = new TestEntity
        {
            Revision = new RevisionRecord<DateTime>
            {
                Version = DateTime.Now.AddDays(-10).Date,
                Date = DateTime.Now.AddDays(-10).Date
            },
            StringProperty = "Initial",
            IntProperty = 2,
            CollectionProperty = new List<string>
            {
                "string1"
            }
        };

        public static TestEntity FirstUpdate { get; set; } = new TestEntity
        {
            Revision = new RevisionRecord<DateTime>
            {
                Version = DateTime.Now.AddDays(-5).Date,
                Date = DateTime.Now.AddDays(-5).Date
            },
            IntProperty = 5
        };

        public static TestEntity SecondUpdate { get; set; } = new TestEntity
        {
            Revision = new RevisionRecord<DateTime>
            {
                Version = DateTime.Now.AddDays(-2).Date,
                Date = DateTime.Now.AddDays(-2).Date
            },
            StringProperty = "Final",
            IntProperty = 5,
            CollectionProperty = new List<string>
            {
                "string1",
                "string2"
            }
        };
    }

    public class TestEntity : TrackableEntity<DateTime>
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
        public List<string> CollectionProperty { get; set; }
    }

    public class TestEntityTracked : TrackedEntity<TestEntity, DateTime>
    {
        public TestEntityTracked()
        {
        }

        public TestEntityTracked(TestEntity initialValue) : base(initialValue)
        {
        }

        public TrackedProperty<string, DateTime> StringProperty { get; set; }
        public TrackedProperty<int, DateTime> IntProperty { get; set; }
        public TrackedColllectionProperty<string, DateTime> CollectionProperty { get; set; }
    }
}