using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetCore.MongoDB.Repository.Tracking
{

    public interface ITrackedProperty
    {
        IPropertyRevision GetVersion(object version = null);
        object GetVersionValue(object version = null);
        void AddVersion(object value, object version);

    }



   

    
    public interface ITrackedProperty<TValue, TVersion> : IEnumerable<PropertyRevision<TValue,TVersion>> , ITrackedProperty
        where TVersion : IComparable
    {

        IPropertyRevision<TValue, TVersion> GetRevision(TVersion version = default(TVersion));
        TValue GetValue(TVersion version = default(TVersion));
        void AddRevision(TValue value, TVersion version);

    }


    public class TrackedColllectionProperty<TValue, TVersion> : TrackedProperty<IEnumerable<TValue>, TVersion>
        where TVersion : IComparable
    {
        public override void AddRevision(IEnumerable<TValue> value, TVersion version)
        {
            var currentValue = GetValue();

            if (currentValue == null)
            {
                if (value == null)
                    return;
            }
            else
            {
                if (value != null && value.SequenceEqual(currentValue))
                {
                    return;
                }
            }

            base.AddRevision(value, version);
          
        }
    }

    public class TrackedProperty<TValue, TVersion> : List<PropertyRevision<TValue, TVersion>>, ITrackedProperty<TValue, TVersion>
        where TVersion : IComparable
    {

        public IPropertyRevision<TValue, TVersion> GetRevision(TVersion version = default(TVersion))
        {
            return version.Equals(default(TVersion)) ? this.LastOrDefault() : this.LastOrDefault(x => x.Version.CompareTo(version) <= 0);
        }


        public TValue GetValue(TVersion version = default(TVersion))
        {
            var ver = GetRevision(version);
            return ver != null ? ver.GetValue() : default(TValue);
        }

        public virtual void AddRevision(TValue value, TVersion version)
        {
            if (!typeof(TVersion).IsAssignableFrom(version.GetType()))
            {
                throw new PropertyVersionTypeMisMatchException();
            }

            if (this.Count == 0)
            {
                this.Add(new PropertyRevision<TValue, TVersion>(value, version));
                return;
            }


            var currentValue = GetValue();

            if (currentValue == null && value == null)
                return;
            
            if (currentValue!= null && currentValue.Equals(value))
                return;
            


            if (this.Last().Version.CompareTo((version)) >= 0)
            {
                throw new OldVersionInsertionException();
            }

            this.Add(new PropertyRevision<TValue, TVersion>(value, version));

        }

        object ITrackedProperty.GetVersionValue(object version)
        {
            return GetValue((TVersion)version);
        }

        void ITrackedProperty.AddVersion(object value, object version)
        {
        
            AddRevision(value==null?default(TValue):(TValue)value, (TVersion)version);
        }

        IPropertyRevision ITrackedProperty.GetVersion(object version)
        {
            return this.GetRevision((TVersion) version);
        }
    }





}
