using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Yaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// La gara del demo nel formato v3 (sprint workflow v3, F0): il workflow, le schede dei quattro agenti con la loro
    /// interfaccia, il documento delle responsabilità. È il caso di riferimento dei test del workflow e dello schedulatore.
    /// </summary>
    internal static class GaraV3
    {
        public const string Workflow = @"{
  ""mde_workflow"": 3,
  ""title"": ""Gara: dal bando alla decisione"",
  ""description"": ""Dal bando trovato sul portale alla decisione dell'account manager."",
  ""variables"": {
    ""codice"": ""il codice del bando, scelto con «Avvia il giro»"",
    ""capitolato"": ""il documento di gara scaricato per quel bando"",
    ""ricerca"": ""la ricerca dell'account manager""
  },
  ""steps"": [
    {
      ""id"": ""ricerca"",
      ""agent"": ""account-manager"",
      ""skill"": ""cerca-bandi"",
      ""title"": ""Cerca i bandi"",
      ""trigger"": { ""launch"": true },
      ""start"": ""manual""
    },
    {
      ""id"": ""tecnica"",
      ""agent"": ""responsabile-tecnico"",
      ""skill"": ""scheda-tecnica"",
      ""title"": ""Scheda tecnica"",
      ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" },
      ""start"": ""ask-owner"",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""{capitolato}"", ""ricerca"": ""{ricerca}"", ""criterio"": ""2 — competenza"" }
    },
    {
      ""id"": ""contratto"",
      ""agent"": ""responsabile-legale"",
      ""skill"": ""scheda-contrattuale"",
      ""title"": ""Scheda contrattuale"",
      ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" },
      ""start"": ""ask-owner"",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""{capitolato}"", ""ricerca"": ""{ricerca}"", ""criterio"": ""4 — rischio contrattuale"" }
    },
    {
      ""id"": ""delivery"",
      ""agent"": ""responsabile-delivery"",
      ""skill"": ""scheda-delivery"",
      ""title"": ""Scheda di delivery"",
      ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" },
      ""start"": ""ask-owner"",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""{capitolato}"", ""ricerca"": ""{ricerca}"", ""criterio"": ""3 — tempi di consegna"" }
    },
    {
      ""id"": ""sintesi"",
      ""agent"": ""account-manager"",
      ""skill"": ""sintesi-gara"",
      ""title"": ""Sintesi per decidere"",
      ""trigger"": { ""after"": [""tecnica"", ""contratto"", ""delivery""], ""wait"": ""all"" },
      ""start"": ""auto"",
      ""inputs"": {
        ""codice"": ""{codice}"",
        ""ricerca"": ""{ricerca}"",
        ""schede"": { ""from"": [""tecnica.scheda"", ""contratto.scheda"", ""delivery.scheda""] }
      }
    },
    {
      ""id"": ""decisione"",
      ""person"": ""Account manager"",
      ""title"": ""Decidi se partecipare"",
      ""trigger"": { ""after"": [""sintesi""] },
      ""task"": ""Leggi la sintesi sul bando {codice} e decidi se Pentagroup partecipa."",
      ""inputs"": { ""sintesi"": { ""from"": ""sintesi.sintesi"" } },
      ""outcomes"": [ { ""id"": ""partecipa"", ""label"": ""Partecipiamo"" }, { ""id"": ""rinuncia"", ""label"": ""Non partecipiamo"" } ]
    }
  ],
  ""loops"": [
    { ""id"": ""rifacimento"", ""steps"": [""tecnica"", ""contratto"", ""delivery""], ""until"": ""approved"", ""restart"": ""manual"" }
  ]
}";

        /// <summary>
        /// La gara senza la decisione della persona e con un brief per passo: il caso degli scenari dello schedulatore.
        /// </summary>
        public const string Plain = @"{
  ""mde_workflow"": 3, ""title"": ""Gara"",
  ""variables"": { ""codice"": ""il bando"", ""capitolato"": ""il documento di gara"", ""ricerca"": ""la ricerca"" },
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""account-manager"", ""skill"": ""cerca-bandi"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""tecnica"", ""agent"": ""responsabile-tecnico"", ""skill"": ""scheda-tecnica"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }, ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda tecnica sul bando {codice}."",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""il capitolato"", ""ricerca"": ""la ricerca"", ""criterio"": ""2"" } },
    { ""id"": ""contratto"", ""agent"": ""responsabile-legale"", ""skill"": ""scheda-contrattuale"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }, ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda contrattuale sul bando {codice}."",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""il capitolato"", ""ricerca"": ""la ricerca"", ""criterio"": ""4"" } },
    { ""id"": ""delivery"", ""agent"": ""responsabile-delivery"", ""skill"": ""scheda-delivery"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" }, ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda di delivery sul bando {codice}."",
      ""inputs"": { ""codice"": ""{codice}"", ""capitolato"": ""il capitolato"", ""ricerca"": ""la ricerca"", ""criterio"": ""3"" } },
    { ""id"": ""sintesi"", ""agent"": ""account-manager"", ""skill"": ""sintesi-gara"", ""trigger"": { ""after"": [""tecnica"", ""contratto"", ""delivery""] }, ""start"": ""auto"",
      ""brief"": ""Scrivi la sintesi sul bando {codice}."",
      ""inputs"": { ""codice"": ""{codice}"", ""ricerca"": ""la ricerca"", ""schede"": { ""from"": [""tecnica.scheda"", ""contratto.scheda"", ""delivery.scheda""] } } }
  ],
  ""loops"": [ { ""id"": ""rifacimento"", ""steps"": [""tecnica"", ""contratto"", ""delivery""], ""until"": ""approved"", ""restart"": ""manual"" } ]
}";

        public const string AccountManager = @"---
a2a:
  name: account-manager
  skills:
    - id: cerca-bandi
      description: Confronta il portale con il registro e il profilo
      reads:
        - { path: gara/bandi.md, note: i bandi del committente }
        - { path: gara/profilo.md, section: Criteri di valutazione di un bando, note: i cinque criteri }
      outputs:
        - { id: ricerca, path: gara/ricerche/ricerca-*.md, note: con una domanda per ciascuno }
        - { id: capitolato, path: gara/documenti/*-capitolato.md, action: downloads }
      replies: [avvia-giro]
    - id: sintesi-gara
      description: Scrive la sintesi per decidere se partecipare
      inputs:
        - { id: codice, description: il codice del bando }
        - { id: ricerca, description: la tua ricerca }
        - { id: schede, description: le schede approvate dei responsabili }
      outputs:
        - { id: sintesi, path: gara/schede/sintesi.md }
        - { id: registro, path: gara/registro.md, action: updates }
  replies:
    - id: avvia-giro
      label: Avvia il giro su {codice}
      description: Parte la valutazione del bando {codice} sul documento {capitolato}, con la ricerca {ricerca}.
      message: avvia {codice}
      values: [codice, capitolato, ricerca]
---
corpo";

        /// <summary>La scheda di un responsabile: tutti e tre uguali, con il loro capitolo e la loro scheda.</summary>
        public static string Responsible(string name, string skill, string section, string file) => $@"---
a2a:
  name: {name}
  skills:
    - id: {skill}
      description: Scrive la sua scheda di un bando
      inputs:
        - {{ id: codice, description: il codice del bando }}
        - {{ id: capitolato, description: il documento di gara }}
        - {{ id: ricerca, description: la ricerca dell'account manager }}
        - {{ id: criterio, description: il criterio che ti spetta }}
      reads:
        - {{ path: gara/profilo.md, section: {section} }}
      outputs:
        - {{ id: scheda, path: gara/schede/{file} }}
---
corpo";

        public static readonly (string Name, string Text)[] Cards =
        {
            ("account-manager", AccountManager),
            ("responsabile-tecnico", Responsible("responsabile-tecnico", "scheda-tecnica", "Tecnica", "tecnica.md")),
            ("responsabile-legale", Responsible("responsabile-legale", "scheda-contrattuale", "Contratto", "contrattuale.md")),
            ("responsabile-delivery", Responsible("responsabile-delivery", "scheda-delivery", "Delivery", "delivery.md")),
        };

        public const string Ownership = @"---
mde_type: ownership
---
| Ruolo | Responsabile | Git Email | Agenti |
|-------|--------------|-----------|--------|
| Account manager | Anna | anna@x.it | account-manager |
| Responsabile tecnico | Marco | marco@x.it | responsabile-tecnico |
| Responsabile legale | Anna | anna@x.it | responsabile-legale |
| Responsabile delivery | Anna | anna@x.it | responsabile-delivery |
";

        public const string Profile = "# Profilo\n\n## Criteri di valutazione di un bando\n\n## Tecnica\n\n## Contratto\n\n## Delivery\n";

        /// <summary>Le schede lette dai testi, senza file: ciò che un giro salva.</summary>
        public static WorkflowCardSet CardSet()
        {
            var parser = new YamlAgentCardParser();
            var set = new WorkflowCardSet();
            foreach (var (name, text) in Cards)
            {
                var card = parser.GetDescriptor(text);
                Assert.IsTrue(card.IsValid, card.RegistrationError);
                set.Agents[name] = new WorkflowCard { Path = $".github/agents/{name}.agent.md", Skills = card.Card.Skills.ToList(), Replies = card.Card.Replies?.ToList() ?? new() };
            }
            return set;
        }

        /// <summary>
        /// Aggiunge alla scheda di <paramref name="agent"/> (creandola) un lavoro senza ingressi, con al più un'uscita: per i
        /// workflow dei test che non sono la gara.
        /// </summary>
        public static WorkflowCardSet Job(this WorkflowCardSet set, string agent, string skill, string output = null)
        {
            if (!set.Agents.TryGetValue(agent, out var card))
                set.Agents[agent] = card = new WorkflowCard { Path = $".github/agents/{agent}.agent.md" };
            card.Skills.Add(new MdExplorer.Features.Yaml.Models.AgentCardSkill
            {
                Id = skill,
                Description = skill,
                Outputs = output == null ? null : new List<MdExplorer.Features.Yaml.Models.AgentCardOutput> { new() { Id = "out", Path = output } },
            });
            return set;
        }

        /// <summary>Il workflow letto e unito alle schede: senza problemi, o il test si ferma dicendo quali.</summary>
        public static WorkflowDescriptor Bound(string json = Workflow, WorkflowCardSet cards = null)
        {
            var r = WorkflowParser.Parse(json);
            Assert.IsTrue(r.IsValid, string.Join("\n", r.Issues));
            var issues = WorkflowBinder.Bind(r.Descriptor, cards ?? CardSet());
            Assert.AreEqual(0, issues.Count, string.Join("\n", issues));
            return r.Descriptor;
        }

        /// <summary>
        /// Un progetto su disco con la gara: workflow, schede in <c>.github/agents</c>, responsabilità, documenti letti e
        /// cartelle delle uscite. <paramref name="change"/> riceve la radice per guastare qualcosa prima della lettura.
        /// </summary>
        public static TempProject Project(Action<string> change = null)
        {
            var root = Path.Combine(Path.GetTempPath(), "gara-v3-" + Guid.NewGuid().ToString("N"));
            void Write(string relative, string text)
            {
                var full = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                File.WriteAllText(full, text);
            }
            Write("gara/gara.workflow.json", Workflow);
            foreach (var (name, text) in Cards) Write($".github/agents/{name}.agent.md", text);
            Write("responsabilita.md", Ownership);
            Write("gara/bandi.md", "# Bandi\n");
            Write("gara/profilo.md", Profile);
            Write("gara/registro.md", "# Registro\n");
            foreach (var folder in new[] { "gara/ricerche", "gara/documenti", "gara/schede" })
                Write(folder + "/README.md", "# " + folder + "\n");
            change?.Invoke(root);
            return new TempProject(root);
        }
    }

    internal sealed class TempProject : IDisposable
    {
        public TempProject(string root) => Root = root;
        public string Root { get; }

        public WorkflowCheckResult Check() => WorkflowFile.Check(Root, "gara/gara.workflow.json", "responsabilita.md");

        public void Dispose()
        {
            try { Directory.Delete(Root, true); } catch (IOException) { /* il sistema la ripulirà */ }
        }
    }
}
