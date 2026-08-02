using FluentNHibernate.Mapping;
using MdExplorer.Abstractions.Entities.UserDB;
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
            Map(_ => _.LastUpdate).Not.Nullable();
            Map(_ => _.SidenavWidth).Nullable();
            Map(_ => _.SelectedIde).Length(50).Nullable();
            Map(_ => _.LinkIndexingEnabled).Not.Nullable().Default("1");
            Map(_ => _.PlantUmlKeepOriginalColorsInDarkMode).Not.Nullable().Default("0");
            Map(_ => _.UseCopilotCliAsDefault).Not.Nullable().Default("1");
            Map(_ => _.ExecutionTrusted).Not.Nullable().Default("0");
            Map(_ => _.ExcludeSubmodulesFromGitStatus).Not.Nullable().Default("1");
            Map(_ => _.UseAgentWorktrees).Nullable();
            Map(_ => _.AgentWorktreeSlots).Nullable();
            Map(_ => _.IndexAllTextFiles).Not.Nullable().Default("0");
            Map(_ => _.TextFileExtensions).Length(int.MaxValue).Nullable();
            HasMany(x => x.Bookmarks).LazyLoad().Cascade.SaveUpdate();
        }
    }
}
