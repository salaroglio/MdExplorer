using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MdExplorer.Features.Exports;
using MdExplorer.Features.Yaml.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Exports
{
    [TestClass]
    public class WordTemplateServiceTests
    {
        private WordTemplateService _service;
        private ILogger<WordTemplateService> _logger;

        [TestInitialize]
        public void Setup()
        {
            // Use null logger for tests
            _logger = new TestLogger<WordTemplateService>();
            _service = new WordTemplateService(_logger);
        }

        [TestMethod]
        public void ReplaceTags_Should_Replace_Simple_Tags()
        {
            // Arrange
            var template = "Hello {{name}}, welcome to {{company}}!";
            var tags = new Dictionary<string, string>
            {
                { "name", "Mario" },
                { "company", "Azienda SpA" }
            };

            // Act
            var result = _service.ReplaceTags(template, tags);

            // Assert
            Assert.AreEqual("Hello Mario, welcome to Azienda SpA!", result);
        }

        [TestMethod]
        public void ReplaceTags_Should_Handle_Default_Values()
        {
            // Arrange
            var template = "Version: {{version|1.0}}, Status: {{status|Draft}}";
            var tags = new Dictionary<string, string>
            {
                { "version", "2.0" }
            };

            // Act
            var result = _service.ReplaceTags(template, tags);

            // Assert
            Assert.AreEqual("Version: 2.0, Status: Draft", result);
        }

        [TestMethod]
        public async Task InsertPredefinedPagesAsync_Should_Return_Original_If_No_Pages_Configured()
        {
            // Arrange
            var originalContent = "# My Document\n\nContent here";
            var descriptor = new MdExplorerDocumentDescriptor
            {
                WordSection = new WordSection()
            };

            // Act
            var result = await _service.InsertPredefinedPagesAsync(originalContent, descriptor, "/project");

            // Assert
            Assert.AreEqual(originalContent, result);
        }

        [TestMethod]
        public void ReplaceTags_Should_Keep_Original_Tag_If_Not_Found()
        {
            // Arrange
            var template = "Hello {{name}}, your ID is {{id}}";
            var tags = new Dictionary<string, string>
            {
                { "name", "Mario" }
            };

            // Act
            var result = _service.ReplaceTags(template, tags);

            // Assert
            Assert.AreEqual("Hello Mario, your ID is {{id}}", result);
        }

        [TestMethod]
        public void ReplaceTags_Should_Handle_Empty_Template()
        {
            // Arrange
            var template = "";
            var tags = new Dictionary<string, string>
            {
                { "name", "Mario" }
            };

            // Act
            var result = _service.ReplaceTags(template, tags);

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ReplaceTags_Should_Handle_Null_Template()
        {
            // Arrange
            string template = null;
            var tags = new Dictionary<string, string>
            {
                { "name", "Mario" }
            };

            // Act
            var result = _service.ReplaceTags(template, tags);

            // Assert
            Assert.IsNull(result);
        }
    }

    // Simple test logger implementation
    public class TestLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
    }
}