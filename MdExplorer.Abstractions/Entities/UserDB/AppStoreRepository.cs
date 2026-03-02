using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class AppStoreRepository
    {
        public virtual Guid Id { get; set; }
        public virtual string Label { get; set; }
        public virtual string Url { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
        public virtual int SortOrder { get; set; }
    }
}
