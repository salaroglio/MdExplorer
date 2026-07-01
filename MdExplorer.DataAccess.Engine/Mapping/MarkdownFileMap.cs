using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.EngineDB;

namespace MdExplorer.DataAccess.Engine
{
    public class MarkdownFileMap: ClassMap<MarkdownFile>
    {
        public MarkdownFileMap()
        {
            Table("MarkdownFile");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.FileName).Not.Nullable();
            Map(_ => _.Path).Not.Nullable();            
            Map(_ => _.FileType).Not.Nullable();
            Map(_ => _.Tldr).Nullable().Length(int.MaxValue);
            Map(_ => _.FileLastWriteUtc).Nullable().Length(64);
            Map(_ => _.FileSize).Nullable();
            Map(_ => _.FileHash).Nullable().Length(64);
            Map(_ => _.LinksHash).Nullable().Length(64);
            Map(_ => _.FtsHash).Nullable().Length(64);
            HasMany(x => x.Links).Cascade.All();
        }
    }
}
