namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public interface IPasswordProtector
    {
        string Protect(string plaintext);
        string Unprotect(string ciphertext);
    }
}
