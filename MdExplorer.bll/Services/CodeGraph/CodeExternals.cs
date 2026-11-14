using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Gli archi verso un esterno per una relazione (dal database).</summary>
    public sealed class CodeExternalUse
    {
        public string QualifiedName { get; set; }
        public string Relation { get; set; }
        public int Uses { get; set; }
        public int Callers { get; set; }
        /// <summary>Un punto d'uso, <c>percorso:riga</c>.</summary>
        public string Example { get; set; }
    }

    /// <summary>Un tipo esterno usato dal codice: di lui si sa solo il nome (D43 F3).</summary>
    public sealed class CodeExternalType
    {
        /// <summary>Il nome qualificato del tipo; <c>?</c> per le chiamate di cui nemmeno il tipo è noto.</summary>
        public string Type { get; init; }
        public string Package { get; init; }
        public int Uses { get; set; }
        public int Callers { get; set; }
        /// <summary>I membri usati (metodi chiamati), dal più usato.</summary>
        public List<(string Name, int Uses)> Members { get; } = new();
        public string Example { get; set; }
    }

    /// <summary>
    /// Le chiamate esterne di cui non si sa niente (D43 F3, utente 30/10/2026: «identificare facilmente le chiamate esterne di
    /// cui non si sa nulla»): gli usi degli esterni raggruppati per tipo, i membri chiamati sotto il loro tipo. Un esterno
    /// bersaglio di <c>calls</c> è un membro (il tipo è il nome senza l'ultima parte); degli altri archi è il tipo stesso.
    /// </summary>
    public static class CodeExternals
    {
        public const string UnknownOwner = "?";

        public static List<CodeExternalType> Group(IEnumerable<CodeExternalUse> uses)
        {
            var byType = new Dictionary<string, CodeExternalType>(StringComparer.Ordinal);
            foreach (var u in uses.Where(u => !string.IsNullOrEmpty(u.QualifiedName)))
            {
                var member = u.Relation == "calls";
                var dot = u.QualifiedName.LastIndexOf('.');
                var type = member ? (dot > 0 ? u.QualifiedName[..dot] : UnknownOwner) : u.QualifiedName;
                if (!byType.TryGetValue(type, out var t))
                {
                    var pdot = type.LastIndexOf('.');
                    byType[type] = t = new CodeExternalType { Type = type, Package = type == UnknownOwner || pdot <= 0 ? null : type[..pdot] };
                }
                t.Uses += u.Uses;
                t.Callers = Math.Max(t.Callers, u.Callers);   // per relazione: il massimo è un limite basso onesto, la somma conterebbe due volte
                if (t.Example == null || string.CompareOrdinal(u.Example, t.Example) < 0) t.Example = u.Example;
                if (member)
                {
                    var name = dot >= 0 ? u.QualifiedName[(dot + 1)..] : u.QualifiedName;
                    var i = t.Members.FindIndex(m => m.Name == name);
                    if (i >= 0) t.Members[i] = (name, t.Members[i].Uses + u.Uses); else t.Members.Add((name, u.Uses));
                }
            }
            foreach (var t in byType.Values) t.Members.Sort((a, b) => b.Uses != a.Uses ? b.Uses.CompareTo(a.Uses) : string.CompareOrdinal(a.Name, b.Name));
            return byType.Values.OrderByDescending(t => t.Uses).ThenBy(t => t.Type, StringComparer.Ordinal).ToList();
        }
    }
}
