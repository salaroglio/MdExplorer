using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Models;

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
            Map(_ => _.LinkPath).Not.Nullable();
            Map(_ => _.FileType).Not.Nullable();
        }
    }
}
