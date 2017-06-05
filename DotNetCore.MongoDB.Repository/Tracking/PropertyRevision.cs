using System;

namespace DotNetCore.MongoDB.Repository.Tracking
{
    public interface IPropertyRevision
    {
        object GetValue();
        object Create(object value, object version);
    }

    public interface IPropertyRevision<TValue, TVersion> : IPropertyRevision
    {
        TVersion Version { get; set; }
        TValue Value { get; set; }

        new TValue GetValue();
        IPropertyRevision<TValue, TVersion> Create(TValue value, TVersion version);
    }

    public class PropertyRevision<TValue, TVersion> : IPropertyRevision<TValue, TVersion>
        where TVersion : IComparable
    {
        public PropertyRevision()
        {
        }

        public PropertyRevision(TValue value, TVersion version)
        {
            Value = value;
            Version = version;
        }

        public TVersion Version { get; set; }
        public TValue Value { get; set; }

        public TValue GetValue()
        {
            return Value;
        }

        public IPropertyRevision<TValue, TVersion> Create(TValue value, TVersion version)
        {
            return new PropertyRevision<TValue, TVersion>(value, version);
        }

        object IPropertyRevision.GetValue()
        {
            return GetValue();
        }

        object IPropertyRevision.Create(object value, object version)
        {
            return Create((TValue) value, (TVersion) version);
        }
    }
}