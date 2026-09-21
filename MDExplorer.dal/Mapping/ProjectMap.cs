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
            // Motore di MarkAgent: NULL = segue l'harness del repository (MarkAgentEngines.Resolve).
            // Le colonne UseCopilotCliAsDefault/UseClaudeCodeAsDefault non si mappano piu': restano
            // nel DB (su SQLite Delete.Column e' un no-op, vedi M2026_09_21_001) ma sono NOT NULL
            // con DEFAULT, quindi l'INSERT che non le nomina funziona lo stesso.
            Map(_ => _.MarkAgentEngine).Length(20).Nullable();
            Map(_ => _.CopilotChatModel).Length(200).Nullable();
            Map(_ => _.ClaudeCodeChatModel).Length(200).Nullable();
            Map(_ => _.OpenCodeChatModel).Length(200).Nullable();
            Map(_ => _.ExecutionTrusted).Not.Nullable().Default("0");
            Map(_ => _.UseAgentWorktrees).Nullable();
            Map(_ => _.AgentWorktreeSlots).Nullable();
            Map(_ => _.IndexAllTextFiles).Not.Nullable().Default("0");
            Map(_ => _.TextFileExtensions).Length(int.MaxValue).Nullable();
            HasMany(x => x.Bookmarks).LazyLoad().Cascade.SaveUpdate();
        }
    }
}
