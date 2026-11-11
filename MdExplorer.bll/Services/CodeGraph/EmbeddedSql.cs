using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Che cosa una frase SQL fa a un oggetto del database.</summary>
    public enum SqlAccess { Read, Write, Call }

    /// <summary>Un oggetto nominato da una frase SQL; <see cref="Dynamic"/> = il nome non è scritto, arriva da una variabile.</summary>
    public sealed record SqlObject(string Name, SqlAccess Access, bool Dynamic);

    /// <summary>
    /// L'SQL scritto dentro le stringhe del codice (11/11/2026): quali tabelle legge e scrive, quali procedure chiama. Non è
    /// un parser SQL: riconosce le forme che nominano un oggetto (<c>FROM x</c>, <c>JOIN x</c>, <c>INSERT INTO x</c>,
    /// <c>UPDATE x SET</c>, <c>DELETE FROM x</c>, <c>MERGE INTO x</c>, <c>CALL x(</c>, <c>BEGIN x(</c>), anche in un pezzo di
    /// frase (<c>" FROM ORDINI O "</c> aggiunto a una variabile). Il testo arriva con <see cref="Hole"/> al posto delle parti
    /// non scritte (una variabile concatenata): un nome che cade lì è <see cref="SqlObject.Dynamic"/>.
    /// <para>
    /// Sbaglia per difetto: una stringa che comincia come una frase (<c>"Update available from server"</c>) ma non ha la
    /// forma del suo comando non dà niente. Gli archi che ne nascono sono <c>inferred</c>.
    /// </para>
    /// </summary>
    public static class EmbeddedSql
    {
        /// <summary>Il segnaposto di una parte non scritta (non <c>?</c>: è il parametro di OleDb).</summary>
        public const char Hole = '\u0001';

        /// <summary>Oltre non si legge: una stringa così lunga non è una frase scritta a mano.</summary>
        public const int MaxLength = 20000;

        private const RegexOptions Opt = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
        private static readonly TimeSpan Limit = TimeSpan.FromSeconds(1);

        private const string Part = @"(?:[A-Za-z_][\w$#]*|""[^""]+""|\[[^\]]+\]|`[^`]+`)";
        private const string Id = @"(?<n>" + Part + @"(?:\s*\.\s*" + Part + @"){0,2}(?:@[\w.]+)?|\u0001)";

        private static readonly Regex Start = new(@"^[\s(]*(?<k>SELECT|INSERT|UPDATE|DELETE|MERGE|WITH|BEGIN|DECLARE|CALL|EXEC|EXECUTE|TRUNCATE|\{)(?![\w$#])", Opt, Limit);
        private static readonly Regex Piece = new(@"^[\s,)]*(?:FROM|(?:(?:LEFT|RIGHT|FULL|INNER|CROSS|NATURAL)\s+(?:OUTER\s+)?)?JOIN|UNION(?:\s+ALL)?\s+SELECT)\b", Opt, Limit);
        private static readonly Regex Literal = new(@"'(?:[^']|'')*'", Opt, Limit);
        private static readonly Regex Comment = new(@"--[^\n]*|/\*.*?\*/", Opt | RegexOptions.Singleline, Limit);
        private static readonly Regex Space = new(@"\s+", Opt, Limit);
        /// <summary><c>EXTRACT(YEAR FROM d)</c>, <c>TRIM(' ' FROM x)</c>: quel FROM non nomina una tabella.</summary>
        private static readonly Regex FunctionFrom = new(@"\b(?:EXTRACT|TRIM|SUBSTRING|OVERLAY)\s*\([^()]*?\bFROM\b", Opt, Limit);

        private static readonly Regex[] Writes =
        {
            new(@"\bINSERT\s+(?:ALL\s+|FIRST\s+)?INTO\s+" + Id, Opt, Limit),
            new(@"\bMERGE\s+INTO\s+" + Id, Opt, Limit),
            new(@"(?<!\bFOR\s)(?<!\bON\s)\bUPDATE\s+" + Id + @"(?=\s+(?:[A-Za-z_]\w*\s+)?SET\b|\s*$)", Opt, Limit),
            new(@"(?<!\bON\s)\bDELETE\s+FROM\s+" + Id, Opt, Limit),
            new(@"^[\s(]*DELETE\s+(?!FROM\b)" + Id, Opt, Limit),
            new(@"\bTRUNCATE\s+TABLE\s+" + Id, Opt, Limit),
        };
        private static readonly Regex From = new(@"\bFROM\s+", Opt, Limit);
        private static readonly Regex Item = new(@"\G\s*" + Id + @"(?:\s+(?:AS\s+)?(?<a>[A-Za-z_]\w*))?\s*(?<more>,)?", Opt, Limit);
        private static readonly Regex Cte = new(@"(?:\bWITH\s+(?:RECURSIVE\s+)?|\)\s*,\s*)(?<n>[A-Za-z_]\w*)\s*(?:\([^()]*\)\s*)?AS\s*\(", Opt, Limit);
        private static readonly Regex AfterSubquery = new(@"\G\s*(?:(?:AS\s+)?(?<a>[A-Za-z_]\w*))?\s*(?<more>,)?", Opt, Limit);
        private static readonly Regex[] Reads =
        {
            new(@"\bJOIN\s+" + Id, Opt, Limit),
            new(@"\bUSING\s+" + Id + @"(?=\s+(?:[A-Za-z_]\w*\s+)?ON\b)", Opt, Limit),
        };
        private static readonly Regex[] Calls =
        {
            new(@"\bCALL\s+" + Id + @"\s*(?:\(|\}|$)", Opt, Limit),
            // «Execute order» è una frase: il nome di una procedura ha un punto o un trattino basso, o è seguito dai suoi argomenti
            new(@"\bEXEC(?:UTE)?\s+(?!IMMEDIATE\b)(?:@\w+\s*=\s*)?(?=\S*[._]|\S+\s*[@';\d])" + Id, Opt, Limit),
            new(@"\bBEGIN\s+(?:[:\w.]+\s*:=\s*)?" + Id + @"\s*[(;]", Opt, Limit),
        };

        /// <summary>Parole che seguono FROM/JOIN/BEGIN e non sono un oggetto, o che dopo un nome non sono il suo alias.</summary>
        private static readonly HashSet<string> NotAName = new(StringComparer.OrdinalIgnoreCase)
        {
            "DUAL", "SELECT", "TABLE", "LATERAL", "ONLY", "UNNEST", "THE", "XMLTABLE", "JSON_TABLE", "VALUES",
            "IF", "FOR", "WHILE", "LOOP", "NULL", "RETURN", "DECLARE", "BEGIN", "INSERT", "UPDATE", "DELETE", "MERGE", "COMMIT", "ROLLBACK",
            "OPEN", "CLOSE", "FETCH", "EXECUTE", "CASE", "RAISE", "LOCK", "SAVEPOINT", "SET", "WITH", "TRANSACTION", "TRAN", "TRY",
        };
        private static readonly HashSet<string> NotAnAlias = new(StringComparer.OrdinalIgnoreCase)
        {
            "WHERE", "ON", "JOIN", "LEFT", "RIGHT", "FULL", "INNER", "CROSS", "NATURAL", "OUTER", "GROUP", "ORDER", "HAVING", "UNION", "MINUS",
            "INTERSECT", "EXCEPT", "CONNECT", "START", "FOR", "FETCH", "OFFSET", "LIMIT", "SET", "USING", "PARTITION", "SAMPLE", "PIVOT", "UNPIVOT", "WITH", "RETURNING",
        };

        /// <summary>
        /// Gli oggetti che il testo nomina; vuoto se non è SQL. <paramref name="text"/> è la stringa com'è nel codice, con
        /// <see cref="Hole"/> al posto di ogni parte non scritta.
        /// </summary>
        public static IReadOnlyList<SqlObject> Analyze(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 8 || text.Length > MaxLength || !text.Any(char.IsWhiteSpace)) return Array.Empty<SqlObject>();
            try { return Read(text); }
            catch (RegexMatchTimeoutException) { return Array.Empty<SqlObject>(); }
        }

        private static IReadOnlyList<SqlObject> Read(string raw)
        {
            var start = Start.Match(raw);
            if (!start.Success && !Piece.IsMatch(raw)) return Array.Empty<SqlObject>();
            // una frase per una persona, non per il database: «Select a file from the list.»
            var trimmed = raw.TrimEnd();
            if (trimmed.EndsWith('.') || trimmed.EndsWith('!') || trimmed.EndsWith("...", StringComparison.Ordinal)) return Array.Empty<SqlObject>();

            var sql = Space.Replace(Literal.Replace(Comment.Replace(raw, " "), "''"), " ");
            // WITH t AS (…): t è un nome della frase, non una tabella
            var local = Cte.Matches(sql).Select(m => m.Groups["n"].Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var found = new List<SqlObject>();
            var seen = new HashSet<(string, SqlAccess)>();
            void Add(string written, SqlAccess access)
            {
                var dynamic = written.Length == 1 && written[0] == Hole;
                var name = dynamic ? "?" : Clean(written);
                if (!dynamic && (name.Length == 0 || NotAName.Contains(name) || (access == SqlAccess.Read && local.Contains(name)))) return;
                if (seen.Add((name.ToUpperInvariant(), access))) found.Add(new SqlObject(name, access, dynamic));
            }

            var deletes = new HashSet<int>();   // i FROM di «DELETE FROM x»: una scrittura, non una lettura
            foreach (var w in Writes)
                foreach (Match m in w.Matches(sql))
                {
                    Add(m.Groups["n"].Value, SqlAccess.Write);
                    var from = From.Match(sql, m.Index, m.Length);
                    if (from.Success) deletes.Add(from.Index);
                }
            var writes = found.Count;

            var skipped = FunctionFrom.Matches(sql).Select(m => m.Index + m.Length - 4).ToHashSet();
            foreach (Match f in From.Matches(sql))
            {
                if (deletes.Contains(f.Index) || skipped.Contains(f.Index)) continue;
                // FROM A a, B b, (SELECT …): l'elenco finché c'è una virgola; una parentesi è una sottoquery, col suo FROM
                for (var at = f.Index + f.Length; at < sql.Length;)
                {
                    var open = at;
                    while (open < sql.Length && sql[open] == ' ') open++;
                    Match item;
                    if (open < sql.Length && sql[open] == '(')
                    {
                        // una sottoquery fra le tabelle: si salta fino alla sua parentesi (il suo FROM si legge a parte)
                        var depth = 0; var close = open;
                        for (; close < sql.Length; close++)
                        {
                            if (sql[close] == '(') depth++;
                            else if (sql[close] == ')' && --depth == 0) break;
                        }
                        if (close >= sql.Length) break;
                        item = AfterSubquery.Match(sql, close + 1);
                    }
                    else
                    {
                        item = Item.Match(sql, at);
                        if (!item.Success) break;
                        Add(item.Groups["n"].Value, SqlAccess.Read);
                    }
                    var alias = item.Groups["a"];
                    if (alias.Success && NotAnAlias.Contains(alias.Value)) break;
                    if (!item.Groups["more"].Success) break;
                    at = item.Index + item.Length;
                }
            }
            foreach (var r in Reads)
                foreach (Match m in r.Matches(sql)) Add(m.Groups["n"].Value, SqlAccess.Read);
            var reads = found.Count - writes;
            foreach (var c in Calls)
                foreach (Match m in c.Matches(sql)) Add(m.Groups["n"].Value, SqlAccess.Call);
            var calls = found.Count - writes - reads;

            // la parola con cui comincia deve avere la forma del suo comando, o è una frase qualunque
            var ok = !start.Success || start.Groups["k"].Value.ToUpperInvariant() switch
            {
                "SELECT" or "WITH" => reads > 0,
                "INSERT" or "UPDATE" or "DELETE" or "MERGE" or "TRUNCATE" => writes > 0,
                "CALL" or "EXEC" or "EXECUTE" or "{" => calls > 0,
                _ => found.Count > 0,   // BEGIN, DECLARE: un blocco, con quello che contiene
            };
            return ok ? found : Array.Empty<SqlObject>();
        }

        /// <summary><c>"SCHEMA" . [Tabella]</c> → <c>SCHEMA.Tabella</c>: senza virgolette né spazi attorno al punto.</summary>
        private static string Clean(string written)
        {
            var sb = new StringBuilder(written.Length);
            foreach (var ch in written)
                if (ch != '"' && ch != '[' && ch != ']' && ch != '`' && !char.IsWhiteSpace(ch)) sb.Append(ch);
            return sb.ToString();
        }
    }
}
