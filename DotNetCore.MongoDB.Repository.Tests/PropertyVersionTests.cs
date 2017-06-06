using System;
using System.Collections.Generic;
using DotNetCore.MongoDB.Repository.Tracking;
using Xunit;

namespace DotNetCore.MongoDB.Repository.Tests
{
    public class PropertyVersionTests
    {
        [Fact]
        public void IntPropertyVersionMapsToValue()
        {
            var intPropertyVersion = new PropertyRevision<int, DateTime>
            {
                Version = DateTime.Now,
                Value = 123
            };

            Assert.Equal(intPropertyVersion.Value, intPropertyVersion.GetValue());
        }

        [Fact]
        public void StringListPropertyVErsionMapsToValue()
        {
            var stringListPropertyVersion = new PropertyRevision<IEnumerable<string>, DateTime>
            {
                Version = DateTime.Now,
                Value = new List<string>
                {
                    "string1",
                    "string2",
                    "string3"
                }
            };

            Assert.Equal(stringListPropertyVersion.GetValue(), stringListPropertyVersion.Value);
        }

        [Fact]
        public void StringPropertyVersionMapsToValue()
        {
            var stringPropertyVersion = new PropertyRevision<string, DateTime>
            {
                Version = DateTime.Now,
                Value = "TestValue"
            };

            Assert.Equal(stringPropertyVersion.Value, stringPropertyVersion.GetValue());
        }
    }
}