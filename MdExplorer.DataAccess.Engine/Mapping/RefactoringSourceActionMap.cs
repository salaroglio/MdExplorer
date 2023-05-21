using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.EngineDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.DataAccess.Engine.Mapping
{
    public class RefactoringSourceActionMap : ClassMap<RefactoringSourceAction>
    {
        public RefactoringSourceActionMap() 
        {
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.Action).Length(int.MaxValue).Not.Nullable();
            Map(_ => _.OldName).Length(255).Nullable();
            Map(_ => _.NewName).Length(255).Nullable();
            Map(_ => _.OldFullPath).Length(int.MaxValue).Nullable();
            Map(_ => _.NewFullPath).Length(int.MaxValue).Nullable();
            Map(_=>_.Status).Length(255).Nullable();            
            Map(_ => _.CreationDate).Not.Nullable();            
            HasMany(_ => _.ActionDetails).Cascade.All();
        }
    }
}
