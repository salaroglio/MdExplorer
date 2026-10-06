using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Agents.Workflow
{
    /// <summary>
    /// Legge un <c>*.workflow.json</c> (standard v2) e ne verifica la struttura, senza guardare il progetto. Tolleranza
    /// zero: una chiave sconosciuta è un errore con il suo percorso, mai ignorata, perché un errore di battitura in una
    /// regola («strat» invece di «start») cambierebbe il comportamento in silenzio. Non si ferma al primo problema: li
    /// raccoglie tutti, così chi corregge (una persona o un LLM) lo fa in un giro solo.
    /// </summary>
    public static class WorkflowParser
    {
        public const int SupportedVersion = 2;

        private static readonly Regex KebabId = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Regex VariableName = new(@"^[a-z][a-z0-9_]*$", RegexOptions.Compiled);
        /// <summary>Un segnaposto in un brief: <c>{codice}</c>.</summary>
        public static readonly Regex Placeholder = new(@"\{([^{}\s]+)\}", RegexOptions.Compiled);

        private static readonly string[] TopKeys = { "mde_workflow", "title", "description", "variables", "steps", "loops" };
        private static readonly string[] StepKeys = { "id", "agent", "title", "trigger", "start", "brief", "produces" };
        private static readonly string[] TriggerKeys = { "launch", "reply", "to", "after", "wait" };
        private static readonly string[] LoopKeys = { "id", "steps", "until", "max", "restart", "times" };

        public static WorkflowCheckResult Parse(string json)
        {
            var result = new WorkflowCheckResult();
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(json ?? string.Empty, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Disallow });
            }
            catch (JsonException ex)
            {
                Error(result, "", $"non è JSON valido: {ex.Message}");
                return result;
            }

            using (doc)
            {
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    Error(result, "", "il workflow è un oggetto JSON ({ … }).");
                    return result;
                }

                var wf = new WorkflowDescriptor();
                result.Descriptor = wf;

                if (!root.TryGetProperty("mde_workflow", out var version))
                    Error(result, "mde_workflow", "manca la versione dello standard.", $"aggiungi \"mde_workflow\": {SupportedVersion}");
                else if (version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out var v))
                    Error(result, "mde_workflow", "la versione è un numero intero.", $"\"mde_workflow\": {SupportedVersion}");
                else if (v == 1)
                {
                    // La v1 descriveva chi manda l'incarico (assignment/approval); con lo schedulatore il passo parte quando i
                    // precedenti sono finiti. La conversione è meccanica: si dice qual è, invece di indovinarla.
                    Error(result, "mde_workflow", "versione 1 non più supportata: con lo schedulatore i passi partono quando i precedenti sono finiti.",
                        "\"mde_workflow\": 2; ogni \"assignment\": \"x\" e \"approval\": [\"x\", …] diventa \"after\": [\"x\", …]; ogni passo che non è un lancio ha un \"brief\"; i cicli: \"until\": \"approved\" con \"max\" e \"restart\", oppure \"times\"");
                    return result;
                }
                else if (v != SupportedVersion)
                    Error(result, "mde_workflow", $"versione {v} non supportata: questo MdExplorer conosce la {SupportedVersion}.");
                else
                    wf.Version = v;

                UnknownKeys(result, root, "", TopKeys);
                wf.Title = RequiredString(result, root, "title", "title");
                wf.Description = OptionalString(result, root, "description", "description");

                if (root.TryGetProperty("variables", out var variables))
                {
                    if (variables.ValueKind != JsonValueKind.Object)
                        Error(result, "variables", "le variabili sono un oggetto: nome → che cos'è (per esempio { \"codice\": \"il codice del bando\" }).");
                    else
                        foreach (var p in variables.EnumerateObject())
                        {
                            if (!VariableName.IsMatch(p.Name))
                                Error(result, "variables." + p.Name, $"'{p.Name}' non è un nome di variabile (minuscole, cifre e '_', comincia con una lettera).");
                            else if (p.Value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(p.Value.GetString()))
                                Error(result, "variables." + p.Name, "una variabile ha la sua descrizione, una stringa non vuota.");
                            else
                                wf.Variables[p.Name] = p.Value.GetString().Trim();
                        }
                }

                if (!root.TryGetProperty("steps", out var steps))
                    Error(result, "steps", "mancano i passi.", "aggiungi \"steps\": [ … ] con almeno un passo");
                else if (steps.ValueKind != JsonValueKind.Array)
                    Error(result, "steps", "i passi sono una lista ([ … ]).");
                else
                {
                    var i = 0;
                    foreach (var s in steps.EnumerateArray())
                    {
                        var step = ReadStep(result, s, $"steps[{i}]");
                        if (step != null) wf.Steps.Add(step);
                        i++;
                    }
                    if (i == 0) Error(result, "steps", "serve almeno un passo.");
                }

                if (root.TryGetProperty("loops", out var loops))
                {
                    if (loops.ValueKind != JsonValueKind.Array)
                        Error(result, "loops", "i cicli sono una lista ([ … ]).");
                    else
                    {
                        var i = 0;
                        foreach (var l in loops.EnumerateArray())
                        {
                            var loop = ReadLoop(result, l, $"loops[{i}]");
                            if (loop != null) wf.Loops.Add(loop);
                            i++;
                        }
                    }
                }

                CheckVariables(result, wf);
                CheckGraph(result, wf);
            }
            return result;
        }

        private static WorkflowStep ReadStep(WorkflowCheckResult r, JsonElement e, string path)
        {
            if (e.ValueKind != JsonValueKind.Object)
            {
                Error(r, path, "un passo è un oggetto ({ \"id\": …, \"agent\": …, \"trigger\": …, \"start\": … }).");
                return null;
            }
            UnknownKeys(r, e, path, StepKeys);
            var step = new WorkflowStep
            {
                Id = RequiredId(r, e, "id", path + ".id"),
                Agent = RequiredId(r, e, "agent", path + ".agent"),
                Title = OptionalString(r, e, "title", path + ".title"),
                Trigger = ReadTrigger(r, e, path + ".trigger"),
                Brief = OptionalString(r, e, "brief", path + ".brief"),
            };

            var start = RequiredString(r, e, "start", path + ".start");
            switch (start)
            {
                case null: break;
                case "manual": step.Start = WorkflowStart.Manual; break;
                case "ask-owner": step.Start = WorkflowStart.AskOwner; break;
                case "auto": step.Start = WorkflowStart.Auto; break;
                default:
                    Error(r, path + ".start", $"'{start}' non è un modo di avvio.", "usa manual, ask-owner oppure auto");
                    start = null;
                    break;
            }
            if (start != null && step.Trigger != null)
                CheckStartFitsTrigger(r, step, path);

            // Il brief è ciò che l'agente riceve da MDE: senza, un passo che non lancia la persona non saprebbe che fare.
            if (step.Trigger != null && step.Trigger.Kind != WorkflowTriggerKind.Launch && step.Brief == null && !e.TryGetProperty("brief", out _))
                Error(r, path + ".brief", "manca il testo dell'incarico: è ciò che MDE manda all'agente quando il passo parte.",
                    "per esempio \"brief\": \"Scrivi la scheda tecnica sul bando {codice}.\"");

            if (e.TryGetProperty("produces", out var produces))
            {
                if (produces.ValueKind != JsonValueKind.Array)
                    Error(r, path + ".produces", "gli artefatti sono una lista di percorsi ([ \"cartella/file.md\" ]).");
                else
                {
                    var i = 0;
                    foreach (var p in produces.EnumerateArray())
                    {
                        var pp = $"{path}.produces[{i++}]";
                        if (p.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(p.GetString()))
                        {
                            Error(r, pp, "un artefatto è un percorso, una stringa non vuota.");
                            continue;
                        }
                        var value = p.GetString().Trim();
                        if (value.Contains('\\'))
                            Error(r, pp, $"'{value}': nei percorsi si usa la barra '/'.", "scrivi " + value.Replace('\\', '/'));
                        else if (value.StartsWith("/") || value.Split('/').Contains(".."))
                            Error(r, pp, $"'{value}' esce dal progetto o parte dalla radice del disco.", "scrivi il percorso dalla radice del progetto, senza '/' iniziale e senza '..'");
                        else if (value.Split('/').Take(value.Split('/').Length - 1).Any(seg => seg.Contains('*')))
                            Error(r, pp, $"'{value}': '*' è ammesso solo nel nome del file, non nelle cartelle.");
                        else
                            step.Produces.Add(value);
                    }
                }
            }
            return step;
        }

        private static WorkflowTrigger ReadTrigger(WorkflowCheckResult r, JsonElement step, string path)
        {
            if (!step.TryGetProperty("trigger", out var e))
            {
                Error(r, path, "manca cosa fa partire il passo.", "per esempio \"trigger\": { \"launch\": true }");
                return null;
            }
            if (e.ValueKind != JsonValueKind.Object)
            {
                Error(r, path, "il trigger è un oggetto, per esempio { \"after\": [\"ricerca\"] }.");
                return null;
            }
            // Le forme della v1: si dice con che cosa si sostituiscono, invece di un generico «chiave sconosciuta».
            foreach (var old in new[] { "assignment", "approval" })
                if (e.TryGetProperty(old, out _))
                    Error(r, path + "." + old, $"«{old}» era della versione 1: nella 2 un passo parte quando i precedenti sono finiti.",
                        "usa \"after\": [\"<id del passo>\", …]");
            UnknownKeys(r, e, path, TriggerKeys.Concat(new[] { "assignment", "approval" }).ToArray());

            var forms = new[] { "launch", "reply", "after" }.Where(k => e.TryGetProperty(k, out _)).ToList();
            if (forms.Count != 1)
            {
                if (forms.Count > 1 || !e.EnumerateObject().Any(p => p.Name == "assignment" || p.Name == "approval"))
                    Error(r, path, forms.Count == 0
                            ? "il trigger non dice cosa fa partire il passo."
                            : $"il trigger ha più forme insieme ({string.Join(", ", forms)}): un passo parte in un modo solo.",
                        "usa una sola tra launch, reply, after");
                return null;
            }

            var t = new WorkflowTrigger();
            switch (forms[0])
            {
                case "launch":
                    t.Kind = WorkflowTriggerKind.Launch;
                    if (e.GetProperty("launch").ValueKind != JsonValueKind.True)
                        Error(r, path + ".launch", "si scrive \"launch\": true.");
                    break;
                case "reply":
                    t.Kind = WorkflowTriggerKind.Reply;
                    t.ReplyId = RequiredId(r, e, "reply", path + ".reply");
                    t.FromStep = RequiredId(r, e, "to", path + ".to");
                    break;
                case "after":
                    t.Kind = WorkflowTriggerKind.After;
                    var after = e.GetProperty("after");
                    if (after.ValueKind != JsonValueKind.Array)
                        Error(r, path + ".after", "\"after\" è la lista dei passi che devono essere finiti, per esempio [\"ricerca\"].");
                    else
                    {
                        var i = 0;
                        foreach (var s in after.EnumerateArray())
                        {
                            var sp = $"{path}.after[{i++}]";
                            if (s.ValueKind != JsonValueKind.String || !KebabId.IsMatch(s.GetString() ?? ""))
                                Error(r, sp, "è l'id di un passo (kebab-case).");
                            else if (t.After.Contains(s.GetString()))
                                Error(r, sp, $"'{s.GetString()}' è già nella lista.");
                            else
                                t.After.Add(s.GetString());
                        }
                        if (i == 0) Error(r, path + ".after", "la lista è vuota: dopo quali passi parte questo?");
                    }
                    var wait = OptionalString(r, e, "wait", path + ".wait");
                    if (wait == "any") t.Wait = WorkflowWait.Any;
                    else if (wait != null && wait != "all")
                        Error(r, path + ".wait", $"'{wait}' non è un modo di attendere.", "usa all (tutti) oppure any (il primo)");
                    break;
            }
            if (t.Kind != WorkflowTriggerKind.Reply && e.TryGetProperty("to", out _))
                Error(r, path + ".to", "\"to\" serve solo con \"reply\": dice sotto il messaggio di quale passo sta il pulsante.");
            if (t.Kind != WorkflowTriggerKind.After && e.TryGetProperty("wait", out _))
                Error(r, path + ".wait", "\"wait\" serve solo con \"after\".");
            return t;
        }

        /// <summary>Chi avvia deve avere senso per ciò che fa partire il passo.</summary>
        private static void CheckStartFitsTrigger(WorkflowCheckResult r, WorkflowStep step, string path)
        {
            var (allowed, why) = step.Trigger.Kind switch
            {
                WorkflowTriggerKind.Launch => (new[] { WorkflowStart.Manual }, "un lancio lo fa la persona: start è manual"),
                // Chi preme il pulsante sceglie per il proprio agente; il passo può essere di un agente di un'altra persona.
                WorkflowTriggerKind.Reply => (new[] { WorkflowStart.AskOwner, WorkflowStart.Auto }, "lo avvia il responsabile (ask-owner) o parte da solo (auto)"),
                _ => (new[] { WorkflowStart.AskOwner, WorkflowStart.Auto }, "lo avvia il responsabile (ask-owner) o parte da solo (auto)"),
            };
            if (!allowed.Contains(step.Start))
                Error(r, path + ".start", $"'{StartName(step.Start)}' non va con un trigger {KindName(step.Trigger.Kind)}: {why}.");
        }

        private static WorkflowLoop ReadLoop(WorkflowCheckResult r, JsonElement e, string path)
        {
            if (e.ValueKind != JsonValueKind.Object)
            {
                Error(r, path, "un ciclo è un oggetto: { \"id\": …, \"steps\": […], \"until\": \"approved\", … } oppure { …, \"times\": 3 }.");
                return null;
            }
            foreach (var old in new[] { "on", "then" })
                if (e.TryGetProperty(old, out _))
                    Error(r, path + "." + old, $"«{old}» era della versione 1.", "un ciclo «fino a che» si scrive \"until\": \"approved\", con \"max\" (facoltativo) e \"restart\"");
            UnknownKeys(r, e, path, LoopKeys.Concat(new[] { "on", "then" }).ToArray());
            var loop = new WorkflowLoop { Id = RequiredId(r, e, "id", path + ".id") };

            if (!e.TryGetProperty("steps", out var steps) || steps.ValueKind != JsonValueKind.Array)
                Error(r, path + ".steps", "manca la lista dei passi a cui si applica il ciclo.");
            else
            {
                var i = 0;
                foreach (var s in steps.EnumerateArray())
                {
                    var sp = $"{path}.steps[{i++}]";
                    if (s.ValueKind != JsonValueKind.String || !KebabId.IsMatch(s.GetString() ?? ""))
                        Error(r, sp, "è l'id di un passo (kebab-case).");
                    else
                        loop.Steps.Add(s.GetString());
                }
                if (i == 0) Error(r, path + ".steps", "la lista è vuota.");
            }

            var hasUntil = e.TryGetProperty("until", out var until);
            var hasTimes = e.TryGetProperty("times", out var times);
            if (hasUntil == hasTimes)
            {
                Error(r, path, hasUntil
                        ? "un ciclo è «fino a che» (until) oppure «for» (times), non tutti e due."
                        : "il ciclo non dice che tipo è.",
                    "\"until\": \"approved\" (rifai finché non è approvato) oppure \"times\": 3 (fai 3 giri)");
                return loop;
            }

            if (hasUntil)
            {
                loop.Kind = WorkflowLoopKind.UntilApproved;
                if (until.ValueKind != JsonValueKind.String || until.GetString() != "approved")
                    Error(r, path + ".until", "un ciclo «fino a che» si ripete finché il lavoro non è approvato.", "\"until\": \"approved\"");
                if (e.TryGetProperty("max", out var max))
                {
                    if (max.ValueKind != JsonValueKind.Number || !max.TryGetInt32(out var m) || m < 1)
                        Error(r, path + ".max", "è un numero intero da 1 in su; se non c'è un limite, togli \"max\".");
                    else
                        loop.Max = m;
                }
                var restart = RequiredString(r, e, "restart", path + ".restart");
                if (restart == "manual") loop.Restart = WorkflowStart.Manual;
                else if (restart == "auto") loop.Restart = WorkflowStart.Auto;
                else if (restart != null)
                    Error(r, path + ".restart", $"'{restart}' non è un modo di ripartire.", "manual (la persona preme «Fai ripartire») oppure auto (riparte da solo)");
            }
            else
            {
                loop.Kind = WorkflowLoopKind.Times;
                if (times.ValueKind != JsonValueKind.Number || !times.TryGetInt32(out var n) || n < 2)
                    Error(r, path + ".times", "è un numero intero da 2 in su: un giro solo non è un ciclo.");
                else
                    loop.Times = n;
                foreach (var k in new[] { "max", "restart" })
                    if (e.TryGetProperty(k, out _))
                        Error(r, path + "." + k, $"«{k}» serve ai cicli «fino a che» (until): un «for» fa esattamente \"times\" giri.");
            }
            return loop;
        }

        /// <summary>Ogni {variabile} di un brief è dichiarata: altrimenti l'agente riceverebbe le parentesi così come sono.</summary>
        private static void CheckVariables(WorkflowCheckResult r, WorkflowDescriptor wf)
        {
            for (var i = 0; i < wf.Steps.Count; i++)
            {
                var brief = wf.Steps[i].Brief;
                if (brief == null) continue;
                foreach (Match m in Placeholder.Matches(brief))
                    if (!wf.Variables.ContainsKey(m.Groups[1].Value))
                        Error(r, $"steps[{i}].brief", $"'{{{m.Groups[1].Value}}}' non è una variabile dichiarata.",
                            wf.Variables.Count == 0
                                ? $"dichiarala in \"variables\": {{ \"{m.Groups[1].Value}\": \"che cos'è\" }}"
                                : "variabili dichiarate: " + string.Join(", ", wf.Variables.Keys));
            }
        }

        /// <summary>Riferimenti, unicità, raggiungibilità, assenza di cicli nascosti: ciò che si vede solo guardando tutti i passi.</summary>
        private static void CheckGraph(WorkflowCheckResult r, WorkflowDescriptor wf)
        {
            var index = new Dictionary<string, int>();
            for (var i = 0; i < wf.Steps.Count; i++)
            {
                var id = wf.Steps[i].Id;
                if (id == null) continue;
                if (index.ContainsKey(id))
                    Error(r, $"steps[{i}].id", $"'{id}' è già l'id di steps[{index[id]}]: gli id sono unici.");
                else
                    index[id] = i;
            }

            for (var i = 0; i < wf.Steps.Count; i++)
            {
                var t = wf.Steps[i].Trigger;
                if (t == null) continue;
                var where = t.Kind == WorkflowTriggerKind.Reply ? ".to" : ".after";
                foreach (var src in t.Sources.Where(s => s != null))
                {
                    if (!index.ContainsKey(src))
                        Error(r, $"steps[{i}].trigger{where}", $"'{src}' non è l'id di nessun passo.", Known(index.Keys));
                    else if (src == wf.Steps[i].Id)
                        Error(r, $"steps[{i}].trigger{where}", "un passo non può partire da sé stesso: per ripeterlo si dichiara un ciclo in \"loops\".");
                }
            }

            var loopIds = new HashSet<string>();
            var looped = new Dictionary<string, string>();
            for (var i = 0; i < wf.Loops.Count; i++)
            {
                var loop = wf.Loops[i];
                if (loop.Id != null && !loopIds.Add(loop.Id))
                    Error(r, $"loops[{i}].id", $"'{loop.Id}' è già l'id di un altro ciclo.");
                for (var j = 0; j < loop.Steps.Count; j++)
                {
                    var stepId = loop.Steps[j];
                    if (!index.ContainsKey(stepId))
                    {
                        Error(r, $"loops[{i}].steps[{j}]", $"'{stepId}' non è l'id di nessun passo.", Known(index.Keys));
                        continue;
                    }
                    // Un ciclo per tipo: «for» e «fino a che» insieme hanno un senso (ogni giro si rifà finché non è approvato),
                    // due dello stesso tipo no (quale dei due conta?).
                    var key = stepId + "|" + loop.Kind;
                    if (looped.TryGetValue(key, out var other))
                        Error(r, $"loops[{i}].steps[{j}]", $"'{stepId}' è già nel ciclo '{other}', dello stesso tipo: un passo sta al più in un ciclo «fino a che» e in un «for».");
                    else
                        looped[key] = loop.Id;
                    if (loop.Kind == WorkflowLoopKind.UntilApproved && wf.Step(stepId).Produces.Count == 0)
                        Warning(r, $"loops[{i}].steps[{j}]", $"il passo '{stepId}' non dichiara artefatti: «fino a che è approvato» ripete un artefatto rifiutato.",
                            "aggiungi \"produces\" al passo, o toglilo dal ciclo");
                }
            }

            if (!wf.Steps.Any(s => s.Trigger?.Kind == WorkflowTriggerKind.Launch))
            {
                Error(r, "steps", "nessun passo parte da un lancio della persona: il workflow non comincerebbe mai.",
                    "il primo passo ha \"trigger\": { \"launch\": true }, \"start\": \"manual\"");
                return;
            }

            // Grafo: arco da ogni passo sorgente al passo che fa partire.
            var next = wf.Steps.Where(s => s.Id != null && index.ContainsKey(s.Id)).ToDictionary(s => s.Id, _ => new List<string>());
            foreach (var s in wf.Steps.Where(s => s.Id != null && s.Trigger != null))
                foreach (var src in s.Trigger.Sources.Where(x => x != null && next.ContainsKey(x) && x != s.Id))
                    next[src].Add(s.Id);

            // Nessun ritorno tra i passi: le ripetizioni si dichiarano in «loops», dove si vedono e si contano.
            var state = new Dictionary<string, int>();
            foreach (var id in next.Keys)
            {
                var cycle = FindCycle(id, next, state, new Stack<string>());
                if (cycle != null)
                {
                    Error(r, $"steps[{index[cycle[0]]}]", $"i passi si fanno partire a vicenda: {string.Join(" → ", cycle)}.",
                        "i passi formano una catena senza ritorni; per ripetere un passo dichiara un ciclo in \"loops\" (until o times)");
                    break;
                }
            }

            var reached = new HashSet<string>();
            var queue = new Queue<string>(wf.Steps.Where(s => s.Trigger?.Kind == WorkflowTriggerKind.Launch && s.Id != null).Select(s => s.Id));
            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (!reached.Add(id)) continue;
                if (next.TryGetValue(id, out var outs)) foreach (var o in outs) queue.Enqueue(o);
            }
            foreach (var s in wf.Steps.Where(s => s.Id != null && index.ContainsKey(s.Id) && !reached.Contains(s.Id)))
                Error(r, $"steps[{index[s.Id]}]", $"il passo '{s.Id}' non parte mai: nessuna catena lo raggiunge da un lancio.");
        }

        private static List<string> FindCycle(string id, Dictionary<string, List<string>> next, Dictionary<string, int> state, Stack<string> path)
        {
            if (state.TryGetValue(id, out var st))
            {
                if (st == 2) return null;
                var trail = path.Reverse().ToList();
                var from = trail.IndexOf(id);
                return trail.Skip(from).Append(id).ToList();
            }
            state[id] = 1;
            path.Push(id);
            foreach (var n in next[id])
            {
                var c = FindCycle(n, next, state, path);
                if (c != null) return c;
            }
            path.Pop();
            state[id] = 2;
            return null;
        }

        // ---- lettura dei valori ----

        private static void UnknownKeys(WorkflowCheckResult r, JsonElement e, string path, string[] known)
        {
            var shown = known.Where(k => k != "assignment" && k != "approval" && k != "on" && k != "then");
            foreach (var p in e.EnumerateObject().Where(p => !known.Contains(p.Name)))
                Error(r, Join(path, p.Name), $"chiave sconosciuta '{p.Name}'.", "chiavi ammesse qui: " + string.Join(", ", shown));
        }

        private static string RequiredString(WorkflowCheckResult r, JsonElement e, string key, string path)
        {
            if (!e.TryGetProperty(key, out var v))
            {
                Error(r, path, $"manca '{key}'.");
                return null;
            }
            if (v.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(v.GetString()))
            {
                Error(r, path, $"'{key}' è una stringa non vuota.");
                return null;
            }
            return v.GetString().Trim();
        }

        private static string OptionalString(WorkflowCheckResult r, JsonElement e, string key, string path)
        {
            if (!e.TryGetProperty(key, out var v)) return null;
            if (v.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(v.GetString()))
            {
                Error(r, path, $"'{key}' è una stringa non vuota; se non serve, toglila.");
                return null;
            }
            return v.GetString().Trim();
        }

        private static string RequiredId(WorkflowCheckResult r, JsonElement e, string key, string path)
        {
            var v = RequiredString(r, e, key, path);
            if (v != null && !KebabId.IsMatch(v))
            {
                Error(r, path, $"'{v}' non è kebab-case (minuscole, cifre e trattini).");
                return null;
            }
            return v;
        }

        private static string Join(string path, string key) => string.IsNullOrEmpty(path) ? key : path + "." + key;
        private static string Known(IEnumerable<string> ids) => "passi esistenti: " + string.Join(", ", ids);
        private static string StartName(WorkflowStart s) => s switch { WorkflowStart.AskOwner => "ask-owner", WorkflowStart.Auto => "auto", _ => "manual" };
        private static string KindName(WorkflowTriggerKind k) => k.ToString().ToLowerInvariant();

        private static void Error(WorkflowCheckResult r, string path, string message, string fix = null) =>
            r.Issues.Add(new WorkflowIssue { Severity = WorkflowSeverity.Error, Path = path, Message = message, Fix = fix });

        private static void Warning(WorkflowCheckResult r, string path, string message, string fix = null) =>
            r.Issues.Add(new WorkflowIssue { Severity = WorkflowSeverity.Warning, Path = path, Message = message, Fix = fix });
    }
}
