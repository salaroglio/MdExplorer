using Ad.Tools.Dal.UnitTest.concrete;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.UnitTest.map
{
    public class MarkdownFileMap : ClassMap<MarkdownFile>
    {
        public MarkdownFileMap()
        {
            Table("MarkdownFile");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.Path).Not.Nullable();
            
        }
    }
}
