using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.EngineDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.DataAccess.Engine.Mapping
{
    public class LinkInsideMarkdownMap : ClassMap<LinkInsideMarkdown>
    {
        public LinkInsideMarkdownMap()
        {
            Table("LinkInsideMarkdown");
            Id(_=>_.Id).GeneratedBy.GuidComb();
            References(_ => _.MarkdownFile).Column("MarkdownFileId").Not.Nullable();
            Map(_ => _.Path).Length(255).Not.Nullable();
            Map(_ => _.Source).Length(255).Not.Nullable();
            Map(_ => _.SectionIndex).Nullable();
            Map(_ => _.LinkedCommand).Not.Nullable();
            Map(_ => _.FullPath).Not.Nullable();
        }
    }
}
