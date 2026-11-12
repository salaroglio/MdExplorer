using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// I valori che una stringa può avere nel punto in cui si usa (12/11/2026, utente: «mettere dentro a delle variabili i nomi
    /// è molto comune, … potrebbe indicare che ci sia un motivo, magari cambiano»; e: «prediligo l'esattezza delle indicazioni
    /// alle approssimazioni, ed anche il fatto che siano tutte»).
    /// <para>
    /// <see cref="Known"/> sono TUTTI i valori che il codice dice, mai una parte: oltre <see cref="MaxValues"/> non se ne
    /// elenca nessuno e <see cref="Unknown"/> lo dice, col motivo. <see cref="Parameters"/>: il valore arriva da un parametro del
    /// metodo, lo dicono i chiamanti. <see cref="Unknown"/>: almeno una strada porta un valore che il codice non dice (una
    /// funzione, la configurazione, il database), con <see cref="Reason"/>.
    /// </para>
    /// </summary>
    public sealed class StringValues
    {
        public const int MaxValues = 256;

        public List<string> Known { get; } = new();
        public bool Unknown { get; private set; }
        /// <summary>Perché almeno un valore non si sa (il primo motivo incontrato); null se si sanno tutti.</summary>
        public string Reason { get; private set; }
        /// <summary>I parametri del metodo (per posizione) da cui il valore può venire: lo dicono i chiamanti.</summary>
        public SortedSet<int> Parameters { get; } = new();
        /// <summary>La variabile, il campo o il parametro attraverso cui il valore passa (il più vicino all'uso); null se è scritto lì.</summary>
        public string Via { get; set; }

        public static StringValues Of(string value) { var v = new StringValues(); v.Known.Add(value); return v; }
        public static StringValues Dynamic(string reason) { var v = new StringValues(); v.MarkUnknown(reason); return v; }

        /// <summary>Tutti i valori sono in <see cref="Known"/>: niente da un parametro, niente che non si sa.</summary>
        public bool IsComplete => !Unknown && Parameters.Count == 0;
        public bool IsSingleKnown => Known.Count == 1 && IsComplete;

        public void MarkUnknown(string reason)
        {
            Unknown = true;
            Reason ??= reason;
        }

        public void Add(StringValues other)
        {
            foreach (var k in other.Known) if (!Known.Contains(k, StringComparer.Ordinal)) Known.Add(k);
            if (other.Unknown) MarkUnknown(other.Reason);
            Parameters.UnionWith(other.Parameters);
            Via ??= other.Via;
            if (Known.Count > MaxValues) { Known.Clear(); MarkUnknown($"more than {MaxValues} possible values"); }
        }

        public StringValues Copy()
        {
            var c = new StringValues { Via = Via, Unknown = Unknown, Reason = Reason };
            c.Known.AddRange(Known);
            c.Parameters.UnionWith(Parameters);
            return c;
        }
    }

    /// <summary>
    /// Il calcolo di <see cref="StringValues"/> sulle operazioni di Roslyn (<see cref="IOperation"/>), le stesse per C# e Visual
    /// Basic. Per una variabile o un parametro contano solo le assegnazioni che ARRIVANO al punto d'uso, sul grafo di controllo
    /// del corpo (<see cref="ControlFlowGraph"/>): rami, cicli, riassegnazioni dopo l'uso. Per un campo, tutte le sue scritture
    /// (inizializzatore e assegnazioni): nel tipo se è privato, in tutti i file se no; un campo che si scrive in modo che non si
    /// segue (una proprietà, un argomento ByRef) non si sa.
    /// </summary>
    public sealed class RoslynStringFlow
    {
        /// <summary>Quanto si scende dentro un'espressione (pezzi, ternari); a ogni variabile si riparte.</summary>
        private const int MaxDepth = 32;
        /// <summary>Quante variabili e campi, uno dentro l'altro, al massimo.</summary>
        private const int MaxChain = 16;

        private readonly Func<SyntaxTree, SemanticModel> _modelOf;
        private readonly IReadOnlyList<SyntaxTree> _trees;
        private readonly Dictionary<IOperation, ControlFlowGraph> _graphs = new(ReferenceEqualityComparer.Instance);
        /// <summary>I valori già calcolati di un uso (la sintassi del riferimento) o di un campo, fuori da un ciclo.</summary>
        private readonly Dictionary<object, StringValues> _done = new();
        /// <summary>I nomi scritti nei file (identificatori), per trovare le scritture di un campo senza rileggere tutto: si costruisce alla prima richiesta.</summary>
        private Dictionary<string, List<SyntaxToken>> _identifiers;

        public RoslynStringFlow(Func<SyntaxTree, SemanticModel> modelOf, IReadOnlyList<SyntaxTree> trees)
        {
            _modelOf = modelOf;
            _trees = trees ?? Array.Empty<SyntaxTree>();
        }

        /// <summary>Il modello semantico di un file analizzato; null fuori dal codice analizzato.</summary>
        public SemanticModel ModelOf(SyntaxTree tree) => _modelOf(tree);

        /// <summary>I valori dell'espressione <paramref name="node"/> (una sintassi), con il modello del suo file.</summary>
        public StringValues Of(SyntaxNode node, SemanticModel model)
        {
            var op = node == null ? null : model.GetOperation(node);
            return op == null ? StringValues.Dynamic("not an expression") : Of(op);
        }

        /// <summary>I valori di un'operazione (un argomento, un valore assegnato).</summary>
        public StringValues Of(IOperation op) => Of(op, new HashSet<object>(), 0);

        private StringValues Of(IOperation op, HashSet<object> visiting, int depth)
        {
            if (op == null) return StringValues.Dynamic("no value");
            if (depth > MaxDepth) return StringValues.Dynamic("expression too deep");
            if (op.ConstantValue.HasValue)
                return op.ConstantValue.Value is string s ? StringValues.Of(s) : op.ConstantValue.Value == null ? new StringValues() : StringValues.Dynamic("not a string");
            switch (op)
            {
                case IConversionOperation c: return Of(c.Operand, visiting, depth + 1);
                case IParenthesizedOperation p: return Of(p.Operand, visiting, depth + 1);
                case IBinaryOperation b when b.OperatorKind is BinaryOperatorKind.Add or BinaryOperatorKind.Concatenate:
                    return Join(Of(b.LeftOperand, visiting, depth + 1), Of(b.RightOperand, visiting, depth + 1));
                case IInterpolatedStringOperation i:
                    {
                        var acc = StringValues.Of(string.Empty);
                        foreach (var part in i.Parts)
                            acc = Join(acc, part switch
                            {
                                IInterpolatedStringTextOperation t => Of(t.Text, visiting, depth + 1),
                                IInterpolationOperation x when x.Alignment == null && x.FormatString == null => Of(x.Expression, visiting, depth + 1),
                                _ => StringValues.Dynamic("formatted interpolation"),
                            });
                        return acc;
                    }
                case IConditionalOperation t when t.WhenFalse != null:
                    {
                        var v = Of(t.WhenTrue, visiting, depth + 1);
                        v.Add(Of(t.WhenFalse, visiting, depth + 1));
                        return v;
                    }
                case ICoalesceOperation co:
                    {
                        var v = Of(co.Value, visiting, depth + 1);
                        v.Add(Of(co.WhenNull, visiting, depth + 1));
                        return v;
                    }
                case IInvocationOperation inv when inv.Instance != null && inv.Arguments.Length == 0
                                                  && inv.TargetMethod.ContainingType?.SpecialType == SpecialType.System_String
                                                  && inv.TargetMethod.Name is "Trim" or "ToUpper" or "ToUpperInvariant" or "ToLower" or "ToLowerInvariant":
                    {
                        var v = Of(inv.Instance, visiting, depth + 1);
                        var mapped = v.Copy();
                        mapped.Known.Clear();
                        foreach (var k in v.Known)
                        {
                            var m = inv.TargetMethod.Name switch { "Trim" => k.Trim(), "ToUpper" or "ToUpperInvariant" => k.ToUpperInvariant(), _ => k.ToLowerInvariant() };
                            if (!mapped.Known.Contains(m, StringComparer.Ordinal)) mapped.Known.Add(m);
                        }
                        return mapped;
                    }
                case IParameterReferenceOperation pr: return Reaching(pr, pr.Parameter, visiting);
                case ILocalReferenceOperation lr: return Reaching(lr, lr.Local, visiting);
                case IFieldReferenceOperation fr: return Field(fr.Field, visiting);
                case IInvocationOperation call: return StringValues.Dynamic($"returned by {call.TargetMethod.Name}()");
                case IPropertyReferenceOperation prop: return StringValues.Dynamic($"read from the property {prop.Property.Name}");
                default: return StringValues.Dynamic("computed at run time");
            }
        }

        /// <summary>Due pezzi uniti: ogni combinazione; un pezzo che non si sa, o che viene da un parametro, rende non saputo il tutto.</summary>
        private static StringValues Join(StringValues a, StringValues b)
        {
            var r = new StringValues { Via = a.Via ?? b.Via };
            if (!a.IsComplete || !b.IsComplete)
            {
                r.MarkUnknown(a.Reason ?? b.Reason ?? "a piece comes from a parameter");
                return r;
            }
            if ((long)a.Known.Count * b.Known.Count > StringValues.MaxValues) { r.MarkUnknown($"more than {StringValues.MaxValues} possible values"); return r; }
            foreach (var x in a.Known)
                foreach (var y in b.Known)
                    if (!r.Known.Contains(x + y, StringComparer.Ordinal)) r.Known.Add(x + y);
            return r;
        }

        // ------------------------------------------------------------------ variabili e parametri: le definizioni che arrivano

        /// <summary>Una scrittura della variabile nel grafo di controllo: il valore (dalla sintassi originale) o «non si sa».</summary>
        private sealed class Definition
        {
            public SyntaxNode Syntax;     // l'assegnazione o la dichiarazione, nella sintassi del sorgente
            public string Unknown;        // il motivo, per una scrittura che non si segue
            public bool Entry;            // il valore all'ingresso del metodo (un parametro: lo dicono i chiamanti)
        }

        private StringValues Reaching(IOperation reference, ISymbol variable, HashSet<object> visiting)
        {
            var key = reference.Syntax;
            if (_done.TryGetValue(key, out var cached)) return cached.Copy();
            var result = new StringValues { Via = variable.Name };
            if (visiting.Count >= MaxChain || !visiting.Add(key)) { result.MarkUnknown("the value goes round a loop of variables"); return result; }
            try
            {
                var graph = GraphOf(reference);
                var defs = graph == null ? null : DefinitionsAt(graph, variable, reference.Syntax);
                if (defs == null) { result.MarkUnknown("the control flow of the body cannot be read"); return result; }
                foreach (var d in defs)
                {
                    if (d.Entry) { if (variable is IParameterSymbol p) result.Parameters.Add(p.Ordinal); continue; }
                    if (d.Unknown != null) { result.MarkUnknown(d.Unknown); continue; }
                    var value = ValueOf(d.Syntax);
                    if (value == null) { result.MarkUnknown("a write that cannot be read"); continue; }
                    result.Add(Of(value, visiting, 0));
                }
                result.Via = variable.Name;
                if (visiting.Count == 1) _done[key] = result.Copy();
                return result;
            }
            finally { visiting.Remove(key); }
        }

        /// <summary>Il grafo di controllo del corpo che contiene l'operazione; null se non si può costruire.</summary>
        private ControlFlowGraph GraphOf(IOperation op)
        {
            var root = op;
            while (root.Parent != null) root = root.Parent;
            if (_graphs.TryGetValue(root, out var g)) return g;
            try
            {
                g = root switch
                {
                    IMethodBodyOperation m => ControlFlowGraph.Create(m),
                    IConstructorBodyOperation c => ControlFlowGraph.Create(c),
                    IBlockOperation b => ControlFlowGraph.Create(b),
                    IFieldInitializerOperation f => ControlFlowGraph.Create(f),
                    IPropertyInitializerOperation p => ControlFlowGraph.Create(p),
                    IParameterInitializerOperation pi => ControlFlowGraph.Create(pi),
                    _ => null,
                };
            }
            catch (ArgumentException) { g = null; }
            return _graphs[root] = g;
        }

        /// <summary>
        /// Le definizioni di <paramref name="variable"/> che arrivano all'uso <paramref name="use"/>: quella prima dell'uso nello
        /// stesso blocco, oppure quelle che arrivano all'ingresso del blocco (le definizioni raggiungenti, fino al punto fisso).
        /// Un blocco senza predecessori che non è l'ingresso (un catch, un finally) riceve tutte le definizioni del corpo.
        /// Null se l'uso non è nel grafo (dentro una lambda: ha un suo grafo, e la variabile è catturata).
        /// </summary>
        private static List<Definition> DefinitionsAt(ControlFlowGraph graph, ISymbol variable, SyntaxNode use)
        {
            var blocks = graph.Blocks;
            var last = new Definition[blocks.Length];                 // l'ultima definizione del blocco
            var all = new List<Definition>();
            (int Block, Definition Before)? useAt = null;
            for (var b = 0; b < blocks.Length; b++)
            {
                Definition current = null;
                var ops = blocks[b].Operations.ToList();
                if (blocks[b].BranchValue != null) ops.Add(blocks[b].BranchValue);
                foreach (var op in ops)
                {
                    // nell'istruzione, l'uso viene prima della scrittura (x = f(x))
                    if (useAt == null && op.DescendantsAndSelf().Any(d => d.Syntax == use && IsReferenceTo(d, variable)))
                        useAt = (b, current);
                    foreach (var d in Writes(op, variable)) { current = d; all.Add(d); }
                }
                last[b] = current;
            }
            if (useAt == null) return null;

            var entry = new Definition { Entry = true };
            var outSets = new HashSet<Definition>[blocks.Length];
            var inSets = new HashSet<Definition>[blocks.Length];
            for (var b = 0; b < blocks.Length; b++) { inSets[b] = new HashSet<Definition>(); outSets[b] = new HashSet<Definition>(); }
            for (var changed = true; changed;)
            {
                changed = false;
                for (var b = 0; b < blocks.Length; b++)
                {
                    var block = blocks[b];
                    var input = new HashSet<Definition>();
                    if (block.Kind == BasicBlockKind.Entry) input.Add(entry);
                    else if (block.Predecessors.Length == 0) { input.Add(entry); input.UnionWith(all); }
                    else foreach (var pred in block.Predecessors) input.UnionWith(outSets[pred.Source.Ordinal]);
                    var output = last[b] != null ? new HashSet<Definition> { last[b] } : input;
                    if (!input.SetEquals(inSets[b]) || !output.SetEquals(outSets[b])) { inSets[b] = input; outSets[b] = output; changed = true; }
                }
            }
            var (useBlock, before) = useAt.Value;
            return before != null ? new List<Definition> { before } : inSets[useBlock].ToList();
        }

        private Dictionary<string, List<SyntaxToken>> Identifiers()
        {
            if (_identifiers != null) return _identifiers;
            var index = new Dictionary<string, List<SyntaxToken>>(StringComparer.OrdinalIgnoreCase);
            foreach (var tree in _trees)
                foreach (var token in tree.GetRoot().DescendantTokens())
                {
                    if (token.Parent == null || token.ValueText.Length == 0 || !(char.IsLetter(token.ValueText[0]) || token.ValueText[0] == '_')) continue;
                    if (!index.TryGetValue(token.ValueText, out var list)) index[token.ValueText] = list = new List<SyntaxToken>();
                    list.Add(token);
                }
            return _identifiers = index;
        }

        private static bool IsReferenceTo(IOperation op, ISymbol variable) => op switch
        {
            ILocalReferenceOperation l => SymbolEqualityComparer.Default.Equals(l.Local, variable),
            IParameterReferenceOperation p => SymbolEqualityComparer.Default.Equals(p.Parameter, variable),
            _ => false,
        };

        /// <summary>Le scritture della variabile in un'istruzione del grafo, nell'ordine.</summary>
        private static IEnumerable<Definition> Writes(IOperation statement, ISymbol variable)
        {
            foreach (var op in statement.DescendantsAndSelf())
            {
                switch (op)
                {
                    case ISimpleAssignmentOperation a when IsReferenceTo(a.Target, variable):
                        yield return new Definition { Syntax = a.Syntax };
                        break;
                    case ICompoundAssignmentOperation c when IsReferenceTo(c.Target, variable):
                        yield return new Definition { Unknown = "the value is built up step by step (&=, +=)" };
                        break;
                    case IIncrementOrDecrementOperation i when IsReferenceTo(i.Target, variable):
                        yield return new Definition { Unknown = "not a string" };
                        break;
                    case IArgumentOperation arg when arg.Parameter != null && arg.Parameter.RefKind != RefKind.None && arg.Value.DescendantsAndSelf().Any(v => IsReferenceTo(v, variable)):
                        yield return new Definition { Unknown = $"written by {(arg.Parent as IInvocationOperation)?.TargetMethod.Name ?? "a call"}() (ByRef)" };
                        break;
                    case IDeconstructionAssignmentOperation dc when dc.Target.DescendantsAndSelf().Any(t => IsReferenceTo(t, variable)):
                        yield return new Definition { Unknown = "written by a deconstruction" };
                        break;
                }
            }
        }

        /// <summary>Il valore assegnato, letto dalla sintassi originale (il grafo di controllo riscrive le espressioni).</summary>
        private IOperation ValueOf(SyntaxNode assignment)
        {
            var model = _modelOf(assignment.SyntaxTree);
            if (model == null) return null;
            for (var n = assignment; n != null; n = n.Parent)
            {
                switch (model.GetOperation(n))
                {
                    case ISimpleAssignmentOperation sa: return sa.Value;
                    case IExpressionStatementOperation { Operation: ISimpleAssignmentOperation sa2 }: return sa2.Value;
                    case IVariableDeclaratorOperation d: return d.Initializer?.Value ?? (d.Parent as IVariableDeclarationOperation)?.Initializer?.Value;
                    case IVariableDeclarationOperation vd when vd.Declarators.Length == 1: return vd.Initializer?.Value ?? vd.Declarators[0].Initializer?.Value;
                    case IForEachLoopOperation: return null;
                    case null: break;
                    default: return null;
                }
                if (n.Parent != null && n.Parent.Span.Length > n.Span.Length * 4 + 200) break;
            }
            return null;
        }

        // ------------------------------------------------------------------ campi

        /// <summary>
        /// Un campo (non costante: le costanti sono già sopra): tutte le sue scritture — l'inizializzatore e le assegnazioni —
        /// nel tipo se è privato, in tutti i file se no. Nessuna scrittura: vale Nothing/null, nessun valore.
        /// </summary>
        private StringValues Field(IFieldSymbol field, HashSet<object> visiting)
        {
            // lo stesso campo visto da due compilazioni (un progetto che riferisce l'altro) è due simboli: si confronta per nome qualificato
            if (field.Type?.SpecialType is not (SpecialType.System_String or SpecialType.System_Object)) return StringValues.Dynamic("not a string");
            var key = "field:" + field.OriginalDefinition.ToDisplayString();
            bool Same(IFieldSymbol f) => f != null && "field:" + f.OriginalDefinition.ToDisplayString() == key;
            if (_done.TryGetValue(key, out var cached)) return cached.Copy();
            var result = new StringValues { Via = field.Name };
            if (visiting.Count >= MaxChain || !visiting.Add(key)) { result.MarkUnknown("the value goes round a loop of fields"); return result; }
            try
            {
                // privato: le scritture sono nel tipo; altrimenti in qualunque file
                var where = field.DeclaredAccessibility == Accessibility.Private
                    ? new HashSet<SyntaxTree>(field.ContainingType.DeclaringSyntaxReferences.Select(r => r.SyntaxTree))
                    : null;
                if (where != null && where.Count == 0) { result.MarkUnknown($"the field {field.Name} is not in the analyzed code"); return result; }
                if (!field.DeclaringSyntaxReferences.Any()) { result.MarkUnknown($"the field {field.Name} is in a library"); return result; }
                Identifiers().TryGetValue(field.Name, out var tokens);
                foreach (var token in tokens ?? new List<SyntaxToken>())
                {
                    var tree = token.SyntaxTree;
                    if (where != null && !where.Contains(tree)) continue;
                    var model = _modelOf(tree);
                    if (model == null) { result.MarkUnknown("a file cannot be read"); continue; }
                    {
                        // la dichiarazione: l'inizializzatore sta ACCANTO al nome (C# «X = …» nel dichiaratore, VB «X As String = …»)
                        if (model.GetDeclaredSymbol(token.Parent) is IFieldSymbol declared && Same(declared))
                        {
                            var scope = token.Parent.Parent ?? token.Parent;
                            foreach (var n in scope.DescendantNodesAndSelf())
                                if (model.GetOperation(n) is IFieldInitializerOperation init && init.InitializedFields.Any(Same)) { result.Add(Of(init.Value, visiting, 0)); break; }
                            continue;
                        }
                        foreach (var n in token.Parent.AncestorsAndSelf().Take(6))
                        {
                            var op = model.GetOperation(n);
                            if (op == null) continue;
                            if (op is IExpressionStatementOperation statement) op = statement.Operation;   // VB: l'assegnazione è un'istruzione
                            if (op is ISimpleAssignmentOperation a && a.Target is IFieldReferenceOperation f1 && Same(f1.Field)) { result.Add(Of(a.Value, visiting, 0)); break; }
                            if (op is ICompoundAssignmentOperation c && c.Target is IFieldReferenceOperation f2 && Same(f2.Field)) { result.MarkUnknown("the value is built up step by step (&=, +=)"); break; }
                            if (op is IArgumentOperation arg && arg.Parameter?.RefKind is RefKind.Ref or RefKind.Out && arg.Value is IFieldReferenceOperation f3 && Same(f3.Field)) { result.MarkUnknown("written by a call (ByRef)"); break; }
                            if (op is IFieldReferenceOperation) continue;   // una lettura: si sale a vedere se è il bersaglio di una scrittura
                        }
                    }
                }
                result.Via = field.Name;
                if (visiting.Count == 1) _done[key] = result.Copy();
                return result;
            }
            finally { visiting.Remove(key); }
        }
    }
}
