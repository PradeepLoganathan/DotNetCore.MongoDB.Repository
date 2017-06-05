using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNETCore.Repository.Mongo;

namespace DotNetCore.MongoDB.Repository.Tracking
{
    public interface ITrackedEntity<TEntity, TVersion> : IEntity
        where TVersion : IComparable
        where TEntity : ITrackableEntity<TVersion>, new()
    {
        List<RevisionRecord<TVersion>> Revisions { get; set; }
        TEntity GetRevision(TVersion version = default(TVersion));
        void Initialize(TEntity entity);
    }


    public class TrackedEntity<TEntity, TVersion> : Entity, ITrackedEntity<TEntity, TVersion>
        where TEntity : TrackableEntity<TVersion>, new()
        where TVersion : IComparable


    {
        public TrackedEntity()
        {
            Revisions = new List<RevisionRecord<TVersion>>();
        }

        public TrackedEntity(TEntity initialValue)
        {
            Revisions = new List<RevisionRecord<TVersion>>();
            Initialize(initialValue);
        }

        public List<RevisionRecord<TVersion>> Revisions { get; set; }


        public TEntity GetRevision(TVersion version = default(TVersion))
        {
            var versionRecord = version.Equals(default(TVersion))
                ? Revisions.First(x => x.Version.Equals(Revisions.Max(a => a.Version)))
                : Revisions.FirstOrDefault(x => x.Version.CompareTo(version) <= 0);

            var allSourceProperties = GetType()
                .GetProperties();

            var versionedSourceProperties = allSourceProperties
                .Where(x => typeof(ITrackedProperty).IsAssignableFrom(x.PropertyType)).ToList();
            var standardSourceProperties = allSourceProperties
                .Where(x => !versionedSourceProperties.Contains(x));

            var result = new TEntity {Revision = versionRecord};

            var allTargetProperties = result
                .GetType()
                .GetProperties()
                .Where(x => x.SetMethod != null && x.SetMethod.IsPublic)
                .ToList();

            foreach (var prop in versionedSourceProperties)
            {
                var value = (ITrackedProperty) prop.GetValue(this);
                result.GetType().GetProperties().First(x => x.Name == prop.Name)
                    .SetValue(result, value.GetVersionValue(version));
            }

            foreach (var prop in standardSourceProperties)
                allTargetProperties
                    .FirstOrDefault(x => x.Name == prop.Name)?
                    .SetValue(result, prop.GetValue(this));

            return result;
        }

        public void Initialize(TEntity entity)
        {
            AddRevision(entity);
        }


        public void AddRevision(TEntity entity, DateTime versionDateTime = default(DateTime))
        {
            if (Revisions.Any())
                if (Revisions.Max(x => x.Version).CompareTo(entity.Revision.Version) >= 0)
                    throw new OldVersionInsertionException();

            Revisions.Add(new RevisionRecord<TVersion>
            {
                Version = entity.Revision.Version,
                Date = versionDateTime == default(DateTime) ? DateTime.UtcNow : versionDateTime
            });

            var targetProperties = GetType()
                .GetProperties().Where(
                    x => x.SetMethod != null &&
                         x.SetMethod.IsPublic && x.Name != "Revisions").ToArray();

            var sourceProperties = entity.GetType().GetProperties();

            foreach (var prop in targetProperties)
            {
                var sourceValue = sourceProperties
                    .FirstOrDefault(x => x.Name == prop.Name)?
                    .GetValue(entity);

                if (typeof(ITrackedProperty).IsAssignableFrom(prop.PropertyType))
                {
                    var propVal = (ITrackedProperty) prop.GetValue(this);
                    if (propVal == null)
                    {
                        propVal = (ITrackedProperty) Activator.CreateInstance(prop.PropertyType);
                        prop.SetValue(this, propVal);
                    }

                    propVal.AddVersion(sourceValue, entity.Revision.Version);
                }
                else
                {
                    prop.SetValue(this, sourceValue);
                }
            }
        }
    }
}