using DocumentFormat.OpenXml.Wordprocessing;
using MdExplorer.Abstractions.Models.GIT;
using MdExplorer.Features.GIT;
using MdExplorer.Features.GIT.models;
using MdExplorer.Features.snippets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Tests.snippets
{
    [TestClass]
    public class GanttShould
    {
        IServiceCollection serviceCollection;

        [TestInitialize]
        public void InitTest()
        {
            serviceCollection = new ServiceCollection();
            serviceCollection.AddMDExplorerCommands();
            // La factory degli snippet costruisce TextDocument -> GitService, che a runtime
            // richiede IUserSettingsDB (non disponibile in un unit test). Sostituiamo il
            // git con un fake: l'ultima registrazione vince, così la factory usa questo.
            serviceCollection.AddSingleton<IGitService, FakeGitService>();
        }

        [TestMethod]
        public void AsUserGetSnippet()
        {
            
            var provider = serviceCollection.BuildServiceProvider();
            var _snippets = provider.GetService<ISnippet<DictionarySnippetParam>[]>();
            
            // Text Document Management
            var templateContent = string.Empty;
            var snippetTextDocument = _snippets.Where(_ => _.Id == 0).FirstOrDefault();
            var dictParam = new DictionarySnippetParam();
            dictParam.Add(ParameterName.StringDocumentTitle, "User should create Gantt");
            dictParam.Add(ParameterName.ProjectPath, @"c:\test");
            dictParam.Add(ParameterName.DocumentType, "document");
            templateContent = snippetTextDocument.GetSnippet(dictParam);
            Assert.IsNotNull(templateContent);

        }
    }

    /// <summary>Git fittizio per i test degli snippet: nessuna dipendenza dalla UserDB.</summary>
    internal class FakeGitService : IGitService
    {
        public string GetCurrentUser(string projectPath) => "Test User";
        public string GetCurrentUserEmail(string projectPath) => "test.user@example.com";
        public string GetCurrentBranch(string projectPath) => "main";
        public int HowManyFilesAreChanged(string projectPath) => 0;
        public int HowManyFilesAreToPull(string projectPath) => 0;
        public int CountCommitsBehindTrackedBranch(string projectPath) => 0;
        public GitBranch[] GetBranches(string projectPath) => Array.Empty<GitBranch>();
        public GitTag[] GetTagList(string path) => Array.Empty<GitTag>();
        public GitBranch CheckoutBranch(GitBranch branch, string path, GitService.GitCallBack callback) => branch;
        public bool CloneRepository(CloneInfo request) => true;
        public (bool IsConnectionMissing, bool IsAuthenticationMissing, bool ThereAreConflicts, string ErrorMessage) Pull(PullInfo pullInfo) => (false, false, false, string.Empty);
        public (bool, bool, bool, string) CommitAndPush(PullInfo commitAndPushInfo) => (false, false, false, string.Empty);
        public (bool, bool, bool, string) Commit(PullInfo commitAndPushInfo) => (false, false, false, string.Empty);
        public (bool, bool, bool, string) Push(PullInfo commitAndPushInfo) => (false, false, false, string.Empty);
        public IList<FileNameAndAuthor> GetFilesChangesAndAuthors(string projectPath, string repository, string branch) => new List<FileNameAndAuthor>();
        public IList<FileNameAndAuthor> GetFilesAndAuthorsToBeChanged(string projectPath) => new List<FileNameAndAuthor>();
        public IList<FileNameAndAuthor> CheckExistenceAccountAndGetFilesAndAuthorsToBeChanged(string projectPath, PullInfo pullInfo) => new List<FileNameAndAuthor>();
    }
}
