using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Diagnostics
{
    /// <summary>Chi è la persona sul computer: serve a togliere il suo nome e la sua cartella dai log.</summary>
    public sealed class RedactionContext
    {
        /// <summary>Il nome utente del sistema (<c>Environment.UserName</c>, per esempio <c>SLRCLM72P</c> o <c>carlo</c>).</summary>
        public string UserName { get; init; }

        /// <summary>Il dominio, se c'è (<c>Environment.UserDomainName</c>, per esempio <c>NCO</c>).</summary>
        public string Domain { get; init; }

        /// <summary>La cartella dell'utente (<c>C:\Users\SLRCLM72P</c>, <c>/home/carlo</c>).</summary>
        public string UserProfile { get; init; }

        /// <summary>
        /// Altri nomi della persona: il <c>user.name</c> di git, la parte prima della @ della sua email. Riconosciuti con qualunque
        /// separatore (<c>carlo.salaroglio</c>, <c>carlo-salaroglio_dedagrp</c>, <c>Carlo Salaroglio</c>): 06/11/2026, 291 volte nel
        /// log di un giorno.
        /// </summary>
        public IReadOnlyList<string> OtherNames { get; init; } = Array.Empty<string>();
    }

    /// <summary>Il testo oscurato e quante sostituzioni per tipo, da mostrare nell'anteprima.</summary>
    public sealed class RedactionResult
    {
        public string Text { get; init; }
        public IReadOnlyDictionary<string, int> Counts { get; init; }
        public int Total => Counts.Values.Sum();
    }

    /// <summary>
    /// L'oscuramento dei log prima di spedirli (sprint «Invia diagnostica», 06/11/2026). Toglie ciò che non deve lasciare il
    /// computer — email, nome e cartella della persona, segreti riconoscibili — e lascia ciò che serve a capire: i percorsi dei
    /// progetti restano, e l'anteprima li mostra prima dell'invio.
    /// <para>
    /// Sbaglia per eccesso: un falso positivo rende una riga meno leggibile, un falso negativo spedisce un segreto. L'ordine
    /// conta: prima i segreti (che possono contenere una email, come le URL con credenziali), poi la cartella (che contiene il
    /// nome), poi il nome da solo.
    /// </para>
    /// </summary>
    public static class DiagnosticsRedactor
    {
        public const string Email = "email";
        public const string User = "utente";
        public const string Secret = "segreto";

        private const RegexOptions Opt = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        /// <summary>Segreti con una forma propria: si sostituisce tutto il valore.</summary>
        private static readonly Regex[] KnownSecrets =
        {
            new Regex(@"\bgh[pousr]_[A-Za-z0-9]{20,}", Opt),                 // token GitHub
            new Regex(@"\bgithub_pat_[A-Za-z0-9_]{20,}", Opt),
            new Regex(@"\bsk-[A-Za-z0-9_\-]{16,}", Opt),                      // chiavi OpenAI / Anthropic (sk-ant-…)
            new Regex(@"\bxox[abprs]-[A-Za-z0-9\-]{10,}", Opt),              // Slack
            new Regex(@"\bAIza[0-9A-Za-z_\-]{30,}", Opt),                     // Google
            new Regex(@"\b\d{8,10}:[A-Za-z0-9_\-]{35}\b", Opt),               // token di un bot Telegram
            new Regex(@"\beyJ[A-Za-z0-9_\-]{10,}\.[A-Za-z0-9_\-]{10,}\.[A-Za-z0-9_\-]{10,}", Opt), // JWT
        };

        /// <summary><c>Bearer xxx</c>, <c>Basic xxx</c>: resta la parola, sparisce il valore.</summary>
        private static readonly Regex AuthHeader = new Regex(@"\b(Bearer|Basic)\s+[A-Za-z0-9._~+/=\-]{8,}", Opt | RegexOptions.IgnoreCase);

        /// <summary><c>chiave = valore</c> o <c>chiave: valore</c> con un nome da segreto: resta la chiave, sparisce il valore.</summary>
        private static readonly Regex KeyValue = new Regex(
            @"(?<key>\b[A-Za-z_\-]*(api[_\-]?key|apikey|token|password|passwd|pwd|secret|roomsecret|x-api-key|authorization|client[_\-]?secret|pat)\b[""']?\s*[:=]\s*[""']?)(?<value>[^\s""',;&}\]]{4,})",
            Opt | RegexOptions.IgnoreCase);

        /// <summary><c>https://utente:password@host</c>: le credenziali dentro una URL.</summary>
        private static readonly Regex UrlCredentials = new Regex(@"(?<scheme>\b[a-z][a-z0-9+\-.]*://)[^/\s:@]+:[^/\s@]+@", Opt | RegexOptions.IgnoreCase);

        private static readonly Regex EmailAddress = new Regex(@"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b", Opt);

        public static RedactionResult Redact(string text, RedactionContext context)
        {
            var counts = new Dictionary<string, int> { [Secret] = 0, [Email] = 0, [User] = 0 };
            if (string.IsNullOrEmpty(text)) return new RedactionResult { Text = text ?? string.Empty, Counts = counts };

            string Replace(string input, Regex regex, string category, MatchEvaluator evaluator)
                => regex.Replace(input, m => { counts[category]++; return evaluator(m); });

            var s = text;
            s = Replace(s, UrlCredentials, Secret, m => m.Groups["scheme"].Value + "<segreto>@");
            foreach (var r in KnownSecrets) s = Replace(s, r, Secret, _ => "<segreto>");
            s = Replace(s, AuthHeader, Secret, m => m.Groups[1].Value + " <segreto>");
            // «Authorization: Bearer <segreto>»: la regola di sopra ha già tolto il valore, qui «Bearer» è solo lo schema.
            s = KeyValue.Replace(s, m =>
            {
                var value = m.Groups["value"].Value;
                if (m.Value.Contains("<segreto>") || value.Equals("Bearer", StringComparison.OrdinalIgnoreCase) || value.Equals("Basic", StringComparison.OrdinalIgnoreCase))
                    return m.Value;
                counts[Secret]++;
                return m.Groups["key"].Value + "<segreto>";
            });
            s = Replace(s, EmailAddress, Email, _ => "<email>");

            foreach (var form in ProfileForms(context?.UserProfile))
                s = Replace(s, new Regex(Regex.Escape(form), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), User, _ => "<utente>");

            foreach (var name in (context?.OtherNames ?? Array.Empty<string>()).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var parts = Regex.Split(name.Trim(), @"[._\-\s]+").Where(p => p.Length > 0).Select(Regex.Escape).ToArray();
                if (string.Join("", parts).Length < 4) continue; // un nome troppo corto oscurerebbe mezzo log
                var pattern = @"(?<![A-Za-z0-9])" + string.Join(@"[._\-\s]", parts) + @"(?![A-Za-z0-9])";
                s = Replace(s, new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), User, _ => "<utente>");
            }

            var user = context?.UserName;
            if (!string.IsNullOrWhiteSpace(user) && user.Length >= 3)
            {
                if (!string.IsNullOrWhiteSpace(context.Domain))
                    s = Replace(s, new Regex(Regex.Escape(context.Domain) + @"\\{1,2}" + Regex.Escape(user), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), User, _ => "<utente>");
                s = Replace(s, new Regex(@"(?<![A-Za-z0-9])" + Regex.Escape(user) + @"(?![A-Za-z0-9])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), User, _ => "<utente>");
            }

            return new RedactionResult { Text = s, Counts = counts };
        }

        /// <summary>La cartella dell'utente come compare nei log: con <c>\</c>, con <c>/</c>, e con <c>\\</c> nei JSON.</summary>
        private static IEnumerable<string> ProfileForms(string profile)
        {
            if (string.IsNullOrWhiteSpace(profile)) yield break;
            var p = profile.TrimEnd('/', '\\');
            if (p.Length < 4) yield break; // «/» o «C:\» non sono una cartella utente
            var forms = new[] { p, p.Replace('\\', '/'), p.Replace('/', '\\'), p.Replace("\\", "\\\\") };
            foreach (var f in forms.Distinct(StringComparer.OrdinalIgnoreCase).OrderByDescending(f => f.Length)) yield return f;
        }
    }
}
