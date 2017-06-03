using System;
using System.Collections.Generic;
using System.Text;
using DotNetCore.MongoDB.Repository.Tracking;
using Xunit;

namespace DotNetCore.MongoDB.Repository.Tests
{
    public class TrackedPropertyTests
    {
        public static TrackedProperty<string, DateTime> TestProperty = new TrackedProperty<string, DateTime>()
        {
            new PropertyRevision<string,DateTime>()
            {
                Version = DateTime.Now.AddDays(-10).Date,
                Value = "Old"
            },

            new PropertyRevision<string,DateTime>()
            {
                Version = DateTime.Now.AddDays(-5).Date,
                Value = "Newer"
            },

            new PropertyRevision<string,DateTime>()
            {
                Version = DateTime.Now.Date,
                Value = "Newest"
            },
        };

        [Fact]
        public void CanGetLatestVersion()
        {
            Assert.Equal(TestProperty.GetValue(), "Newest");
            
        }

        [Fact]
        void CanGetSpecificVersion()
        {
            Assert.Equal("Newer", TestProperty.GetRevision(DateTime.Now.AddDays(-2).Date).GetValue());
            Assert.Equal("Old", TestProperty.GetRevision(DateTime.Now.AddDays(-10).Date).GetValue());
        }
         
    }
}
