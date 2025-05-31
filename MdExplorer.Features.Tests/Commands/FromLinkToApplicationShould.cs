using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MdExplorer.Features.Commands.html;
using MdExplorer.Features.Configuration.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Tests.Commands
{
    [TestClass]
    public class FromLinkToApplicationShould
    {
        public ILogger FromLinkToApplicationHtml { get; private set; }

        [TestInitialize]
        public void InitTest()
        {
            
        }

        public class Log : ILogger<FromLinkToApplicationHtml>
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                throw new NotImplementedException();
            }

            void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                throw new NotImplementedException();
            }
        }

        public class MockApplicationExtensionConfiguration : IApplicationExtensionConfiguration
        {
            public List<string> GetSupportedExtensions()
            {
                // Return the default extensions for testing
                return new List<string> { "xlsx", "pdf", "bmpr", "docx", "pptx", "xls", "ppt" };
            }

            public bool IsExtensionSupported(string extension)
            {
                return GetSupportedExtensions().Contains(extension.ToLower().TrimStart('.'));
            }

            public void ReloadConfiguration()
            {
                // No-op for testing
            }
        }


        [TestMethod]
        public void ChangeLinkIntoCallAJavscriptFunction()
        {
            var log = new Log();
            var serverCache = new ServerCache();
            var extensionConfiguration = new MockApplicationExtensionConfiguration();
            var text = System.IO.File.ReadAllText(@"Commands\TestStore\ChangeLinkIntoCallAJavscriptFunction.html");
            var fromLinkToApplication = new FromLinkToApplicationHtml(log, serverCache, extensionConfiguration);

            var matches = fromLinkToApplication.GetMatches(text);
            var testCount = matches.Count;
            Assert.AreEqual(2, testCount);
            foreach (Match item in matches)
            {
                var data1 = item.Groups[1].Value;
                var data2 = item.Groups[2].Value;
                var data3 = item.Groups[3].Value;
                var data4 = item.Groups[4].Value;
            }


        }
    }
}
