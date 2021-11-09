using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Models;
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
            Map(_ => _.DocumentPath).Not.Nullable();
        }
    }
}
