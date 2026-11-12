using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'estrattore C# (D1): una <see cref="CSharpCompilation"/> sui soli file della cartella più le reference del runtime
    /// che MdExplorer si porta dietro — niente <c>.csproj</c>, MSBuild né SDK sulla macchina dell'utente (provato il
    /// 09/10/2026). Il <see cref="SemanticModel"/> risolve davvero: tipi, overload, dispatch per interfaccia.
    /// <list type="bullet">
    /// <item>Simboli: class, interface, record, struct, enum, method, constructor, property, field; parametri con tipo e
    /// direzione; firma per esteso da <see cref="SymbolDisplayFormat"/>.</item>
    /// <item>Archi <c>extracted</c>: contains, inherits, implements, references (tipi di campi, proprietà, parametri,
    /// ritorni), instantiates, calls (risolte). <c>inferred</c>: la chiamata su un'interfaccia verso ogni implementazione
    /// nel perimetro. <c>ambiguous</c>: candidati di Roslyn quando non sa scegliere, o il testo della chiamata come
    /// nodo external quando non trova niente (reference NuGet assenti).</item>
    /// <item>Un simbolo fuori perimetro (runtime, pacchetti) è un nodo <c>external</c> col suo nome qualificato.</item>
    /// </list>
    /// Gli id sono stabili rispetto alle righe: hash di linguaggio, kind e nome qualificato (con i tipi dei parametri per i
    /// metodi); spostare un metodo non cambia il suo id, rinominarlo o cambiarne i parametri sì.
    /// </summary>
    public sealed class RoslynCSharpExtractor : RoslynExtractorBase
    {
        public override string LanguageId => CodeLanguages.CSharp;
        /// <summary>
        /// 2 = i punti di chiamata con stringhe (D42 passo 2, 24/10/2026: <c>return View("Ordini")</c>, <c>RedirectToAction(nameof(Elenco))</c>).
        /// <para>3 = i fatti per le regole di registrazione (D43 per .NET, 11/11/2026): <c>typeof(X)</c>, gli argomenti generici
        /// scritti nella chiamata e le catene (<c>services.AddScoped&lt;IFoo, Foo&gt;()</c>, <c>RegisterType&lt;Foo&gt;().As&lt;IFoo&gt;()</c>),
        /// gli attributi (<c>[Export(typeof(IFoo))]</c>) e il tipo restituito dai metodi.</para>
        /// <para>4 = una compilazione per <c>.csproj</c>, con
        /// le librerie che il progetto riferisce e quelle di .NET Framework se punta lì; gli eventi (nodo, chi li solleva, i
        /// gestori agganciati con <c>+=</c>), i nomi assegnati a una proprietà o passati a un costruttore, le costanti stringa.</para>
        /// <para>5 = l'SQL scritto nelle stringhe: le tabelle lette e scritte e le procedure chiamate (<see cref="EmbeddedSql"/>).</para>
        /// <para>6 = i nodi di quelle tabelle e procedure dicono il linguaggio <c>sql</c>: si ricollegano al simbolo vero di un
        /// linguaggio database quando c'è (<see cref="CodeSqlLinker"/>, 12/11/2026).</para>
        /// </summary>
        public override string ExtractorVersion => "6";

        private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest, DocumentationMode.None);

        public RoslynCSharpExtractor(ILogger<RoslynCSharpExtractor> logger) : base(logger) { }

        protected override string ProjectExtension => ".csproj";

        protected override SyntaxTree Parse(string text, string path, DotNetProject project) => CSharpSyntaxTree.ParseText(text, ParseOptions, path: path);

        protected override Compilation Compile(string name, IReadOnlyList<SyntaxTree> trees, IReadOnlyList<MetadataReference> references, DotNetProject project)
            => CSharpCompilation.Create(name, trees, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        protected override IEnumerable<SyntaxNode> TypeDeclarations(SyntaxNode root) => root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>();

        protected override bool IsCreation(SyntaxNode node, out string typeName, out IReadOnlyList<SyntaxNode> arguments)
        {
            typeName = null; arguments = Array.Empty<SyntaxNode>();
            if (node is not BaseObjectCreationExpressionSyntax creation) return false;
            if (creation is ObjectCreationExpressionSyntax written)
                typeName = written.Type.DescendantNodesAndSelf().OfType<SimpleNameSyntax>().LastOrDefault(n => n.Parent is not TypeArgumentListSyntax)?.Identifier.ValueText;
            arguments = creation.ArgumentList?.Arguments.Select(a => (SyntaxNode)a.Expression).ToList() ?? (IReadOnlyList<SyntaxNode>)Array.Empty<SyntaxNode>();
            return true;
        }

        protected override bool IsAssignment(SyntaxNode node, out AssignmentShape shape)
        {
            shape = default;
            if (node is not AssignmentExpressionSyntax a || !a.IsKind(SyntaxKind.SimpleAssignmentExpression)) return false;
            var (name, receiver) = a.Left switch
            {
                MemberAccessExpressionSyntax ma => (ma.Name.Identifier.ValueText, ma.Expression.ToString()),
                IdentifierNameSyntax id => (id.Identifier.ValueText, (string)null),
                _ => (null, null),
            };
            if (name == null) return false;
            shape = new AssignmentShape { Name = name, Receiver = receiver, Target = a.Left, Value = a.Right };
            return true;
        }

        protected override bool IsWholeString(SyntaxNode node, out IReadOnlyList<StringPart> parts)
        {
            parts = null;
            if (!IsStringLike(node)) return false;
            var above = node.Parent;
            while (above is ParenthesizedExpressionSyntax) above = above.Parent;
            if (above is BinaryExpressionSyntax up && up.IsKind(SyntaxKind.AddExpression)) return false;   // un pezzo: si legge dall'alto
            var list = new List<StringPart>();
            Flatten(node, list);
            if (!list.Any(p => p.Text != null)) return false;   // a + b fra numeri
            parts = list;
            return true;
        }

        private static bool IsStringLike(SyntaxNode node) => node is InterpolatedStringExpressionSyntax
            || (node is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
            || (node is BinaryExpressionSyntax b && b.IsKind(SyntaxKind.AddExpression));

        private static void Flatten(SyntaxNode node, List<StringPart> parts)
        {
            switch (node)
            {
                case ParenthesizedExpressionSyntax p: Flatten(p.Expression, parts); break;
                case BinaryExpressionSyntax b when b.IsKind(SyntaxKind.AddExpression): Flatten(b.Left, parts); Flatten(b.Right, parts); break;
                case LiteralExpressionSyntax lit when lit.IsKind(SyntaxKind.StringLiteralExpression): parts.Add(new StringPart { Text = lit.Token.ValueText }); break;
                case InterpolatedStringExpressionSyntax i:
                    foreach (var c in i.Contents)
                        parts.Add(c is InterpolatedStringTextSyntax t ? new StringPart { Text = t.TextToken.ValueText } : new StringPart { Hole = (SyntaxNode)(c as InterpolationSyntax)?.Expression ?? c });
                    break;
                default: parts.Add(new StringPart { Hole = node }); break;
            }
        }

        /// <summary><c>Click(this, e)</c>, <c>Click.Invoke(…)</c>, <c>Click?.Invoke(…)</c>: il nodo che nomina l'evento, se lo è.</summary>
        protected override bool IsRaise(SyntaxNode node, out SyntaxNode eventNode)
        {
            eventNode = node switch
            {
                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Invoke" } ma } => ma.Expression,
                InvocationExpressionSyntax { Expression: MemberBindingExpressionSyntax { Name.Identifier.ValueText: "Invoke" }, Parent: ConditionalAccessExpressionSyntax c } => c.Expression,
                InvocationExpressionSyntax { Expression: IdentifierNameSyntax id } => id,
                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax ma } => ma,
                _ => null,
            };
            return eventNode != null;
        }

        /// <summary><c>x.Click += Gestore</c> o <c>+= new EventHandler(Gestore)</c>; senza un evento riconosciuto potrebbe essere una somma.</summary>
        protected override bool IsSubscription(SyntaxNode node, out SyntaxNode eventNode, out SyntaxNode handlerNode, out bool certain)
        {
            eventNode = handlerNode = null; certain = false;
            if (node is not AssignmentExpressionSyntax a || !a.IsKind(SyntaxKind.AddAssignmentExpression)) return false;
            eventNode = a.Left;
            handlerNode = a.Right is ObjectCreationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } args } ? args[0].Expression : a.Right;
            return true;
        }

        /// <summary>Gli attributi stanno sulla dichiarazione del membro; per un campo Roslyn dà il solo nome, la dichiarazione è più su.</summary>
        protected override IEnumerable<AttributeShape> AttributesOf(SyntaxNode declaration)
        {
            var member = declaration as MemberDeclarationSyntax ?? declaration.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault();
            if (member == null) yield break;
            foreach (var a in member.AttributeLists.Where(l => l.Target == null).SelectMany(l => l.Attributes))
            {
                var args = new List<AttributeArgumentShape>();
                var position = 0;
                foreach (var arg in a.ArgumentList?.Arguments ?? default)
                {
                    var element = arg.NameEquals?.Name.Identifier.ValueText ?? arg.NameColon?.Name.Identifier.ValueText;
                    var at = element == null ? position++ : -1;
                    foreach (var e in Elements(arg.Expression))
                        args.Add(new AttributeArgumentShape
                        {
                            Element = element, Position = at,
                            Value = e is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression) ? lit.Token.ValueText : null,
                            TypeNode = (e as TypeOfExpressionSyntax)?.Type,
                        });
                }
                yield return new AttributeShape { NameNode = a.Name, Arguments = args };
            }
        }

        /// <summary>Un vettore scritto nell'argomento vale un elemento alla volta: <c>new[] { typeof(A), typeof(B) }</c>, <c>[typeof(A)]</c>.</summary>
        private static IEnumerable<ExpressionSyntax> Elements(ExpressionSyntax e) => e switch
        {
            ImplicitArrayCreationExpressionSyntax i => i.Initializer.Expressions,
            ArrayCreationExpressionSyntax { Initializer: { } init } => init.Expressions,
            CollectionExpressionSyntax c => c.Elements.OfType<ExpressionElementSyntax>().Select(x => x.Expression),
            _ => new[] { e },
        };

        /// <summary>
        /// Il punto di chiamata con le stringhe letterali (D42): il nome del metodo, il ricevente scritto prima del punto e,
        /// per argomento, il testo di una stringa letterale o di <c>nameof(X)</c> (una costante anche lei); null per il resto.
        /// </summary>
        protected override bool IsInvocation(SyntaxNode node, SemanticModel model, out InvocationShape shape)
        {
            shape = default;
            if (node is not InvocationExpressionSyntax inv) return false;
            var (callee, receiver) = inv.Expression switch
            {
                MemberAccessExpressionSyntax ma => (ma.Name.Identifier.ValueText, ma.Expression.ToString()),
                SimpleNameSyntax n => (n.Identifier.ValueText, (string)null),
                MemberBindingExpressionSyntax mb => (mb.Name.Identifier.ValueText, (string)null),
                _ => (null, null),
            };
            var name = inv.Expression switch { MemberAccessExpressionSyntax ma => ma.Name, SimpleNameSyntax n => n, MemberBindingExpressionSyntax mb => mb.Name, _ => null };
            List<string> args = null;
            if (callee == "nameof") callee = null;
            if (callee != null)
                args = inv.ArgumentList.Arguments.Select(a => a.Expression switch
                {
                    LiteralExpressionSyntax lit when lit.IsKind(SyntaxKind.StringLiteralExpression) => lit.Token.ValueText,
                    InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } } n when n.ArgumentList.Arguments.Count == 1
                        => n.ArgumentList.Arguments[0].Expression.ToString().Split('.').Last(),
                    _ => null,
                }).ToList();
            shape = new InvocationShape
            {
                Text = inv.Expression.ToString(), Callee = callee, Receiver = receiver, Arguments = args,
                ArgumentNodes = callee == null ? null : inv.ArgumentList.Arguments.Select(a => (SyntaxNode)a.Expression).ToList(),
                TypeNodes = inv.ArgumentList.Arguments.Select(a => a.Expression is TypeOfExpressionSyntax t ? (SyntaxNode)t.Type : null).ToList(),
                GenericNodes = name is GenericNameSyntax g ? g.TypeArgumentList.Arguments.Where(a => a is not OmittedTypeArgumentSyntax).ToList<SyntaxNode>() : new List<SyntaxNode>(),
                ReceiverNode = (inv.Expression as MemberAccessExpressionSyntax)?.Expression,
            };
            return true;
        }

        // ------------------------------------------------------------------ ids

        public static string StableId(string languageId, string kind, string qualifiedName)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{languageId}|{kind}|{qualifiedName}"));
            return Convert.ToHexString(bytes, 0, 12).ToLowerInvariant(); // 24 hex
        }
    }
}
