using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDExplorer.DataAccess.Mapping
{
    public class GitlabSettingMap : ClassMap<GitlabSetting>
    {
        public GitlabSettingMap()
        {
            Table("GitlabSetting");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.UserName).Length(256).Not.Nullable();
            Map(_ => _.Password).Length(256).Not.Nullable();
            Map(_ => _.GitlabLink).Length(int.MaxValue).Not.Nullable();
            Map(_ => _.LocalPath).Length(int.MaxValue).Not.Nullable();
        }
    }
}
