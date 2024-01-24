using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDExplorer.DataAccess.Mapping
{
    public class DocumentSettingMap : ClassMap<DocumentSetting>
    {
        public DocumentSettingMap()
        {
            Table("DocumentSetting");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.ShowTOC).Not.Nullable();
            Map(_=>_.ShowRefs).Not.Nullable();
            Map(_ => _.DocumentPath).Not.Nullable();
            Map(_=>_.RefsWidth).Nullable();
            Map(_=>_.TocWidth).Nullable();
        }
    }
}
