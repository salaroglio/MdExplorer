using Microsoft.Extensions.Logging;
using MdExplorer.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models;

namespace MdExplorer.Tests.Controllers
{
    [TestClass]
    public class MdExplorer
    {
        [TestMethod]
        public async Task GetPage()
        {
            var mockLogger = new Mock<ILogger<MdExplorerController>>();
            var mockWatcher = new FileSystemWatcher();
            mockWatcher.Path = @".\Documentation";
            var controller = new MdExplorerController(mockLogger.Object, mockWatcher);

            var mdFile = new FileInfoNode { Name = "home.md", Path = "Home.md" };

            var something = await controller.GetPageAsync(mdFile);
            

        }
    }
}
