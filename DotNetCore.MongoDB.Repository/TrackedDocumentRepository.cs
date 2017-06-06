using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotNetCore.MongoDB.Repository.Tracking;
using DotNETCore.Repository.Mongo;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Polly;

namespace DotNetCore.MongoDB.Repository
{
    public class TrackedDocumentRepository<TTracked, TTrackable, TVersion>
        where TTrackable : TrackableEntity<TVersion>, new()
        where TVersion : IComparable, new()
        where TTracked : TrackedEntity<TTrackable, TVersion>, new()
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        private readonly Lazy<PropertyInfo[]> _trackedProperties = new Lazy<PropertyInfo[]>(() =>
            typeof(TTracked)
                .GetProperties()
                .Where(x =>
                    typeof(ITrackedProperty).IsAssignableFrom(x.PropertyType)).ToArray()
        );

        public TrackedDocumentRepository(IConfiguration configuration)
        {
            Collection = Database<TTracked>.GetCollection(configuration);
            StandardRepository = new Repository<TTracked>(configuration);
            _configuration = configuration;
        }

        public TrackedDocumentRepository(string connectionString)
        {
            Collection = Database<TTracked>.GetCollectionFromConnectionString(connectionString);
            StandardRepository = new Repository<TTracked>(connectionString);
            _connectionString = connectionString;
        }

        public Repository<TTracked> StandardRepository { get; set; }

        #region Projections

        public ProjectionDefinition<TTracked> VersionProject(TVersion version = default(TVersion))
        {
            var result =
                StandardRepository.Project.Slice(
                    new StringFieldDefinition<TTracked>(_trackedProperties.Value.First().Name), -1);


            foreach (var prop in _trackedProperties.Value)
            {
                var propertyName = new StringFieldDefinition<TTracked>(prop.Name);
                result = result.Slice(propertyName, -1);

                if (version == null || version.Equals(default(TVersion))) continue;

                var filterDefinition = Filter.AnyLte(propertyName, version);
                result = result.ElemMatch(propertyName, filterDefinition);
            }

            return result;
        }

        #endregion

        #region Insert

        /// <summary>
        ///     insert entity
        /// </summary>
        /// <param name="entity">entity</param>
        public virtual void Insert(TTrackable entity)
        {
            var tracked = new TTracked();
            tracked.Initialize(entity);


            Retry(() =>
            {
                Collection.InsertOne(tracked);
                return true;
            });
        }

        #endregion Insert

        #region Update

        /// <summary>
        ///     update a property field in an entity
        /// </summary>
        /// <returns>true if successful, otherwise false</returns>
        public void Update(TTrackable updatedEntity)
        {
            var data = StandardRepository.Get(updatedEntity.Id);
            data.AddRevision(updatedEntity);
            StandardRepository.Replace(data);
        }

        #endregion Update

        #region RetryPolicy

        /// <summary>
        ///     retry operation for three times if IOException occurs
        /// </summary>
        /// <typeparam name="TResult">return type</typeparam>
        /// <param name="action">action</param>
        /// <returns>action result</returns>
        /// <example>
        ///     return Retry(() =>
        ///     {
        ///     do_something;
        ///     return something;
        ///     });
        /// </example>
        protected virtual TResult Retry<TResult>(Func<TResult> action)
        {
            return Policy
                .Handle<MongoConnectionException>(i => i.InnerException.GetType() == typeof(IOException))
                .Retry(3)
                .Execute(action);
        }

        #endregion

        #region MongoSpecific

        /// <summary>
        ///     mongo collection
        /// </summary>
        public IMongoCollection<TTracked> Collection { get; protected set; }


        /// <summary>
        ///     filter for collection
        /// </summary>
        public FilterDefinitionBuilder<TTracked> Filter
            => Builders<TTracked>.Filter;

        /// <summary>
        ///     projector for collection
        /// </summary>
        public ProjectionDefinitionBuilder<TTracked> Project
            => Builders<TTracked>.Projection;

        /// <summary>
        ///     updater for collection
        /// </summary>
        public UpdateDefinitionBuilder<TTracked> Updater
            => Builders<TTracked>.Update;

        protected IFindFluent<TTracked, TTracked> Query(Expression<Func<TTracked, bool>> filter)
        {
            return Collection.Find(filter);
        }


        protected IFindFluent<TTracked, TTracked> Query()
        {
            return Collection.Find(Filter.Empty);
        }

        #endregion MongoSpecific

        #region Deserializaion

        private TTrackable Deserialize(BsonDocument document)
        {
            return BsonSerializer.Deserialize<TTrackable>(document);
        }

        private static IEnumerable<TTrackable> Deserialize(IEnumerable<BsonDocument> document)
        {
            return document.Select(x => BsonSerializer.Deserialize<TTracked>(x).GetRevision());
        }

        #endregion

        #region Get

        /// <summary>
        ///     get by id
        /// </summary>
        /// <param name="id">id value</param>
        /// <returns>entity of <typeparamref name="TTrackable" /></returns>
        public virtual TTrackable Get(string id)
        {
            return Retry(() => { return Find(i => i.Id == id).FirstOrDefault(); });
        }

        /// <summary>
        ///     get by id
        /// </summary>
        /// <param name="id">id value</param>
        /// <param name="version"></param>
        /// <returns>entity of <typeparamref name="TTracked" /></returns>
        public virtual TTrackable Get(string id, TVersion version)
        {
            return Retry(() => { return Find(i => i.Id == id, version).FirstOrDefault(); });
        }

        #endregion Get

        #region Find

        /// <summary>
        ///     find entities
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="version"></param>
        /// <returns>collection of entity</returns>
        public virtual IEnumerable<TTrackable> Find(Expression<Func<TTracked, bool>> filter,
            TVersion version = default(TVersion))
        {
            return Deserialize(Query(filter).Project(VersionProject(version)).ToList());
        }


        /// <summary>
        ///     find entities with paging and ordering in direction
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="order">ordering parameters</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <param name="isDescending">ordering direction</param>
        /// <param name="version"></param>
        /// <returns>collection of entity</returns>
        public virtual IEnumerable<TTrackable> Find(Expression<Func<TTracked, bool>> filter,
            Expression<Func<TTracked, object>> order, int pageIndex, int size, bool isDescending,
            TVersion version = default(TVersion))
        {
            return Retry(() =>
            {
                var query = Query(filter).Project(VersionProject(version)).Skip(pageIndex * size).Limit(size);

                var result = isDescending ? query.SortByDescending(order) : query.SortBy(order);

                return Deserialize(result.ToList());
            });
        }

        #endregion Find

        #region FindAll

        /// <summary>
        ///     fetch all items in collection
        /// </summary>
        /// <returns>collection of entity</returns>
        public virtual IEnumerable<TTrackable> FindAll(TVersion version)
        {
            return Retry(() => Deserialize(Query().Project(VersionProject(version)).ToList()));
        }

        /// <summary>
        ///     fetch all items in collection
        /// </summary>
        /// <returns>collection of entity</returns>
        public IEnumerable<TTrackable> FindAll()
        {
            return Retry(() => Deserialize(Query().Project(VersionProject()).ToList()));
        }

        /// <summary>
        ///     fetch all items in collection with paging
        /// </summary>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <param name="version"></param>
        /// <returns>collection of entity</returns>
        public IEnumerable<TTrackable> FindAll(int pageIndex, int size, TVersion version = default(TVersion))
        {
            return FindAll(i => i.Id, pageIndex, size, version);
        }

        /// <summary>
        ///     fetch all items in collection with paging and ordering
        ///     default ordering is descending
        /// </summary>
        /// <param name="order">ordering parameters</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <param name="version"></param>
        /// <returns>collection of entity</returns>
        public IEnumerable<TTrackable> FindAll(Expression<Func<TTracked, object>> order, int pageIndex, int size,
            TVersion version = default(TVersion))
        {
            return FindAll(order, pageIndex, size, true, version);
        }

        /// <summary>
        ///     fetch all items in collection with paging and ordering in direction
        /// </summary>
        /// <param name="order">ordering parameters</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <param name="isDescending">ordering direction</param>
        /// <param name="version"></param>
        /// <returns>collection of entity</returns>
        public virtual IEnumerable<TTrackable> FindAll(Expression<Func<TTracked, object>> order, int pageIndex,
            int size,
            bool isDescending, TVersion version = default(TVersion))
        {
            return Retry(() =>
            {
                var query = Query().Skip(pageIndex * size).Limit(size).Project(VersionProject(version));
                var result = isDescending ? query.SortByDescending(order) : query.SortBy(order);
                return Deserialize(result.ToList());
            });
        }

        #endregion FindAll
    }
}