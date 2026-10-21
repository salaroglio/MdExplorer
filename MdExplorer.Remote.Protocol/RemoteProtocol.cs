namespace MdExplorer.Remote.Protocol
{
    /// <summary>
    /// Il protocollo tra il backend e <c>mde-remote</c>: una riga JSON per messaggio su stdin/stdout del comando lanciato
    /// via <c>ssh</c>. Il serverino saluta per primo (D6), poi serve una richiesta alla volta, nell'ordine.
    /// </summary>
    public static class RemoteProtocol
    {
        /// <summary>Si alza quando cambia la forma dei messaggi: il backend rifiuta un serverino con un altro numero.</summary>
        public const int Version = 1;

        /// <summary>Exit code del serverino quando la radice non esiste o non è una cartella: niente saluto.</summary>
        public const int ExitRootNotFound = 3;

        /// <summary>Exit code per argomenti sbagliati.</summary>
        public const int ExitBadArguments = 2;
    }

    /// <summary>Le operazioni (<see cref="RemoteRequest.Op"/>).</summary>
    public static class RemoteOps
    {
        public const string List = "list";
        public const string Stat = "stat";
        public const string Read = "read";
        public const string Write = "write";
        public const string Create = "create";
        public const string Mkdir = "mkdir";
        public const string Delete = "delete";
        public const string Move = "move";
        public const string Grep = "grep";
        public const string GitInfo = "gitInfo";
    }

    /// <summary>I codici d'errore (<see cref="RemoteError.Code"/>): il backend li traduce in messaggi per l'utente.</summary>
    public static class RemoteErrorCodes
    {
        public const string BadRequest = "bad_request";
        public const string Unsupported = "unsupported";
        public const string OutsideRoot = "outside_root";
        public const string Link = "link";
        public const string NotFound = "not_found";
        public const string Exists = "exists";
        public const string NotEmpty = "not_empty";
        public const string Changed = "changed";
        public const string Binary = "binary";
        public const string NotUtf8 = "not_utf8";
        public const string Denied = "denied";
        public const string Timeout = "timeout";
        public const string Io = "io";
        public const string Internal = "internal";
    }
}
