using System.Reflection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DotNETCore.Repository.Mongo
{
    internal class Database<T> where T : IEntity
    {
        private Database()
        {
        }

        internal static IMongoCollection<T> GetCollection(IConfiguration configuration)
        {
            return GetCollectionFromConnectionString(GetDefaultConnectionString(configuration));
        }

        /// <summary>
        ///     Creates and returns a MongoCollection from the specified type and connectionstring.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="connectionString">The connectionstring to use to get the collection from.</param>
        /// <returns>Returns a MongoCollection from the specified type and connectionstring.</returns>
        internal static IMongoCollection<T> GetCollectionFromConnectionString(string connectionString)
        {
            return GetCollectionFromConnectionString(connectionString, GetCollectionName());
        }

        /// <summary>
        ///     Creates and returns a MongoCollection from the specified type and connectionstring.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="connectionString">The connectionstring to use to get the collection from.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        /// <returns>Returns a MongoCollection from the specified type and connectionstring.</returns>
        internal static IMongoCollection<T> GetCollectionFromConnectionString(string connectionString,
            string collectionName)
        {
            return GetCollectionFromUrl(new MongoUrl(connectionString), collectionName);
        }

        /// <summary>
        ///     Creates and returns a MongoCollection from the specified type and url.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="url">The url to use to get the collection from.</param>
        /// <returns>Returns a MongoCollection from the specified type and url.</returns>
        internal static IMongoCollection<T> GetCollectionFromUrl(MongoUrl url)
        {
            return GetCollectionFromUrl(url, GetCollectionName());
        }

        /// <summary>
        ///     Creates and returns a MongoCollection from the specified type and url.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="url">The url to use to get the collection from.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        /// <returns>Returns a MongoCollection from the specified type and url.</returns>
        internal static IMongoCollection<T> GetCollectionFromUrl(MongoUrl url, string collectionName)
        {
            return GetDatabaseFromUrl(url).GetCollection<T>(collectionName);
        }

        /// <summary>
        ///     Creates and returns a MongoDatabase from the specified url.
        /// </summary>
        /// <param name="url">The url to use to get the database from.</param>
        /// <returns>Returns a MongoDatabase from the specified url.</returns>
        private static IMongoDatabase GetDatabaseFromUrl(MongoUrl url)
        {
            var client = new MongoClient(url);
            return client.GetDatabase(url.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        #region Connection String

        /// <summary>
        ///     Retrieves the default connectionstring from the App.config or Web.config file.
        /// </summary>
        /// <returns>Returns the default connectionstring from the App.config or Web.config file.</returns>
        internal static string GetDefaultConnectionString(IConfiguration configuration)
        {
            return configuration.GetConnectionString(GetConnectionName());
        }

        #endregion Connection String

        #region Collection Name

        /// <summary>
        ///     Determines the collection name for T and assures it is not empty
        /// </summary>
        /// <typeparam name="T">The type to determine the collection name for.</typeparam>
        /// <returns>Returns the collection name for T.</returns>
        private static string GetCollectionName()
        {
            var collectionName = typeof(T).GetTypeInfo().BaseType.Equals(typeof(object))
                ? GetCollectionNameFromInterface()
                : GetCollectionNameFromType();

            if (string.IsNullOrEmpty(collectionName))
                collectionName = typeof(T).Name;
            return collectionName.ToLowerInvariant();
        }

        /// <summary>
        ///     Determines the collection name from the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get the collection name from.</typeparam>
        /// <returns>Returns the collection name from the specified type.</returns>
        private static string GetCollectionNameFromInterface()
        {
            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = typeof(T).GetTypeInfo().Assembly.GetCustomAttribute<CollectionNameAttribute>();
            return att != null ? att.Name : typeof(T).Name;
        }

        /// <summary>
        ///     Determines the collectionname from the specified type.
        /// </summary>
        /// <returns>Returns the collectionname from the specified type.</returns>
        private static string GetCollectionNameFromType()
        {
            var entitytype = typeof(T);
            string collectionname;

            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = typeof(T).GetTypeInfo().Assembly.GetCustomAttribute<CollectionNameAttribute>();
            if (att != null)
                collectionname = att.Name;
            else
                collectionname = entitytype.Name;

            return collectionname;
        }

        #endregion Collection Name

        #region Connection Name

        /// <summary>
        ///     Determines the connection name for T and assures it is not empty
        /// </summary>
        /// <typeparam name="T">The type to determine the connection name for.</typeparam>
        /// <returns>Returns the connection name for T.</returns>
        private static string GetConnectionName()
        {
            var collectionName = typeof(T).GetTypeInfo().BaseType.Equals(typeof(object))
                ? GetConnectionNameFromInterface()
                : GetConnectionNameFromType();

            if (string.IsNullOrEmpty(collectionName))
                collectionName = typeof(T).Name;
            return collectionName.ToLowerInvariant();
        }

        /// <summary>
        ///     Determines the connection name from the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get the connection name from.</typeparam>
        /// <returns>Returns the connection name from the specified type.</returns>
        private static string GetConnectionNameFromInterface()
        {
            // Check to see if the object (inherited from Entity) has a ConnectionName attribute
            var att = typeof(T).GetTypeInfo().GetCustomAttribute<ConnectionNameAttribute>();

            return att != null ? att.Name : typeof(T).Name;
        }

        /// <summary>
        ///     Determines the connection name from the specified type.
        /// </summary>
        /// <returns>Returns the connection name from the specified type.</returns>
        private static string GetConnectionNameFromType()
        {
            var entitytype = typeof(T);
            string collectionname;

            // Check to see if the object (inherited from Entity) has a ConnectionName attribute

            var att = typeof(T).GetTypeInfo().GetCustomAttribute<ConnectionNameAttribute>();


            if (att != null)
            {
                // It does! Return the value specified by the ConnectionName attribute
                collectionname = att.Name;
            }
            else
            {
                if (typeof(Entity).GetTypeInfo().IsAssignableFrom(entitytype))
                    while (!entitytype.GetTypeInfo().BaseType.Equals(typeof(Entity)))
                        entitytype = entitytype.GetTypeInfo().BaseType;
                collectionname = entitytype.Name;
            }

            return collectionname;
        }

        #endregion Connection Name
    }
}