using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.DataAccess.Engine.Mapping
{
    public class RefactoringFilesystemEventMap : ClassMap<RefactoringFilesystemEvent>
    {
        public RefactoringFilesystemEventMap()
        {
            Table("RefactoringFilesystemEvent");
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.EventName).Not.Nullable();
            Map(_ => _.NewFullPath).Not.Nullable();            
            Map(_ => _.OldFullPath).Nullable();
            Map(_ => _.RefactoringGroupId).Not.Nullable();
            Map(_ => _.Processed).Not.Nullable();
            Map(_ => _.CreationDate).Not.Nullable();
            References(_ => _.RefactoringSourceAction).Column("RefactoringSourceActionId").Nullable();
        }
    }
}
