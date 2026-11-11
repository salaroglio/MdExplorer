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
        /// <summary>2 = i punti di chiamata con stringhe (D42 passo 2, 24/10/2026: <c>return View("Ordini")</c>, <c>RedirectToAction(nameof(Elenco))</c>).</summary>
        public override string ExtractorVersion => "2";

        private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest, DocumentationMode.None);

        public RoslynCSharpExtractor(ILogger<RoslynCSharpExtractor> logger) : base(logger) { }

        protected override SyntaxTree Parse(string text, string path) => CSharpSyntaxTree.ParseText(text, ParseOptions, path: path);

        protected override Compilation Compile(IReadOnlyList<SyntaxTree> trees, IReadOnlyList<MetadataReference> references)
            => CSharpCompilation.Create("mde-codegraph", trees, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        protected override IEnumerable<SyntaxNode> TypeDeclarations(SyntaxNode root) => root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>();

        protected override bool IsCreation(SyntaxNode node) => node is BaseObjectCreationExpressionSyntax;

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
            shape = new InvocationShape { Text = inv.Expression.ToString(), Callee = callee, Receiver = receiver, Arguments = args };
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
