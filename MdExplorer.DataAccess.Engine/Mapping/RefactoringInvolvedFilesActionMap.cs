using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.DataAccess.Engine.Mapping
{
    public class RefactoringInvolvedFilesActionMap : ClassMap<RefactoringInvolvedFilesAction>
    {
        public RefactoringInvolvedFilesActionMap()
        {
            Id(_ => _.Id).GeneratedBy.GuidComb();
            Map(_ => _.FileName).Length(255).Not.Nullable();
            Map(_ => _.NewLinkToReplace).Length(255).Not.Nullable();
            Map(_ => _.OldLinkStored).Length(255).Not.Nullable();
            Map(_ => _.FullPath).Length(int.MaxValue).Nullable();
            Map(_ => _.CreationDate).Not.Nullable();
            References(_ => _.RefactoringSourceAction).Column("RefactoringSourceActionId").Nullable();
        }
    }
}
