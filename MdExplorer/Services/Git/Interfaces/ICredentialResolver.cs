namespace MdExplorer.Services.Git.Interfaces
{
    /// <summary>
    /// Come si è autenticata un'operazione di rete. Dallo sprint «un solo meccanismo di
    /// autenticazione» il valore possibile è uno: <see cref="GitCredentialHelper"/>, cioè il git
    /// di sistema col suo credential manager. Gli altri restano per non rompere chi legge il campo.
    /// </summary>
    public enum AuthenticationMethod
    {
        Default = 0,
        SSHKey = 1,
        GitHubToken = 2,
        SystemCredentialStore = 3,
        GitCredentialHelper = 4,
        UserPrompt = 5
    }
}
