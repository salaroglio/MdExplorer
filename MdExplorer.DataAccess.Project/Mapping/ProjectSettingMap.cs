using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.ProjectDB;
using MdExplorer.Abstractions.Entities.UserDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.DataAccess.Project.Mapping
{
    public class ProjectSettingMap : ClassMap<ProjectSetting>
    {
        public ProjectSettingMap()
        {
            Table("ProjectSetting");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.Name).Not.Nullable();
            Map(_ => _.ValueDateTime).Nullable();
            Map(_ => _.ValueDecimal).Nullable();
            Map(_ => _.ValueInt).Nullable();
            Map(_ => _.ValueString).Nullable();
            HasMany(_ => _.LandingPages).Cascade.All();
        }
    }
}
