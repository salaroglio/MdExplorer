using System;

namespace MdExplorer.RemoteProtocol
{
    /// <summary>Un rifiuto con il suo codice (<see cref="MdExplorer.RemoteProtocol.RemoteErrorCodes"/>).</summary>
    public sealed class RemoteFailure : Exception
    {
        public string Code { get; }

        public RemoteFailure(string code, string message) : base(message) { Code = code; }
    }
}
