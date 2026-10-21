using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Remote.Protocol;

namespace MdExplorer.RemoteAgent
{
    /// <summary>I remote da un file <c>config</c> di git, senza il programma <c>git</c> (sul server può non esserci).</summary>
    public static class GitConfig
    {
        /// <summary>Le sezioni <c>[remote "nome"]</c> con il loro <c>url</c>, «origin» per primo.</summary>
        public static List<RemoteGitRemote> Remotes(IEnumerable<string> lines)
        {
            var remotes = new List<RemoteGitRemote>();
            string? current = null;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    current = null;
                    const string prefix = "[remote \"";
                    if (line.StartsWith(prefix, StringComparison.Ordinal) && line.EndsWith("\"]", StringComparison.Ordinal))
                        current = line.Substring(prefix.Length, line.Length - prefix.Length - 2);
                    continue;
                }
                if (current == null) continue;
                var eq = line.IndexOf('=');
                if (eq < 0 || line.Substring(0, eq).Trim() != "url") continue;
                if (remotes.Any(r => r.Name == current)) continue;
                remotes.Add(new RemoteGitRemote { Name = current, Url = line.Substring(eq + 1).Trim() });
            }
            return remotes.OrderBy(r => r.Name == "origin" ? 0 : 1).ToList();
        }
    }
}
