using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.EngineDB;

namespace MdExplorer.DataAccess.Engine
{
    public class TextFileMap : ClassMap<TextFile>
    {
        public TextFileMap()
        {
            Table("TextFile");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.FileName).Not.Nullable();
            Map(_ => _.Path).Not.Nullable();
            Map(_ => _.Extension).Nullable().Length(32);
            Map(_ => _.FileLastWriteUtc).Nullable().Length(64);
            Map(_ => _.FileSize).Nullable();
            Map(_ => _.FileHash).Nullable().Length(64);
            Map(_ => _.FtsHash).Nullable().Length(64);
        }
    }
}
