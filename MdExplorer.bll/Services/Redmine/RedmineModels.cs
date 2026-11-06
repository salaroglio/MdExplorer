using System;
using System.Collections.Generic;
using System.Net;

namespace MdExplorer.Features.Services.Redmine
{
    /// <summary>Dove sta Redmine e con quale chiave API personale (header <c>X-Redmine-API-Key</c>).</summary>
    public class RedmineConnection
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
    }

    /// <summary>
    /// Una chiamata a Redmine non è andata: lo stato HTTP c'è quando Redmine (o ciò che gli sta davanti) ha risposto. Si
    /// solleva invece di restituire un risultato vuoto (regola #2): un elenco vuoto deve voler dire «non c'è niente».
    /// </summary>
    public class RedmineApiException : Exception
    {
        public HttpStatusCode? StatusCode { get; }

        /// <summary>401/403: un problema della chiave o dei permessi, che l'utente deve sistemare.</summary>
        public bool IsAuthFailure => StatusCode == HttpStatusCode.Unauthorized || StatusCode == HttpStatusCode.Forbidden;

        public RedmineApiException(string message, HttpStatusCode? statusCode = null, Exception inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>Un riferimento di Redmine (progetto, tracker, stato, utente…): id e nome.</summary>
    public class RedmineRef
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>L'utente della chiave. Mai la chiave stessa: <c>/users/current.json</c> la restituisce, qui non entra.</summary>
    public class RedmineUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public bool Admin { get; set; }
    }

    public class RedmineIssueSummary
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public RedmineRef Project { get; set; }
        public RedmineRef Tracker { get; set; }
        public RedmineRef Status { get; set; }
        public bool StatusIsClosed { get; set; }
        public RedmineRef Priority { get; set; }
        public RedmineRef Author { get; set; }
        public RedmineRef AssignedTo { get; set; }
        public string Subject { get; set; }
        public string StartDate { get; set; }
        public string DueDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class RedmineCustomFieldValue
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>Il valore; per i campi a più valori, separati da virgola.</summary>
        public string Value { get; set; }
    }

    public class RedmineJournal
    {
        public int Id { get; set; }
        public RedmineRef User { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Notes { get; set; }
        /// <summary>I cambi di quella voce, già leggibili: «status_id: 1 → 2».</summary>
        public List<string> Changes { get; set; } = new List<string>();
    }

    public class RedmineIssueDetail : RedmineIssueSummary
    {
        public string Description { get; set; }
        public int? DoneRatio { get; set; }
        public DateTime? CreatedOn { get; set; }
        public List<RedmineCustomFieldValue> CustomFields { get; set; } = new List<RedmineCustomFieldValue>();
        public List<RedmineJournal> Journals { get; set; } = new List<RedmineJournal>();
        /// <summary>Gli stati in cui QUESTO utente può portare la issue (Redmine 5+).</summary>
        public List<RedmineRef> AllowedStatuses { get; set; } = new List<RedmineRef>();
        public List<string> Attachments { get; set; } = new List<string>();
    }

    /// <summary>Che cosa serve per creare o aggiornare bene in un progetto, e che cosa l'utente può fare lì.</summary>
    public class RedmineProjectOptions
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public List<RedmineRef> Trackers { get; set; } = new List<RedmineRef>();
        public List<RedmineRef> Statuses { get; set; } = new List<RedmineRef>();
        public List<RedmineRef> Priorities { get; set; } = new List<RedmineRef>();
        public List<RedmineRef> Members { get; set; } = new List<RedmineRef>();
        /// <summary>Id e nome; l'API non dice se sono obbligatori né i valori ammessi (lo dice il 422 in creazione).</summary>
        public List<RedmineRef> CustomFields { get; set; } = new List<RedmineRef>();
        /// <summary>I permessi dei ruoli dell'utente nel progetto (<c>add_issues</c>, <c>edit_issues</c>, <c>add_issue_notes</c>…).</summary>
        public List<string> MyPermissions { get; set; } = new List<string>();
    }

    public class RedmineIssueQuery
    {
        /// <summary>Identificativi o id dei progetti; vuoto = tutti quelli visibili.</summary>
        public List<string> Projects { get; set; } = new List<string>();
        public bool AssignedToMe { get; set; }
        /// <summary><c>open</c> (default), <c>closed</c>, <c>*</c> per tutti, o il nome/id di uno stato.</summary>
        public string Status { get; set; }
        /// <summary>Nome o id del tracker.</summary>
        public string Tracker { get; set; }
        /// <summary>Testo cercato in titolo, descrizione e note (<c>any_searchable</c>, Redmine 5+).</summary>
        public string Text { get; set; }
        public int Limit { get; set; } = 25;
    }

    /// <summary>Arriva anche dal corpo di una richiesta HTTP: tutto facoltativo (<c>string?</c>), o ASP.NET lo pretende.</summary>
    public class RedmineIssueChange
    {
        /// <summary>Solo in creazione: identificativo o id del progetto.</summary>
        public string? Project { get; set; }
        public string? Tracker { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        /// <summary>Nome, login o id di un membro del progetto; <c>me</c> per sé stessi; <c>none</c> per togliere.</summary>
        public string? AssignedTo { get; set; }
        /// <summary>Campo (nome o id) → valore.</summary>
        public Dictionary<string, string>? CustomFields { get; set; } = new Dictionary<string, string>();
        /// <summary>Solo in aggiornamento: una nota.</summary>
        public string? Notes { get; set; }
    }
}
