using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDExplorer.DataAccess.Mapping
{
    public class ProjectMap : ClassMap<Project>
    {
        public ProjectMap()
        {
            Table("Project");
            Id(_=>_.Id).GeneratedBy.GuidComb();
            Map(_ => _.Name).Length(255).Not.Nullable();
            Map(_ => _.Path).Length(int.MaxValue).Not.Nullable();
        }
    }
}
