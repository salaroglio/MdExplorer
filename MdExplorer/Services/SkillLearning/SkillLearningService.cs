using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI.ChatHistory;
using MdExplorer.Features.Services.AI.SkillLearning;
using Microsoft.Extensions.DependencyInjection;

namespace MdExplorer.Services.SkillLearning
{
    /// <summary>Il prompt pronto per il motore del tab, e l'intervallo dell'attività che riguarda.</summary>
    public sealed class LearnRequest
    {
        public Guid ThreadId { get; init; }
        public string Prompt { get; init; }
        public long From { get; init; }
        public long To { get; init; }
        public string Harness { get; init; }
        public string SkillsFolder { get; init; }
    }

    /// <summary>Una proposta pronta per la finestra: controllata, con dove andrebbe e le differenze.</summary>
    public sealed class ReviewedProposal
    {
        public int Index { get; init; }
        public string Action { get; init; }
        public string Skill { get; init; }
        public string Why { get; init; }
        public IReadOnlyList<long> Evidence { get; init; }
        public string Content { get; init; }
        public IReadOnlyList<string> Problems { get; init; }
        public bool IsValid => Problems.Count == 0;
        /// <summary>Dove verrebbe scritta, relativo al progetto; null se il nome non è valido.</summary>
        public string TargetPath { get; init; }
        /// <summary>Il testo di oggi, per un update; null per una skill nuova.</summary>
        public string CurrentContent { get; init; }
        public IReadOnlyList<DiffLine> Diff { get; init; }
    }

    public sealed class LearnReview
    {
        public string Prose { get; init; }
        public string BlockProblem { get; init; }
        public IReadOnlyList<ReviewedProposal> Proposals { get; init; } = Array.Empty<ReviewedProposal>();
    }

    /// <summary>
    /// «Impara da questa attività» (sprint 2026-11-24-Skill-Che-Migliorano, F3): il servizio costruisce il prompt
    /// mde-mark-learn (vista compatta dell'attività, skill del progetto, proposte rifiutate), il client lo manda sulla sessione
    /// del tab (stesso motore, stessa sessione), e la risposta torna qui per essere letta, controllata e confrontata con i file.
    /// </summary>
    public sealed class SkillLearningService
    {
        /// <summary>Il canale del tab su cui viaggia la richiesta: resta nella cronologia della chat, distinto dal resto.</summary>
        public const string Channel = "learn";

        private const string PromptResource = "MdExplorer.Service.skills.mde_mark_learn.prompt.md";
        private static readonly Lazy<string> _template = new(LoadTemplate);

        private readonly IChatFileStore _files;
        private readonly IServiceScopeFactory _scopes;

        public SkillLearningService(IChatFileStore files, IServiceScopeFactory scopes)
        {
            _files = files;
            _scopes = scopes;
        }

        /// <summary>Il prompt per l'attività della chat. Eccezione con il motivo da dire all'utente quando non si può.</summary>
        public LearnRequest Prepare(string projectPath, Guid threadId)
        {
            var row = Row(threadId) ?? throw new InvalidOperationException(UiText.Say(
                $"La chat {threadId} non è nella cronologia.", $"The chat {threadId} is not in the history."));
            if (!string.Equals(row.ProjectPath, projectPath, StringComparison.Ordinal))
                throw new InvalidOperationException(UiText.Say($"La chat è di un altro progetto ({row.ProjectPath}).", $"The chat belongs to another project ({row.ProjectPath})."));
            var inventory = ProjectSkillInventory.For(projectPath);
            if (inventory.Unavailable != null) throw new InvalidOperationException(inventory.Unavailable);

            var content = _files.Read(row.FilePath);
            if (!content.Exists) throw new InvalidOperationException(UiText.Say($"Il file della chat non c'è più: {row.FilePath}.", $"The chat file is gone: {row.FilePath}."));
            // L'attività è il lavoro, non le richieste di «Impara» fatte prima nella stessa chat.
            var work = content.Events.Where(e => e.Channel != Channel).ToList();
            if (!work.Any(e => e.Kind == ChatRecordEvent.KindUserPrompt))
                throw new InvalidOperationException(UiText.Say("In questa chat non c'è ancora un'attività da cui imparare.", "This chat has no activity to learn from yet."));

            var from = work.Min(e => e.Seq);
            var to = work.Max(e => e.Seq);
            var prompt = _template.Value
                .Replace("{{harness}}", inventory.Harness)
                .Replace("{{skills}}", ProjectSkillInventory.ForPrompt(inventory))
                .Replace("{{rejected}}", UiText.Say("(nessuna)", "(none)"))
                .Replace("{{thread}}", threadId.ToString("D"))
                .Replace("{{from}}", from.ToString())
                .Replace("{{to}}", to.ToString())
                // Per ultima: l'attività è testo dell'utente e del modello, e non deve far scattare altri segnaposto.
                .Replace("{{activity}}", ChatActivityView.Compact(work));
            return new LearnRequest { ThreadId = threadId, Prompt = prompt, From = from, To = to, Harness = inventory.Harness, SkillsFolder = inventory.SkillsFolder };
        }

        /// <summary>La risposta del motore: proposte lette, controllate e messe a confronto con i file del progetto.</summary>
        public LearnReview Review(string projectPath, long from, long to, string answer)
        {
            var inventory = ProjectSkillInventory.For(projectPath);
            if (inventory.Unavailable != null) throw new InvalidOperationException(inventory.Unavailable);
            var existing = inventory.Skills.ToDictionary(s => s.Name, s => s.IsMde, StringComparer.Ordinal);
            var read = SkillLearningBlock.Read(answer, new SkillLearningContext { ExistingSkills = existing, From = from, To = to });

            var proposals = read.Proposals.Select(p =>
            {
                var target = p.Skill != null && SkillLearningBlock.NameProblem(p.Skill) == null ? $"{inventory.SkillsFolder}/{p.Skill}/SKILL.md" : null;
                var full = target == null ? null : Path.Combine(projectPath, target);
                var current = full != null && File.Exists(full) ? File.ReadAllText(full, Encoding.UTF8) : null;
                return new ReviewedProposal
                {
                    Index = p.Index, Action = p.Action, Skill = p.Skill, Why = p.Why, Evidence = p.Evidence, Content = p.Content,
                    Problems = p.Problems, TargetPath = target, CurrentContent = current,
                    Diff = LineDiff.Compute(current, p.Content),
                };
            }).ToList();
            return new LearnReview { Prose = read.Prose, BlockProblem = read.BlockProblem, Proposals = proposals };
        }

        private ChatThread Row(Guid id)
        {
            using var scope = _scopes.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDALFactory<IUserSettingsDB>>();
            using var session = factory.OpenSession();
            return session.GetDal<ChatThread>().GetList().SingleOrDefault(t => t.Id == id);
        }

        private static string LoadTemplate()
        {
            using var stream = typeof(SkillLearningService).Assembly.GetManifestResourceStream(PromptResource)
                ?? throw new InvalidOperationException($"Manca la risorsa {PromptResource}: il pacchetto non è completo.");
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            var fm = Regex.Match(text, @"^---[ \t]*\r?\n.*?\r?\n---[ \t]*\r?\n", RegexOptions.Singleline);
            if (fm.Success && fm.Index == 0) text = text.Substring(fm.Length);
            return Regex.Replace(text, @"<!--.*?-->", string.Empty, RegexOptions.Singleline).Trim();
        }
    }
}
