using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'estrattore Visual Basic .NET (11/11/2026): il gemello di <see cref="RoslynCSharpExtractor"/>, sul compilatore VB di
    /// Roslyn. Stessi simboli e stessi archi; i nomi qualificati e le firme sono scritti come li scrive VB
    /// (<c>Demo.Foo.Bar(String, Integer)</c>). Un <c>Module</c> è una classe statica.
    /// <list type="bullet">
    /// <item>Dal <c>.vbproj</c> si prendono <c>RootNamespace</c>, <c>Imports</c>, <c>Option</c>, <c>MyType</c>, le librerie e i
    /// progetti riferiti; senza un file di progetto valgono gli <c>Imports</c> di un progetto nuovo e <c>Option Infer On</c>.</item>
    /// <item>In VB le parentesi di una chiamata sono facoltative e <c>a(1)</c> è anche l'accesso a un vettore: una chiamata è
    /// ciò che Roslyn risolve in un metodo, comunque sia scritta; <c>AddressOf Metodo</c> è una chiamata (come i
    /// riferimenti a metodo di Java, F4d).</item>
    /// </list>
    /// </summary>
    public sealed class RoslynVisualBasicExtractor : RoslynExtractorBase
    {
        public override string LanguageId => CodeLanguages.VisualBasic;
        /// <summary>2 = file di progetto (RootNamespace, Imports, librerie, .NET Framework), eventi, nomi assegnati e costanti (11/11/2026).
        /// 3 = l'SQL scritto nelle stringhe: tabelle lette e scritte, procedure chiamate (<see cref="EmbeddedSql"/>).
        /// 4 = quei nodi dicono il linguaggio <c>sql</c>, e si ricollegano al simbolo vero di un linguaggio database (12/11/2026).
        /// 5 = le procedure chiamate per nome con <c>CommandType.StoredProcedure</c> (12/11/2026).
        /// 6 = i valori delle stringhe attraverso variabili, costanti, rami e parametri (<see cref="RoslynStringFlow"/>, 12/11/2026).
        /// 7 = un nome fatto da più pezzi con più valori è <c>ambiguous</c> (combinazioni non verificate).
        /// 8 = i progetti di Visual Studio 2002/2003 (riferimenti per GUID, librerie per HintPath); con una libreria non risolta il
        /// nome della procedura non si deduce dal primo argomento stringa (12/11/2026, rilevazione dal cliente).
        /// 9 = una libreria che manca (progetto riferito assente, HintPath inesistente) presa da un altro progetto con lo stesso nome di assembly.
        /// 10 = se nessun progetto la riferisce, la DLL con quel nome di assembly che sta nella cartella analizzata (13/11/2026).
        /// 11 = <c>Handles MyBase.Load</c> registrato (punto di chiamata <c>handles</c>) per la freccia pagina → gestore; AddHandler con il
        /// controllo come Handles; <c>AddressOf</c> in AddHandler/RemoveHandler non è una chiamata.
        /// 12 = i file che il progetto non compila (non elencati nel .vbproj) restano senza simboli (13/11/2026).
        /// 13 = niente archi verso i simboli che il compilatore crea da solo (il delegato di un Event, il costruttore di default).
        /// 14 = un nome esterno scritto con maiuscole diverse è un nodo solo: il VB non le distingue (13/11/2026, rilevazione dal cliente).
        /// 15 = <c>Shared Sub New</c> ha un id suo (<c>X..cctor()</c>), non quello di <c>Sub New()</c> (13/11/2026).
        /// 16 = stesso nome qualificato del C# per le implementazioni esplicite (formato comune).
        /// 17 = i <c>Delegate</c> sono tipi dichiarati (13/11/2026).
        /// 18 = l'id di un Event è il nome senza parametri: Handles da un altro progetto arriva all'evento dichiarato (13/11/2026).
        /// 19 = i punti di chiamata dicono la classe del metodo chiamato e, di un percorso che non si sa, il nome del file (13/11/2026).
        /// 20 = campi omonimi di classi diverse restano distinti nel seguire le stringhe: ognuno col suo catalogo (14/11/2026, dal cliente).
        /// 21 = un GUID di più progetti (una copia annidata) si risolve con la soluzione .sln che contiene chi riferisce (14/11/2026, dal cliente).
        /// 22 = New esegue il costruttore (calls); una chiamata virtuale arriva agli override nel perimetro (inferred) (14/11/2026, dal cliente).</summary>
        public override string ExtractorVersion => "22";

        protected override bool IgnoresCase => true;

        private static readonly VisualBasicParseOptions ParseOptions = new(LanguageVersion.Latest, DocumentationMode.None);

        /// <summary>Gli <c>Imports</c> di progetto di un progetto VB nuovo: senza, <c>List(Of T)</c> o <c>MsgBox</c> non si risolvono.</summary>
        private static readonly IReadOnlyList<GlobalImport> ProjectImports = GlobalImport.Parse(
            "Microsoft.VisualBasic", "System", "System.Collections", "System.Collections.Generic", "System.Data",
            "System.Diagnostics", "System.Linq", "System.Xml.Linq", "System.Threading.Tasks").ToList();

        public RoslynVisualBasicExtractor(ILogger<RoslynVisualBasicExtractor> logger) : base(logger) { }

        protected override string ProjectExtension => ".vbproj";

        /// <summary><c>MyType</c> del progetto accende lo spazio dei nomi <c>My</c> (<c>My.Computer</c>, <c>My.Application</c>…), che il compilatore si porta dentro.</summary>
        private static VisualBasicParseOptions OptionsOf(DotNetProject project)
            => string.IsNullOrEmpty(project?.MyType) ? ParseOptions : ParseOptions.WithPreprocessorSymbols(new KeyValuePair<string, object>("_MyType", project.MyType));

        protected override SyntaxTree Parse(string text, string path, DotNetProject project) => VisualBasicSyntaxTree.ParseText(text, OptionsOf(project), path: path);

        /// <summary>
        /// Con un <c>.vbproj</c>: il suo <c>RootNamespace</c> (nel VB di una volta i file non hanno <c>Namespace</c>, i tipi
        /// prendono quello del progetto), i suoi <c>Imports</c> e le sue <c>Option</c>. Senza, quelli di un progetto nuovo.
        /// </summary>
        protected override Compilation Compile(string name, IReadOnlyList<SyntaxTree> trees, IReadOnlyList<MetadataReference> references, DotNetProject project)
        {
            var imports = project is { Imports.Count: > 0 } ? GlobalImport.Parse(project.Imports).ToList() : ProjectImports;
            var options = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, globalImports: imports,
                rootNamespace: IsNamespace(project?.RootNamespace) ? project.RootNamespace : string.Empty,
                optionInfer: project?.OptionInfer ?? true, optionExplicit: project?.OptionExplicit ?? true,
                optionStrict: project?.OptionStrict == true ? OptionStrict.On : OptionStrict.Off,
                parseOptions: OptionsOf(project));
            return VisualBasicCompilation.Create(name, trees, references, options);
        }

        /// <summary>Un nome di spazio dei nomi valido: uno sbagliato nel file di progetto fermerebbe la compilazione.</summary>
        private static bool IsNamespace(string name)
            => !string.IsNullOrWhiteSpace(name) && name.Split('.').All(part => SyntaxFacts.IsValidIdentifier(part));

        /// <summary>Le classi, i moduli, le interfacce, gli enum… e i <c>Delegate</c> (13/11/2026: chi li usava puntava a un tipo mai dichiarato).</summary>
        protected override IEnumerable<SyntaxNode> TypeDeclarations(SyntaxNode root)
            => root.DescendantNodes().Where(n => n is TypeBlockSyntax or EnumBlockSyntax or DelegateStatementSyntax);

        protected override INamedTypeSymbol TypeSymbol(SemanticModel model, SyntaxNode typeDeclaration) => typeDeclaration switch
        {
            TypeBlockSyntax t => model.GetDeclaredSymbol(t.BlockStatement),
            EnumBlockSyntax e => model.GetDeclaredSymbol(e.EnumStatement),
            DelegateStatementSyntax d => model.GetDeclaredSymbol(d) as INamedTypeSymbol,
            _ => null,
        };

        /// <summary>
        /// Per un metodo Roslyn dà la riga <c>Sub …</c>, il corpo sta nel blocco che la contiene; per un campo dà il solo
        /// nome, tipo e valore iniziale stanno nella dichiarazione.
        /// </summary>
        protected override SyntaxNode Whole(SyntaxNode memberDeclaration) => memberDeclaration switch
        {
            MethodBaseSyntax { Parent: MethodBlockBaseSyntax block } => block,
            PropertyStatementSyntax { Parent: PropertyBlockSyntax block } => block,
            ModifiedIdentifierSyntax { Parent: VariableDeclaratorSyntax { Parent: FieldDeclarationSyntax field } } => field,
            _ => memberDeclaration,
        };

        protected override bool IsCreation(SyntaxNode node, out string typeName, out IReadOnlyList<SyntaxNode> arguments)
        {
            typeName = null; arguments = Array.Empty<SyntaxNode>();
            if (node is not ObjectCreationExpressionSyntax creation) return false;
            typeName = creation.Type.DescendantNodesAndSelf().OfType<SimpleNameSyntax>().LastOrDefault(n => n.Parent is not TypeArgumentListSyntax)?.Identifier.ValueText;
            arguments = creation.ArgumentList?.Arguments.Select(a => (SyntaxNode)a.GetExpression()).ToList() ?? (IReadOnlyList<SyntaxNode>)Array.Empty<SyntaxNode>();
            return true;
        }

        /// <summary><c>cmd.CommandText = "…"</c> e, in un inizializzatore, <c>With {.CommandText = "…"}</c>.</summary>
        /// <summary><c>"SELECT … " &amp; filtro</c> (o <c>+</c>), <c>$"… {x}"</c>: la stringa intera, letta dal nodo più alto.</summary>
        protected override bool IsWholeString(SyntaxNode node, out IReadOnlyList<StringPart> parts)
        {
            parts = null;
            if (!(node is InterpolatedStringExpressionSyntax || IsStringLiteral(node) || IsConcatenation(node))) return false;
            var above = node.Parent;
            while (above is ParenthesizedExpressionSyntax) above = above.Parent;
            if (IsConcatenation(above)) return false;
            var list = new List<StringPart>();
            Flatten(node, list);
            if (!list.Any(p => p.Text != null)) return false;
            parts = list;
            return true;
        }

        private static bool IsStringLiteral(SyntaxNode node) => node is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression);
        private static bool IsConcatenation(SyntaxNode node) => node is BinaryExpressionSyntax b && (b.IsKind(SyntaxKind.ConcatenateExpression) || b.IsKind(SyntaxKind.AddExpression));

        private static void Flatten(SyntaxNode node, List<StringPart> parts)
        {
            switch (node)
            {
                case ParenthesizedExpressionSyntax p: Flatten(p.Expression, parts); break;
                case BinaryExpressionSyntax b when IsConcatenation(b): Flatten(b.Left, parts); Flatten(b.Right, parts); break;
                case LiteralExpressionSyntax lit when IsStringLiteral(lit): parts.Add(new StringPart { Text = lit.Token.ValueText }); break;
                case InterpolatedStringExpressionSyntax i:
                    foreach (var c in i.Contents)
                        parts.Add(c is InterpolatedStringTextSyntax t ? new StringPart { Text = t.TextToken.ValueText } : new StringPart { Hole = (SyntaxNode)(c as InterpolationSyntax)?.Expression ?? c });
                    break;
                default: parts.Add(new StringPart { Hole = node }); break;
            }
        }

        protected override string WithObject(SyntaxNode node)
            => node.Ancestors().OfType<WithBlockSyntax>().FirstOrDefault()?.WithStatement.Expression.ToString();

        protected override bool IsWithExpression(SyntaxNode node) => node.Parent is WithStatementSyntax w && w.Expression == node;

        protected override bool IsAssignment(SyntaxNode node, out AssignmentShape shape)
        {
            shape = default;
            if (node is NamedFieldInitializerSyntax init)
            {
                shape = new AssignmentShape { Name = init.Name.Identifier.ValueText, Receiver = null, Target = init.Name, Value = init.Expression };
                return true;
            }
            if (node is not AssignmentStatementSyntax a || !a.IsKind(SyntaxKind.SimpleAssignmentStatement)) return false;
            var (name, receiver) = a.Left switch
            {
                MemberAccessExpressionSyntax ma => (ma.Name.Identifier.ValueText, ma.Expression?.ToString()),
                IdentifierNameSyntax id => (id.Identifier.ValueText, (string)null),
                _ => (null, null),
            };
            if (name == null) return false;
            shape = new AssignmentShape { Name = name, Receiver = receiver, Target = a.Left, Value = a.Right };
            return true;
        }

        protected override bool IsRaise(SyntaxNode node, out SyntaxNode eventNode)
        {
            eventNode = (node as RaiseEventStatementSyntax)?.Name;
            return eventNode != null;
        }

        /// <summary><c>AddHandler x.Click, AddressOf Gestore</c>: è scritto che è un evento, anche quando la sua libreria manca.</summary>
        protected override bool IsSubscription(SyntaxNode node, out SyntaxNode eventNode, out SyntaxNode handlerNode, out bool certain)
        {
            eventNode = handlerNode = null; certain = true;
            if (node is not AddRemoveHandlerStatementSyntax a || !a.IsKind(SyntaxKind.AddHandlerStatement)) return false;
            eventNode = a.EventExpression is MemberAccessExpressionSyntax ma ? ma.Name : a.EventExpression;
            handlerNode = a.DelegateExpression is UnaryExpressionSyntax u && u.IsKind(SyntaxKind.AddressOfExpression) ? u.Operand : a.DelegateExpression;
            return true;
        }

        /// <summary><c>Handles btnCerca.Click, Me.Load</c> sulla riga del metodo.</summary>
        protected override IEnumerable<(SyntaxNode Event, SyntaxNode Container)> HandledEvents(SyntaxNode methodDeclaration)
        {
            var statement = methodDeclaration switch { MethodBlockSyntax block => block.SubOrFunctionStatement, MethodStatementSyntax s => s, _ => null };
            return statement?.HandlesClause?.Events.Select(e => ((SyntaxNode)e.EventMember, e.EventContainer is KeywordEventContainerSyntax ? null : (SyntaxNode)e.EventContainer))
                   ?? Enumerable.Empty<(SyntaxNode, SyntaxNode)>();
        }

        protected override string OwnEventReceiver(SyntaxNode eventNode)
            => eventNode.Parent is HandlesClauseItemSyntax { EventContainer: KeywordEventContainerSyntax k } ? k.Keyword.Text : null;

        protected override SyntaxNode SubscriptionContainer(SyntaxNode eventNode)
            => eventNode.Parent is MemberAccessExpressionSyntax { Expression: { } e } ma && ma.Name == eventNode ? e : null;

        /// <summary>In VB gli attributi stanno sulla riga che apre il blocco (<c>&lt;Export(GetType(IFoo))&gt; Public Class Foo</c>).</summary>
        protected override IEnumerable<AttributeShape> AttributesOf(SyntaxNode declaration)
        {
            var lists = declaration switch
            {
                TypeBlockSyntax t => t.BlockStatement.AttributeLists,
                EnumBlockSyntax e => e.EnumStatement.AttributeLists,
                MethodBlockBaseSyntax m => m.BlockStatement.AttributeLists,
                MethodBaseSyntax m => m.AttributeLists,
                PropertyBlockSyntax p => p.PropertyStatement.AttributeLists,
                FieldDeclarationSyntax f => f.AttributeLists,
                _ => default,
            };
            foreach (var a in lists.SelectMany(l => l.Attributes).Where(a => a.Target == null))
            {
                var args = new List<AttributeArgumentShape>();
                var position = 0;
                foreach (var arg in a.ArgumentList?.Arguments ?? default)
                {
                    var element = (arg as SimpleArgumentSyntax)?.NameColonEquals?.Name.Identifier.ValueText;
                    var at = element == null ? position++ : -1;
                    var expression = arg.GetExpression();
                    var elements = expression is CollectionInitializerSyntax c ? c.Initializers.ToList() : new List<ExpressionSyntax> { expression };
                    foreach (var e in elements)
                        args.Add(new AttributeArgumentShape
                        {
                            Element = element, Position = at,
                            Value = e is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression) ? lit.Token.ValueText : null,
                            TypeNode = (e as GetTypeExpressionSyntax)?.Type,
                        });
                }
                yield return new AttributeShape { NameNode = a.Name, Arguments = args };
            }
        }

        protected override bool IsInvocation(SyntaxNode node, SemanticModel model, out InvocationShape shape)
        {
            shape = default;
            if (node is not InvocationExpressionSyntax inv || inv.Expression == null) return false;
            // a(1) su una variabile, un campo o una proprietà è un accesso per indice, non una chiamata (i delegati sì)
            if (model.GetSymbolInfo(inv).Symbol is not IMethodSymbol)
            {
                var target = model.GetSymbolInfo(inv.Expression).Symbol;
                var type = target switch
                {
                    ILocalSymbol l => l.Type, IParameterSymbol p => p.Type, IFieldSymbol f => f.Type, IPropertySymbol pr => pr.Type,
                    _ => null,
                };
                if (target is IPropertySymbol { IsIndexer: true } || (type != null && type.TypeKind != TypeKind.Delegate)) return false;
            }
            var (callee, receiver) = inv.Expression switch
            {
                MemberAccessExpressionSyntax ma => (ma.Name.Identifier.ValueText, ma.Expression?.ToString()),
                SimpleNameSyntax n => (n.Identifier.ValueText, (string)null),
                _ => (null, null),
            };
            var args = callee == null ? null : (inv.ArgumentList?.Arguments ?? default).Select(a => a.GetExpression() switch
            {
                LiteralExpressionSyntax lit when lit.IsKind(SyntaxKind.StringLiteralExpression) => lit.Token.ValueText,
                NameOfExpressionSyntax n => n.Argument.ToString().Split('.').Last(),
                _ => null,
            }).ToList();
            var arguments = inv.ArgumentList?.Arguments ?? default;
            var name = inv.Expression switch { MemberAccessExpressionSyntax ma => ma.Name, SimpleNameSyntax n => n, _ => null };
            shape = new InvocationShape
            {
                Text = inv.Expression.ToString(), Callee = callee, Receiver = receiver, Arguments = args,
                ArgumentNodes = callee == null ? null : arguments.Select(a => (SyntaxNode)a.GetExpression()).ToList(),
                TypeNodes = arguments.Select(a => a.GetExpression() is GetTypeExpressionSyntax t ? (SyntaxNode)t.Type : null).ToList(),
                GenericNodes = name is GenericNameSyntax g ? g.TypeArgumentList.Arguments.ToList<SyntaxNode>() : new List<SyntaxNode>(),
                ReceiverNode = (inv.Expression as MemberAccessExpressionSyntax)?.Expression,
            };
            return true;
        }

        /// <summary>
        /// Il nome di un metodo che NON è l'espressione chiamata di una <see cref="InvocationExpressionSyntax"/>:
        /// <c>x = Conta</c> (parentesi omesse) o <c>AddressOf Gestisci</c>. Dentro <c>NameOf(…)</c> è solo un nome.
        /// </summary>
        protected override bool IsMethodNameOutsideInvocation(SyntaxNode node)
        {
            // un nome dentro un nome qualificato è un tipo o la clausola Implements, mai un'espressione
            if (node is not SimpleNameSyntax name || name.Parent is QualifiedNameSyntax) return false;
            SyntaxNode whole = name.Parent is MemberAccessExpressionSyntax ma && ma.Name == name ? ma : name;
            if (whole.Parent is InvocationExpressionSyntax inv && inv.Expression == whole) return false;
            // AddHandler/RemoveHandler x.Click, AddressOf H: il metodo che registra non esegue H (13/11/2026); la freccia
            // evento → gestore la fa la registrazione
            if (whole.Parent is UnaryExpressionSyntax u && u.IsKind(SyntaxKind.AddressOfExpression) && u.Parent is AddRemoveHandlerStatementSyntax) return false;
            return whole.Parent is not NameOfExpressionSyntax;
        }
    }
}
