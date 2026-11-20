using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SpanControls
{
    public class Foo { public Guid Id { get; set; } public string Name { get; set; } }

    /// <summary>
    /// Due tipi che non sono MemoryExtensions, con la stessa estensione su IEnumerable e su ReadOnlySpan: è la forma di
    /// Enumerable.Contains / MemoryExtensions.Contains. Con C# 13 un array non si converte in span per un'estensione e vince
    /// quella su IEnumerable; con C# 14 vince quella a span. Il corpo a span non usa MemoryExtensions (nessuna riga in più).
    /// </summary>
    public static class EnumerableHelpers
    {
        public static bool Has(this IEnumerable<Guid> s, Guid x) { foreach (var v in s) if (v == x) return true; return false; }
    }

    public static class SpanHelpers
    {
        public static bool Has(this ReadOnlySpan<Guid> s, Guid x) { for (var i = 0; i < s.Length; i++) if (s[i] == x) return true; return false; }
    }

    /// <summary>
    /// Casi di controllo dello strumento d'inventario. Esito ATTESO:
    ///   LangVersion 14: 5 righe «in query: sì» (casi A, B, C, D di tipo memext; H di tipo span) e 1 riga «in query: no»
    ///   (caso E); nessuna per F e G.
    ///   LangVersion 13: zero righe (nessuna conversione implicita da array a span). Più il caso I in fondo al file.
    /// </summary>
    public static class Cases
    {
        public static void All(Guid[] ids, string[] names, IQueryable<Foo> q, List<Guid> list)
        {
            // A — il caso del piano: un'espressione esplicita
            Expression<Func<Foo, bool>> a = f => ids.Contains(f.Id);
            // B — dentro una query LINQ con il metodo Where di IQueryable
            var b = q.Where(f => ids.Contains(f.Id)).ToList();
            // C — la sintassi di query
            var c = (from f in q where names.Contains(f.Name) select f).ToList();
            // D — un altro metodo che esiste in entrambe le classi (SequenceEqual)
            Expression<Func<Foo, bool>> d = f => ids.SequenceEqual(new[] { f.Id });
            // E — fuori da una query: delegato normale, la forma nuova è giusta
            Func<Foo, bool> e = f => ids.Contains(f.Id);
            // F — una lista: nessuna conversione a span, nessuna riga
            Expression<Func<Foo, bool>> g = f => list.Contains(f.Id);
            // G — la correzione del piano: l'array diventa lista prima della query
            var idList = ids.ToList();
            Expression<Func<Foo, bool>> h = f => idList.Contains(f.Id);
            // H — un metodo di un ALTRO tipo con un sovraccarico a span, dentro un'espressione: riga di tipo «span»
            Expression<Func<Foo, bool>> k = f => ids.Has(f.Id);
            GC.KeepAlive(new object[] { a, b, c, d, e, g, h, k });
        }
    }
}

namespace SpanControls
{
    /// <summary>
    /// Caso I — la parola chiave `field` di C# 14. Con C# 13 `field` qui è il campo chiamato `field` della classe; con C# 14
    /// è il campo nascosto della proprietà Name. Esito ATTESO: una riga di tipo «field» con LangVersion 14, zero con 13.
    /// </summary>
    public class FieldKeyword
    {
        private string field = "membro";
        public string Name { get => field; set { } }
    }
}
