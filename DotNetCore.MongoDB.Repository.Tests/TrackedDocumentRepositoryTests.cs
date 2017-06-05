using System;
using System.Linq;
using Xunit;

namespace DotNetCore.MongoDB.Repository.Tests
{
    public class TrackedDocumentRepositoryTests
    {
        public TrackedDocumentRepositoryTests()
        {
            Repository = new TrackedDocumentRepository<
                TestEntityTracked,
                TestEntity,
                DateTime>("mongodb://localhost:27017/testDb");

            Repository.Collection.Database.DropCollection(Repository.Collection.CollectionNamespace.CollectionName);

            Repository.Insert(TestData.InitialEntity);
        }

        public TrackedDocumentRepository<
            TestEntityTracked,
            TestEntity,
            DateTime> Repository { get; set; }

        [Fact]
        public void TrackedDocumentInsertedProperly()
        {
            var all = Repository.FindAll().ToList();


            Assert.True(TestData.InitialEntity.CollectionProperty.SequenceEqual(all.Last().CollectionProperty));
            Assert.Equal(TestData.InitialEntity.StringProperty, all.Last().StringProperty);
            Assert.Equal(TestData.InitialEntity.IntProperty, all.Last().IntProperty);
        }


        [Fact]
        public void TrackedDocumentUpdates()
        {
            var all = Repository.FindAll().ToList();

            var doc = all.Last();

            TestData.FirstUpdate.Id = doc.Id;

            Repository.Update(TestData.FirstUpdate);
            var updatedDoc = Repository.Get(doc.Id);

            Assert.Equal(null, updatedDoc.CollectionProperty);
            Assert.Equal(TestData.FirstUpdate.StringProperty, updatedDoc.StringProperty);
            Assert.Equal(TestData.FirstUpdate.IntProperty, updatedDoc.IntProperty);

            TestData.SecondUpdate.Id = doc.Id;
            Repository.Update(TestData.SecondUpdate);
            updatedDoc = Repository.Get(doc.Id);

            Assert.True(TestData.SecondUpdate.CollectionProperty.SequenceEqual(updatedDoc.CollectionProperty));
            Assert.Equal(TestData.SecondUpdate.StringProperty, updatedDoc.StringProperty);
            Assert.Equal(TestData.SecondUpdate.IntProperty, updatedDoc.IntProperty);
        }
    }
}