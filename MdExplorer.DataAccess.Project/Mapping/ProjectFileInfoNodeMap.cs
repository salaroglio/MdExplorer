using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.ProjectDB;
using Org.BouncyCastle.Crypto.Prng.Drbg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.DataAccess.Project.Mapping
{
    public class ProjectFileInfoNodeMap : ClassMap<ProjectFileInfoNode>
    { 
        public ProjectFileInfoNodeMap() 
        {
            Table("ProjectFileInfoNode");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.Name).Not.Nullable();
            Map(_ => _.Path).Nullable();
            Map(_ => _.FullPath).Nullable();
            Map(_ => _.RelativePath).Nullable();
            Map(_ => _.Type).Nullable();
            Map(_ => _.Level).Nullable();
            Map(_ => _.Expandable).Nullable();
            Map(_ => _.DataText).Nullable();
            References(_ => _.ProjectSetting).Column("ProjectSettingId");
            References(_ => _.Parent).Column("ParentId");
            HasMany(_ => _.Childrens);

        }
    }
}
