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
        /// <summary>La riga di <see cref="ChatLearning"/> da approvare o rifiutare; null per una proposta non valida.</summary>
        public Guid? RecordId { get; init; }
    }

    /// <summary>Com'è andata una decisione: la skill scritta (dove, quale versione) o il motivo per cui no.</summary>
    public sealed class LearnDecision
    {
        public Guid RecordId { get; init; }
        public string Outcome { get; init; }
        public string TargetPath { get; init; }
        public int? Version { get; init; }
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
                .Replace("{{previous}}", Previous(projectPath, threadId))
                .Replace("{{thread}}", threadId.ToString("D"))
                .Replace("{{from}}", from.ToString())
                .Replace("{{to}}", to.ToString())
                // Per ultima: l'attività è testo dell'utente e del modello, e non deve far scattare altri segnaposto.
                .Replace("{{activity}}", ChatActivityView.Compact(work));
            return new LearnRequest { ThreadId = threadId, Prompt = prompt, From = from, To = to, Harness = inventory.Harness, SkillsFolder = inventory.SkillsFolder };
        }

        /// <summary>La risposta del motore: proposte lette, controllate e messe a confronto con i file del progetto.</summary>
        public LearnReview Review(string projectPath, long from, long to, string answer) => Review(projectPath, Guid.Empty, from, to, answer);

        /// <summary>
        /// Come sopra, e le proposte valide diventano righe <see cref="ChatLearning"/> in sospeso: l'utente le approva o rifiuta
        /// una alla volta (U5). Quelle in sospeso di un «Impara» precedente della stessa chat passano a «riviste».
        /// </summary>
        public LearnReview Review(string projectPath, Guid threadId, long from, long to, string answer)
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
            if (threadId != Guid.Empty) proposals = Remember(projectPath, threadId, proposals);
            return new LearnReview { Prose = read.Prose, BlockProblem = read.BlockProblem, Proposals = proposals };
        }

        private List<ReviewedProposal> Remember(string projectPath, Guid threadId, List<ReviewedProposal> proposals)
        {
            WithSession(session =>
            {
                var dal = session.GetDal<ChatLearning>();
                var now = DateTime.UtcNow;
                foreach (var old in dal.GetList().Where(l => l.ThreadId == threadId && l.Outcome == ChatLearning.Pending).ToList())
                {
                    old.Outcome = ChatLearning.Superseded;
                    old.DecidedAt = now;
                    dal.Save(old);
                }
                for (var i = 0; i < proposals.Count; i++)
                {
                    var p = proposals[i];
                    if (!p.IsValid) continue;
                    var row = new ChatLearning
                    {
                        ThreadId = threadId, ProjectPath = projectPath, Action = p.Action, Skill = p.Skill, Why = p.Why,
                        Evidence = string.Join(",", p.Evidence), Content = p.Content, TargetPath = p.TargetPath,
                        Outcome = ChatLearning.Pending, CreatedAt = now,
                    };
                    dal.Save(row);
                    proposals[i] = new ReviewedProposal
                    {
                        Index = p.Index, Action = p.Action, Skill = p.Skill, Why = p.Why, Evidence = p.Evidence, Content = p.Content,
                        Problems = p.Problems, TargetPath = p.TargetPath, CurrentContent = p.CurrentContent, Diff = p.Diff, RecordId = row.Id,
                    };
                }
            });
            return proposals;
        }

        /// <summary>
        /// «Approva» (U5): scrive subito la skill, con il blocco <c>learned:</c> (versione, data, chat), passando dallo scrittore
        /// di MDE, quindi col commit automatico. <paramref name="correctedContent"/> = il testo corretto dall'utente, controllato
        /// come una proposta. Eccezione con il motivo quando non si può (e la riga resta in sospeso).
        /// </summary>
        public LearnDecision Approve(string projectPath, Guid recordId, string correctedContent)
        {
            var row = Record(recordId, projectPath);
            var content = string.IsNullOrWhiteSpace(correctedContent) ? row.Content : correctedContent.Replace("\r\n", "\n");
            var problem = SkillLearningBlock.ContentProblem(content, row.Skill);
            if (problem != null) throw new InvalidOperationException(problem);

            var inventory = ProjectSkillInventory.For(projectPath);
            if (inventory.Unavailable != null) throw new InvalidOperationException(inventory.Unavailable);
            var expected = $"{inventory.SkillsFolder}/{row.Skill}/SKILL.md";
            if (!string.Equals(expected, row.TargetPath, StringComparison.Ordinal))
                throw new InvalidOperationException(UiText.Say(
                    $"Il motore del progetto è cambiato: la skill andrebbe in {expected}, non più in {row.TargetPath}. Premi di nuovo «Impara».",
                    $"The project's engine changed: the skill would go to {expected}, no longer to {row.TargetPath}. Press «Learn» again."));
            var existing = inventory.Skills.FirstOrDefault(s => s.Name == row.Skill);
            if (existing?.IsMde == true)
                throw new InvalidOperationException(UiText.Say($"«{row.Skill}» è di MdExplorer: non si scrive.", $"«{row.Skill}» belongs to MdExplorer: not written."));
            if (row.Action == SkillProposal.ActionCreate && existing != null)
                throw new InvalidOperationException(UiText.Say(
                    $"Nel frattempo la skill «{row.Skill}» è nata: premi di nuovo «Impara» per avere una correzione invece di una skill nuova.",
                    $"Meanwhile the skill «{row.Skill}» was created: press «Learn» again to get an update instead of a new skill."));

            var full = Path.Combine(projectPath, row.TargetPath);
            var previous = File.Exists(full) ? LearnedVersion(File.ReadAllText(full, Encoding.UTF8)) : 0;
            var version = previous + 1;
            var written = WithLearned(content, version, row.ThreadId);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            MdExplorer.Utilities.MdeRepoWriter.WriteIfChanged(full, written, new MdExplorer.Utilities.MdeWhy(
                $"skill «{row.Skill}» imparata da una chat (versione {version})", $"skill «{row.Skill}» learned from a chat (version {version})"));

            var outcome = string.IsNullOrWhiteSpace(correctedContent) || correctedContent.Replace("\r\n", "\n") == row.Content ? ChatLearning.Approved : ChatLearning.Corrected;
            WithSession(session =>
            {
                var r = session.GetDal<ChatLearning>().GetList().Single(l => l.Id == recordId);
                r.Outcome = outcome;
                r.Content = content;
                r.WrittenVersion = version;
                r.DecidedAt = DateTime.UtcNow;
                session.GetDal<ChatLearning>().Save(r);
            });
            return new LearnDecision { RecordId = recordId, Outcome = outcome, TargetPath = row.TargetPath, Version = version };
        }

        /// <summary>«Rifiuta» (U5): si ricorda subito, e il prossimo «Impara» del progetto non la ripropone senza prove nuove.</summary>
        public LearnDecision Reject(string projectPath, Guid recordId)
        {
            Record(recordId, projectPath);
            WithSession(session =>
            {
                var r = session.GetDal<ChatLearning>().GetList().Single(l => l.Id == recordId);
                r.Outcome = ChatLearning.Rejected;
                r.DecidedAt = DateTime.UtcNow;
                session.GetDal<ChatLearning>().Save(r);
            });
            return new LearnDecision { RecordId = recordId, Outcome = ChatLearning.Rejected };
        }

        /// <summary>Le proposte precedenti per il prompt: le rifiutate del progetto, e le approvate e in sospeso di questa chat.</summary>
        private string Previous(string projectPath, Guid threadId)
        {
            List<ChatLearning> rows = null;
            WithSession(session => rows = session.GetDal<ChatLearning>().GetList()
                .Where(l => l.ProjectPath == projectPath
                            && (l.Outcome == ChatLearning.Rejected
                                || (l.ThreadId == threadId && (l.Outcome == ChatLearning.Pending || l.Outcome == ChatLearning.Approved || l.Outcome == ChatLearning.Corrected))))
                .OrderBy(l => l.CreatedAt).ToList());
            if (rows.Count == 0) return UiText.Say("(nessuna)", "(none)");
            return string.Join("\n", rows.Select(l =>
            {
                var state = l.Outcome switch
                {
                    ChatLearning.Rejected => "rifiutata",
                    ChatLearning.Pending => "in sospeso",
                    ChatLearning.Corrected => $"approvata dopo una correzione dell'utente (versione {l.WrittenVersion})",
                    _ => $"approvata (versione {l.WrittenVersion})",
                };
                var body = l.Outcome == ChatLearning.Pending ? $"\n  Testo proposto:\n  ```\n{l.Content}\n  ```" : "";
                return $"- {state}: {l.Action} `{l.Skill}` — {l.Why}{body}";
            }));
        }

        private ChatLearning Record(Guid recordId, string projectPath)
        {
            ChatLearning row = null;
            WithSession(session => row = session.GetDal<ChatLearning>().GetList().SingleOrDefault(l => l.Id == recordId));
            if (row == null) throw new InvalidOperationException(UiText.Say($"La proposta {recordId} non c'è.", $"The proposal {recordId} does not exist."));
            if (!string.Equals(row.ProjectPath, projectPath, StringComparison.Ordinal))
                throw new InvalidOperationException(UiText.Say("La proposta è di un altro progetto.", "The proposal belongs to another project."));
            if (row.Outcome != ChatLearning.Pending)
                throw new InvalidOperationException(UiText.Say($"La proposta è già decisa ({row.Outcome}).", $"The proposal is already decided ({row.Outcome})."));
            return row;
        }

        /// <summary>La versione <c>learned:</c> di una skill, 0 se non ne ha (scritta a mano, o nuova).</summary>
        internal static int LearnedVersion(string text)
        {
            var m = Regex.Match(text ?? string.Empty, @"\A---[ \t]*\r?\n(.*?)\r?\n---", RegexOptions.Singleline);
            if (!m.Success) return 0;
            var v = Regex.Match(m.Groups[1].Value, @"^learned:\s*\r?\n(?:[ \t]+.*\r?\n)*?[ \t]+version:\s*(\d+)", RegexOptions.Multiline);
            return v.Success ? int.Parse(v.Groups[1].Value) : 0;
        }

        /// <summary>Il testo approvato con il blocco <c>learned:</c> in fondo al frontmatter (lo scrive MdExplorer, mai il modello).</summary>
        internal static string WithLearned(string content, int version, Guid threadId)
        {
            var m = Regex.Match(content, @"\A(---[ \t]*\r?\n)(.*?)(\r?\n---[ \t]*(\r?\n|\z))", RegexOptions.Singleline);
            if (!m.Success) throw new InvalidOperationException(UiText.Say("Il testo non comincia con il frontmatter.", "The text does not start with the frontmatter."));
            var learned = $"\nlearned:\n  version: {version}\n  updated: {DateTime.Now:yyyy-MM-dd}\n  chat: {threadId:D}";
            var text = m.Groups[1].Value + m.Groups[2].Value + learned + m.Groups[3].Value + content.Substring(m.Length);
            return text.EndsWith("\n") ? text : text + "\n";
        }

        private void WithSession(Action<IUserSettingsDB> work)
        {
            using var scope = _scopes.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDALFactory<IUserSettingsDB>>();
            using var session = factory.OpenSession();
            session.BeginTransaction();
            try
            {
                work(session);
                session.Commit();
            }
            catch
            {
                session.Rollback();
                throw;
            }
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
