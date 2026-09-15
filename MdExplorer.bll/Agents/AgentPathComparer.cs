using System;
using System.IO;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Confronto canonico di due path (progetto o file agente) usato dalla città degli agenti.
    /// Normalizza via <see cref="Path.GetFullPath(string)"/> e ignora un separatore finale, con
    /// fallback letterale se la normalizzazione non è possibile. Unica fonte di verità: la
    /// mailbox lo usa per <b>ancorare una conversazione al suo progetto</b> (un contextId di un
    /// altro progetto non deve poter agganciare né consumare il budget hop di quella
    /// conversazione).
    /// </summary>
    public static class AgentPathComparer
    {
        public static bool Equals(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;
            try
            {
                return string.Equals(
                    Path.GetFullPath(a).TrimEnd('/', '\\'),
                    Path.GetFullPath(b).TrimEnd('/', '\\'),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
