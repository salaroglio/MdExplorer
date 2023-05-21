using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.ProjectDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.DataAccess.Project.Mapping
{
    public class SemanticClusterElementMap: ClassMap<SemanticClusterElement>
    {
        public SemanticClusterElementMap()
        {
            Table("SemanticClusterElement");
            Id(_ => _.Id).GeneratedBy.GuidComb();            
            Map(_ => _.Name).Length(255).Not.Nullable();
            Map(_ => _.Description).Length(int.MaxValue).Nullable();
            References(_ => _.Cluster).Column("SemanticClusterId");  
        }
    }
}
