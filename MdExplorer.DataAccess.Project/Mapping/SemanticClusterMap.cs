using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.DataAccess.Project.Mapping
{
    public class SemanticClusterMap : ClassMap<SemanticCluster>
    {
        public SemanticClusterMap()
        {
            Table("SemanticCluster");
            Id(_=>_.Id).GeneratedBy.GuidComb();
            Map(_ => _.Name).Length(255).Not.Nullable();
            Map(_ => _.Description).Length(int.MaxValue).Nullable();
            HasMany(_ => _.Elements).Cascade.All();

        }
        
    }
}
