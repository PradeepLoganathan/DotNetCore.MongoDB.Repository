using System;
using DotNETCore.Repository.Mongo;

namespace DotNetCore.MongoDB.Repository.Tracking
{
    public class TrackableEntity<TVersion> : Entity, ITrackableEntity<TVersion>
        where TVersion : IComparable
    {
        public RevisionRecord<TVersion> Revision { get; set; }
    }

    public interface ITrackableEntity<TVersion> : IEntity
        where TVersion : IComparable
    {
        RevisionRecord<TVersion> Revision { get; set; }
    }
}