using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCore.MongoDB.Repository.Tracking;
using Xunit;

namespace DotNetCore.MongoDB.Repository.Tests
{
    

    public class TrackedEntityTests
    {
        
        [Fact]
        public void TrackedEntityCanInitFromTrackableEntity()
        {
            var trackable = new TestEntityTracked(TestData.InitialEntity);
            
            
            Assert.Equal("Initial", trackable.StringProperty.GetValue());
            Assert.Equal(2, trackable.IntProperty.GetValue());

            Assert.True(TestData.InitialEntity
                .CollectionProperty
                .SequenceEqual(
                    trackable
                        .CollectionProperty
                        .GetValue()));

        }


        

        [Fact]
        public void TrackedEntityCanGetLatestVersion()
        {
            var trackable = new TestEntityTracked(TestData.InitialEntity);
            trackable.AddRevision(TestData.FirstUpdate);
            trackable.AddRevision(TestData.SecondUpdate);


            Assert.Equal("Final", trackable.StringProperty.GetValue());
            Assert.Equal(5, trackable.IntProperty.GetValue());
        }


        [Fact]
        public void TrackedEntityCanGetIntermediateVersionWhenNotExact()
        {

            var trackable = new TestEntityTracked(TestData.InitialEntity);

            trackable.AddRevision(TestData.FirstUpdate);
            trackable.AddRevision(TestData.SecondUpdate);

            Assert.Equal(null, trackable.StringProperty.GetValue(DateTime.Now.AddDays(-3).Date));
            Assert.Equal(5, trackable.IntProperty.GetValue(DateTime.Now.AddDays(-3).Date));
        }

        [Fact]
        public void TrackedEntityCanGetIntermediateVersionWhenExact()
        {

            var trackable = new TestEntityTracked(TestData.InitialEntity);

            trackable.AddRevision(TestData.FirstUpdate);
            trackable.AddRevision(TestData.SecondUpdate);

            Assert.Equal("Initial", trackable.StringProperty.GetValue(DateTime.Now.AddDays(-10).Date));
            Assert.Equal(2, trackable.IntProperty.GetValue(DateTime.Now.AddDays(-10).Date));
        }

        [Fact]
        public void TrackedEntitySupportsCollectionProperty()
        {
            var trackable = new TestEntityTracked(TestData.InitialEntity);

            trackable.AddRevision(TestData.FirstUpdate);
            trackable.AddRevision(TestData.SecondUpdate);

            

            Assert.True(
                trackable
                .CollectionProperty
                .GetValue(DateTime.Now.AddDays(-10).Date)
                .SequenceEqual(TestData.InitialEntity.CollectionProperty));

            Assert.Equal(null, trackable
                .CollectionProperty
                .GetValue(DateTime.Now.AddDays(-5).Date));

            Assert.True(
                trackable
                .CollectionProperty
                .GetValue(DateTime.Now.AddDays(-2).Date)
                .SequenceEqual(TestData.SecondUpdate.CollectionProperty));


        }

        [Fact]
        public void TrackedEntityShouldThrowExceptionOnOldVersionInsertion()
        {
            var trackable = new TestEntityTracked(TestData.InitialEntity);

            trackable.AddRevision(TestData.FirstUpdate);
            trackable.AddRevision(TestData.SecondUpdate);

            Assert.Throws<OldVersionInsertionException>(()=>trackable.AddRevision(TestData.FirstUpdate));
        }

        [Fact]
        public void TrackedEntityShouldThrowExceptionOnCurrentVersionInsertion()
        {
            var trackable = new TestEntityTracked(TestData.InitialEntity);

            trackable.AddRevision(TestData.FirstUpdate);
            trackable.AddRevision(TestData.SecondUpdate);

            Assert.Throws<OldVersionInsertionException>(() => trackable.AddRevision(TestData.SecondUpdate));
        }
    }
}
