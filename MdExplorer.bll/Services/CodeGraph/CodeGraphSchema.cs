using System.Collections.Generic;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>I fatti per le regole presenti nel grafo di un progetto, dai più frequenti (D43, <c>code_schema</c>).</summary>
    public sealed class CodeRuleFacts
    {
        /// <summary>Le chiamate con argomenti X.class: nome chiamato → punti.</summary>
        public List<(string Callee, int Count)> TypeCallees { get; } = new();
        /// <summary>Le chiamate con argomenti stringa a forma di nome: nome chiamato → punti.</summary>
        public List<(string Callee, int Count)> StringCallees { get; } = new();
        /// <summary>Le annotazioni: nome com'è scritto, nome risolto (se gli import lo dicono) → simboli.</summary>
        public List<(string Name, string TypeName, int Count)> Annotations { get; } = new();
        /// <summary>I metodi col tipo restituito risolto (per <c>returnType</c>).</summary>
        public int ResolvedReturnTypes { get; set; }
        /// <summary>Le chiamate legate in catena (<c>code:receiverSite</c>).</summary>
        public int Chains { get; set; }
    }

    /// <summary>
    /// Il modello del grafo del codice spiegato a chi lo legge (D43, 30/10/2026, utente: «hai messo la struttura ontologica a
    /// disposizione di MarkAgent perché capisca come usare questo grafo?»). Mark non interroga il database: queste sono le
    /// parole con cui <c>code_schema</c> gli dice che cosa c'è, insieme ai numeri del progetto. In inglese, come le skill.
    /// </summary>
    public static class CodeGraphSchema
    {
        public static readonly IReadOnlyDictionary<string, string> SymbolKinds = new Dictionary<string, string>
        {
            ["namespace"] = "a namespace or package declaration",
            ["module"] = "a file seen as a unit (a script, a page, a TS/JS module); its parameters are the script's param()",
            ["class"] = "a class", ["interface"] = "an interface (also a Java annotation type)", ["record"] = "a record", ["struct"] = "a struct", ["enum"] = "an enum",
            ["type"] = "a type alias or similar declaration",
            ["method"] = "a method", ["constructor"] = "a constructor", ["property"] = "a property", ["field"] = "a field or constant", ["function"] = "a free function",
            ["component"] = "a component (Angular @Component, Spring @Component/@Controller)", ["service"] = "a service (Angular @Injectable, Spring @Service/@Repository)",
            ["directive"] = "an Angular directive", ["pipe"] = "an Angular pipe", ["ngmodule"] = "an Angular NgModule",
            ["fragment"] = "a piece of a long block of a declarative language, from one label to the next",
            ["control"] = "an element of a page with an event (onclick…, (click)) or a javascript: link",
            ["external"] = "something used but declared outside the scanned perimeter (a library, another program not bridged): only its name is known",
        };

        public static readonly IReadOnlyDictionary<string, string> Relations = new Dictionary<string, string>
        {
            ["contains"] = "a type contains its members, a module its symbols",
            ["imports"] = "a file/type imports another",
            ["inherits"] = "extends a base class (or an interface extends another)",
            ["implements"] = "a class implements an interface: who COULD run behind it",
            ["references"] = "uses a type in a signature, field or variable",
            ["calls"] = "calls a method/function",
            ["instantiates"] = "creates an instance (new X, <app-x>)",
            ["jumps"] = "the code goes from one fragment to another (GOTO, falls through): not a dependency",
            ["dispatches"] = "a STRING names the method that runs (enrichment rule)",
            ["includes"] = "a page includes another page",
            ["renders"] = "a component shows its template",
            ["resolvesTo"] = "an abstraction resolves into the implementation REGISTERED to run (META-INF/services, or a registration rule: DI binding, @ImplementedBy, @Provides)",
        };

        public static readonly IReadOnlyDictionary<string, string> Confidences = new Dictionary<string, string>
        {
            ["extracted"] = "the parser is sure",
            ["inferred"] = "deduced (resolved by name or by the only candidate)",
            ["ambiguous"] = "more than one candidate, nobody chose",
            ["llm-resolved"] = "one of several candidates, chosen by the LLM that described the caller: a deduction",
            ["rule"] = "added by an enrichment rule the user approved",
        };

        /// <summary>I fatti che le regole leggono, e da quale regola.</summary>
        public static readonly IReadOnlyDictionary<string, string> RuleFacts = new Dictionary<string, string>
        {
            ["stringArguments"] = "calls with string arguments shaped like names (setTarget(\"read\")): dispatch rules (call.callee, call.argument)",
            ["typeArguments"] = "calls with X.class arguments, type resolved through the imports: registration rules (anchor.call, from argument)",
            ["chains"] = "a call reaches the recorded call further down its receiver chain (to → bind): registration rules (from chain)",
            ["annotations"] = "annotations with their string or X.class arguments: registration rules (anchor.annotation, from annotated / annotationArgument)",
            ["returnTypes"] = "methods whose return type resolves to a type of the project or a library: registration rules (from returnType)",
        };
    }
}
