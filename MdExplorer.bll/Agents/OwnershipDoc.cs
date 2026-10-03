using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Una riga del documento di ownership (§12.3 Agent-Harness-A2A): "chi è responsabile
    /// di quale ambito, con quali agenti". È un <b>routing hint</b>, non un permesso — il
    /// guardrail resta il gate umano (§12.6).
    /// </summary>
    public sealed class OwnershipEntry
    {
        public string Scope { get; init; }
        public string Description { get; init; }
        public string Responsible { get; init; }
        /// <summary>Chiave d'identità del responsabile (email git, lowercased).</summary>
        public string GitEmail { get; init; }
        /// <summary>Agenti citati per questo ambito (nomi <c>a2a.name</c>).</summary>
        public IReadOnlyList<string> Agents { get; init; } = Array.Empty<string>();
    }

    /// <summary>Esito del parsing di un doc di ownership. Fail-loud: gli errori sono espliciti.</summary>
    public sealed class OwnershipParseResult
    {
        /// <summary>Il doc dichiara <c>mde_type: ownership</c> nel frontmatter.</summary>
        public bool IsOwnershipDoc { get; init; }
        public IReadOnlyList<OwnershipEntry> Entries { get; init; } = Array.Empty<OwnershipEntry>();
        /// <summary>Errori strutturali (tabella assente, ambito duplicato, cella obbligatoria mancante).</summary>
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public bool HasErrors => Errors.Count > 0;
    }

    /// <summary>
    /// Parser PURO del documento di ownership (§12.3): riconosce il frontmatter
    /// <c>mde_type: ownership</c> e legge la tabella markdown
    /// (Ambito | Descrizione | Responsabile | Git Email | Agenti). Nessuna dipendenza
    /// esterna: le convalide che richiedono le fonti (email nei participants, agente nel
    /// registry) stanno in <see cref="OwnershipValidator"/>. Header in IT o EN.
    /// </summary>
    public static class OwnershipDocParser
    {
        // Frontmatter in testa al documento: --- ... ---
        private static readonly Regex FrontMatterRegex = new(
            @"\A﻿?\s*-{3}\r?\n(.*?)\r?\n-{3}",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // Riga 'mde_type: ownership' dentro il frontmatter (case-insensitive).
        private static readonly Regex OwnershipTypeRegex = new(
            @"^\s*mde_type\s*:\s*[""']?ownership[""']?\s*$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Alias di intestazione colonna → colonna canonica.
        private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ambito"] = "scope", ["scope"] = "scope",
            ["descrizione"] = "description", ["description"] = "description",
            ["responsabile"] = "responsible", ["responsible"] = "responsible", ["owner"] = "responsible",
            ["git email"] = "gitEmail", ["gitemail"] = "gitEmail", ["email"] = "gitEmail",
            ["agenti"] = "agents", ["agents"] = "agents",
        };

        public static OwnershipParseResult Parse(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return new OwnershipParseResult { IsOwnershipDoc = false };

            var fm = FrontMatterRegex.Match(markdown);
            var isOwnership = fm.Success && OwnershipTypeRegex.IsMatch(fm.Groups[1].Value);
            if (!isOwnership)
                return new OwnershipParseResult { IsOwnershipDoc = false };

            var errors = new List<string>();
            var body = markdown.Substring(fm.Index + fm.Length);
            var table = ParseFirstTable(body);
            if (table == null)
            {
                errors.Add("Documento di ownership senza tabella: attesa una tabella markdown con colonne Ambito | Responsabile | Git Email | Agenti.");
                return new OwnershipParseResult { IsOwnershipDoc = true, Errors = errors };
            }

            if (!table.ColumnIndex.ContainsKey("scope") || !table.ColumnIndex.ContainsKey("gitEmail"))
            {
                errors.Add("La tabella di ownership deve avere almeno le colonne 'Ambito' e 'Git Email'.");
                return new OwnershipParseResult { IsOwnershipDoc = true, Errors = errors };
            }

            var entries = new List<OwnershipEntry>();
            var seenScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rowNum = 0;
            foreach (var row in table.Rows)
            {
                rowNum++;
                var scope = Cell(table, row, "scope");
                var gitEmail = Cell(table, row, "gitEmail");

                if (string.IsNullOrWhiteSpace(scope))
                {
                    errors.Add($"Riga {rowNum}: ambito mancante.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(gitEmail))
                {
                    errors.Add($"Riga {rowNum} ('{scope}'): Git Email del responsabile mancante.");
                    continue;
                }
                if (!seenScopes.Add(scope.Trim()))
                {
                    errors.Add($"Ambito duplicato: '{scope.Trim()}' compare più di una volta (gli ambiti devono essere univoci).");
                    continue;
                }

                entries.Add(new OwnershipEntry
                {
                    Scope = scope.Trim(),
                    Description = Cell(table, row, "description")?.Trim(),
                    Responsible = Cell(table, row, "responsible")?.Trim(),
                    GitEmail = gitEmail.Trim().ToLowerInvariant(),
                    Agents = SplitAgents(Cell(table, row, "agents")),
                });
            }

            return new OwnershipParseResult
            {
                IsOwnershipDoc = true,
                Entries = entries,
                Errors = errors,
            };
        }

        private static string Cell(MarkdownTable table, string[] row, string canonical)
        {
            if (!table.ColumnIndex.TryGetValue(canonical, out var idx)) return null;
            return idx < row.Length ? row[idx] : null;
        }

        private static IReadOnlyList<string> SplitAgents(string cell)
        {
            if (string.IsNullOrWhiteSpace(cell)) return Array.Empty<string>();
            return cell
                .Split(new[] { ',', ';', '\n', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim().Trim('`'))
                .Where(a => a.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // ---- parsing minimale delle tabelle pipe ----

        private sealed class MarkdownTable
        {
            public Dictionary<string, int> ColumnIndex { get; init; }
            public List<string[]> Rows { get; init; }
        }

        private static MarkdownTable ParseFirstTable(string body)
        {
            var lines = body.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length - 1; i++)
            {
                var header = lines[i];
                if (!LooksLikeTableRow(header)) continue;
                if (!IsSeparatorRow(lines[i + 1])) continue;

                var headerCells = SplitRow(header);
                var columnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (var c = 0; c < headerCells.Length; c++)
                {
                    if (HeaderAliases.TryGetValue(headerCells[c].Trim(), out var canonical)
                        && !columnIndex.ContainsKey(canonical))
                        columnIndex[canonical] = c;
                }
                if (columnIndex.Count == 0) continue; // intestazioni non riconosciute: non è la nostra tabella

                var rows = new List<string[]>();
                for (var r = i + 2; r < lines.Length; r++)
                {
                    if (!LooksLikeTableRow(lines[r])) break;
                    rows.Add(SplitRow(lines[r]));
                }
                return new MarkdownTable { ColumnIndex = columnIndex, Rows = rows };
            }
            return null;
        }

        private static bool LooksLikeTableRow(string line)
            => !string.IsNullOrWhiteSpace(line) && line.Contains('|');

        private static bool IsSeparatorRow(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains('|')) return false;
            var cells = SplitRow(line);
            return cells.Length > 0 && cells.All(c => Regex.IsMatch(c.Trim(), @"^:?-{1,}:?$"));
        }

        private static string[] SplitRow(string line)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("|")) trimmed = trimmed.Substring(1);
            if (trimmed.EndsWith("|")) trimmed = trimmed.Substring(0, trimmed.Length - 1);
            return trimmed.Split('|').Select(c => c.Trim()).ToArray();
        }
    }

    /// <summary>
    /// Risolve un <b>ambito</b> di ownership nella riga responsabile (§12.6): dato lo scope di
    /// una richiesta di intervento, trova chi ne è responsabile e con quali agenti. Puro e
    /// deterministico — il routing federato NON lo decide l'LLM. Match esatto case-insensitive.
    /// </summary>
    public static class OwnershipResolver
    {
        /// <summary>La riga di ownership per l'ambito, o <c>null</c> se nessuna combacia.</summary>
        public static OwnershipEntry Resolve(IEnumerable<OwnershipEntry> entries, string scope)
        {
            if (entries == null || string.IsNullOrWhiteSpace(scope)) return null;
            var target = scope.Trim();
            return entries.FirstOrDefault(e =>
                e != null && string.Equals(e.Scope?.Trim(), target, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// L'agente proposto per l'ambito: <paramref name="preferredAgent"/> se citato nella riga,
        /// altrimenti il primo agente della riga. <c>null</c> se la riga non ha agenti.
        /// </summary>
        public static string PickAgent(OwnershipEntry entry, string preferredAgent = null)
        {
            if (entry?.Agents == null || entry.Agents.Count == 0) return null;
            if (!string.IsNullOrWhiteSpace(preferredAgent))
            {
                var match = entry.Agents.FirstOrDefault(a =>
                    string.Equals(a?.Trim(), preferredAgent.Trim(), StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
            }
            return entry.Agents[0];
        }
    }

    /// <summary>
    /// Convalide del doc di ownership che dipendono dalle FONTI (§12.3): il responsabile
    /// deve essere un participant del progetto; ogni agente citato deve esistere nel
    /// registry. Separata dal parser (che è puro) per restare testabile con input espliciti.
    /// </summary>
    public static class OwnershipValidator
    {
        /// <summary>
        /// Ritorna gli errori aggiuntivi (oltre a quelli strutturali del parser):
        /// email non tra i participants, agente citato non nel registry.
        /// </summary>
        public static IReadOnlyList<string> Validate(
            OwnershipParseResult parsed,
            IEnumerable<string> mergedParticipantEmails,
            IEnumerable<string> knownAgentNames)
        {
            var errors = new List<string>();
            if (parsed == null || !parsed.IsOwnershipDoc) return errors;

            var participants = new HashSet<string>(
                (mergedParticipantEmails ?? Enumerable.Empty<string>())
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.Trim().ToLowerInvariant()),
                StringComparer.OrdinalIgnoreCase);

            var agents = new HashSet<string>(
                (knownAgentNames ?? Enumerable.Empty<string>())
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Select(a => a.Trim()),
                StringComparer.OrdinalIgnoreCase);

            foreach (var e in parsed.Entries)
            {
                if (!participants.Contains(e.GitEmail))
                    errors.Add($"Ambito '{e.Scope}': il responsabile '{e.GitEmail}' non è tra i participants del progetto.");

                foreach (var agent in e.Agents)
                {
                    if (!agents.Contains(agent))
                        errors.Add($"Ambito '{e.Scope}': l'agente '{agent}' non esiste nel registry del progetto.");
                }
            }

            return errors;
        }
    }
}
