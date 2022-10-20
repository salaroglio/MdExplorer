using DocumentFormat.OpenXml.Wordprocessing;
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
            templateContent = snippetTextDocument.GetSnippet(dictParam);
            Assert.IsNotNull(templateContent);  



        }
    }
}
