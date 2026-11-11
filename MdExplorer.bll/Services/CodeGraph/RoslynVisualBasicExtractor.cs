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
    /// <item>Niente <c>.vbproj</c>: gli <c>Imports</c> di progetto sono quelli che Visual Studio mette in un progetto nuovo,
    /// <c>Option Infer On</c>. Il <c>RootNamespace</c> del progetto non si legge: i nomi qualificati partono dal
    /// <c>Namespace</c> scritto nei file.</item>
    /// <item>In VB le parentesi di una chiamata sono facoltative e <c>a(1)</c> è anche l'accesso a un vettore: una chiamata è
    /// ciò che Roslyn risolve in un metodo, comunque sia scritta; <c>AddressOf Metodo</c> è una chiamata (come i
    /// riferimenti a metodo di Java, F4d).</item>
    /// </list>
    /// </summary>
    public sealed class RoslynVisualBasicExtractor : RoslynExtractorBase
    {
        public override string LanguageId => CodeLanguages.VisualBasic;
        public override string ExtractorVersion => "1";

        private static readonly VisualBasicParseOptions ParseOptions = new(LanguageVersion.Latest, DocumentationMode.None);

        /// <summary>Gli <c>Imports</c> di progetto di un progetto VB nuovo: senza, <c>List(Of T)</c> o <c>MsgBox</c> non si risolvono.</summary>
        private static readonly IReadOnlyList<GlobalImport> ProjectImports = GlobalImport.Parse(
            "Microsoft.VisualBasic", "System", "System.Collections", "System.Collections.Generic", "System.Data",
            "System.Diagnostics", "System.Linq", "System.Xml.Linq", "System.Threading.Tasks").ToList();

        public RoslynVisualBasicExtractor(ILogger<RoslynVisualBasicExtractor> logger) : base(logger) { }

        protected override SyntaxTree Parse(string text, string path) => VisualBasicSyntaxTree.ParseText(text, ParseOptions, path: path);

        protected override Compilation Compile(IReadOnlyList<SyntaxTree> trees, IReadOnlyList<MetadataReference> references)
            => VisualBasicCompilation.Create("mde-codegraph", trees, references,
                new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, globalImports: ProjectImports, optionInfer: true));

        protected override IEnumerable<SyntaxNode> TypeDeclarations(SyntaxNode root)
            => root.DescendantNodes().Where(n => n is TypeBlockSyntax or EnumBlockSyntax);

        protected override INamedTypeSymbol TypeSymbol(SemanticModel model, SyntaxNode typeDeclaration) => typeDeclaration switch
        {
            TypeBlockSyntax t => model.GetDeclaredSymbol(t.BlockStatement),
            EnumBlockSyntax e => model.GetDeclaredSymbol(e.EnumStatement),
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

        protected override bool IsCreation(SyntaxNode node) => node is ObjectCreationExpressionSyntax;

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
            shape = new InvocationShape { Text = inv.Expression.ToString(), Callee = callee, Receiver = receiver, Arguments = args };
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
            return whole.Parent is not NameOfExpressionSyntax;
        }
    }
}
