using System;

namespace DotNetCore.MongoDB.Repository.Tracking
{
    public interface IRevisionRecord<TVersion>
    {
        TVersion Version { get; set; }
        DateTime Date { get; set; }
    }

    public class RevisionRecord<TVersion> : IRevisionRecord<TVersion>
    {
        public TVersion Version { get; set; }
        public DateTime Date { get; set; }

        public RevisionRecord()
        {
            Date = DateTime.UtcNow;
        }
    }
}