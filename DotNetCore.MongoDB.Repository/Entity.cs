using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace DotNETCore.Repository.Mongo
{
    /// <summary>
    /// mongo entity
    /// </summary>
    [BsonIgnoreExtraElements(Inherited = true)]
    public class Entity : IEntity
    {
        public Entity()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }

        /// <summary>
        /// create date
        /// </summary>
        public DateTime CreatedOn
        {
            get
            {
                return ObjectId.CreationTime;
            }
        }

        /// <summary>
        /// id in string format
        /// </summary>
        [JsonProperty(Order =1)]
        [BsonElement(Order = 0)]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        /// <summary>
        /// modify date
        /// </summary>
        [BsonElement("_m", Order = 1)]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        /// id in objectId format
        /// </summary>
        [JsonIgnore]
        public ObjectId ObjectId
        {
            get
            {
                //Incase, this is required if db record is null
                if (Id == null)
                    Id = ObjectId.GenerateNewId().ToString();
                return ObjectId.Parse(Id);
            }
        }


    }
}