using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// L'identità legata a un RunToken (R2): chi è l'agente svegliato e in quale contesto.
    /// I messaggi in uscita (tool MCP <c>SendAgentMessage</c>) si autenticano col token e
    /// il Service risale a queste claim — così <c>accepts_messages_from</c> non è aggirabile
    /// da un processo locale che si spaccia per un altro agente.
    /// </summary>
    public class RunTokenClaims
    {
        public Guid RunId { get; set; }
        public string AgentName { get; set; }
        public string ProjectPath { get; set; }
        public string ConversationId { get; set; }
    }

    public interface IRunTokenStore
    {
        /// <summary>Genera un token opaco legato alle claim, valido fino alla revoca a fine run.</summary>
        string Mint(RunTokenClaims claims);

        /// <summary>Restituisce le claim se il token è valido, altrimenti null. Non consuma.</summary>
        RunTokenClaims Validate(string token);

        /// <summary>Revoca il token a fine run (il token è "monouso" nel senso di un run).</summary>
        void Revoke(string token);
    }

    /// <summary>
    /// Store in-memory dei RunToken (R2, §10). Il gateway/dispatcher genera un token a ogni
    /// risveglio, lo passa nell'<b>ambiente del processo</b> Copilot (mai nel prompt), il
    /// processo MCP figlio lo eredita e lo presenta al Service, che risale all'identità.
    /// "Monouso" = un token per run, revocato a fine run (non consumato al primo uso: un run
    /// può inviare più messaggi).
    /// </summary>
    public class RunTokenStore : IRunTokenStore
    {
        private readonly ConcurrentDictionary<string, RunTokenClaims> _tokens = new();

        public string Mint(RunTokenClaims claims)
        {
            if (claims == null) throw new ArgumentNullException(nameof(claims));
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
            _tokens[token] = claims;
            return token;
        }

        public RunTokenClaims Validate(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            return _tokens.TryGetValue(token, out var claims) ? claims : null;
        }

        public void Revoke(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            _tokens.TryRemove(token, out _);
        }
    }
}
