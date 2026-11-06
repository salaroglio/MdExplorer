namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Hook invoked right after a project has been opened (Project row saved in the
    /// UserDB). Handlers must be fast or fire-and-forget: a slow/broken handler must
    /// never delay or break project opening (the dispatcher wraps each call in
    /// try/catch).
    /// </summary>
    public interface IProjectOpenedEventHandler
    {
        void OnProjectOpened(string projectPath);
    }
}
