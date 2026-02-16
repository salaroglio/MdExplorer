using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MdExplorer.P2P.Premium.Controllers;
using MdExplorer.P2P.Premium.Services;
using MdExplorer.P2P.Premium.Models;

namespace MdExplorer.P2P.Premium.Tests
{
    [TestClass]
    public class P2PControllerTests
    {
        private Mock<IP2PService> _serviceMock = null!;
        private Mock<ILogger<P2PController>> _loggerMock = null!;
        private P2PController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _serviceMock = new Mock<IP2PService>();
            _loggerMock = new Mock<ILogger<P2PController>>();
            _controller = new P2PController(_serviceMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task GetStatus_ReturnsOk_WithStatus()
        {
            // Arrange
            _serviceMock.Setup(s => s.IsAvailableAsync()).ReturnsAsync(true);
            _serviceMock.Setup(s => s.GetStatsAsync()).ReturnsAsync(new P2PStats
            {
                Torrents = 1,
                DownloadSpeed = 100000
            });

            // Act
            var result = await _controller.GetStatus();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            var status = (P2PStatus)okResult.Value!;
            Assert.IsTrue(status.Enabled);
        }

        [TestMethod]
        public async Task GetStatus_ReturnsOk_WhenServiceUnavailable()
        {
            // Arrange
            _serviceMock.Setup(s => s.IsAvailableAsync()).ReturnsAsync(false);

            // Act
            var result = await _controller.GetStatus();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            var status = (P2PStatus)okResult.Value!;
            Assert.IsFalse(status.Enabled);
        }

        [TestMethod]
        public async Task GetHealth_ReturnsOk_WhenHealthy()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetHealthAsync()).ReturnsAsync(new HealthResponse
            {
                Status = "ok",
                Timestamp = DateTime.UtcNow.ToString("o")
            });

            // Act
            var result = await _controller.GetHealth();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetHealth_Returns503_WhenServiceUnavailable()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetHealthAsync()).ReturnsAsync((HealthResponse?)null);

            // Act
            var result = await _controller.GetHealth();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result!;
            Assert.AreEqual(503, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task GetTransfers_ReturnsOk_WithTransferList()
        {
            // Arrange
            var transfers = new List<TransferInfo>
            {
                new TransferInfo { InfoHash = "abc", Name = "test.mp4" }
            };
            _serviceMock.Setup(s => s.GetTransfersAsync()).ReturnsAsync(transfers);

            // Act
            var result = await _controller.GetTransfers();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result!;
            var returnedTransfers = (List<TransferInfo>)okResult.Value!;
            Assert.AreEqual(1, returnedTransfers.Count);
        }

        [TestMethod]
        public async Task GetTransfer_ReturnsOk_WhenFound()
        {
            // Arrange
            var transfer = new TransferInfo { InfoHash = "abc123", Name = "test.mp4" };
            _serviceMock.Setup(s => s.GetTransferAsync("abc123")).ReturnsAsync(transfer);

            // Act
            var result = await _controller.GetTransfer("abc123");

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetTransfer_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetTransferAsync("notfound")).ReturnsAsync((TransferInfo?)null);

            // Act
            var result = await _controller.GetTransfer("notfound");

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task ShareFile_ReturnsBadRequest_WhenNoFilePath()
        {
            // Arrange
            var request = new ShareRequest { FilePath = "" };

            // Act
            var result = await _controller.ShareFile(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task ShareFile_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var request = new ShareRequest { FilePath = @"C:\test\file.mp4", Name = "file.mp4" };
            var shareResult = new ShareResult
            {
                InfoHash = "abc123",
                MagnetUri = "magnet:?xt=urn:btih:abc123"
            };
            _serviceMock.Setup(s => s.ShareFileAsync(request.FilePath, request.Name))
                .ReturnsAsync(shareResult);

            // Act
            var result = await _controller.ShareFile(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Download_ReturnsBadRequest_WhenNoMagnetUri()
        {
            // Arrange
            var request = new DownloadRequest { MagnetUri = "" };

            // Act
            var result = await _controller.Download(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Download_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var request = new DownloadRequest { MagnetUri = "magnet:?xt=urn:btih:abc123" };
            var shareResult = new ShareResult
            {
                InfoHash = "abc123"
            };
            _serviceMock.Setup(s => s.DownloadAsync(request.MagnetUri, null))
                .ReturnsAsync(shareResult);

            // Act
            var result = await _controller.Download(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task PauseTransfer_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            _serviceMock.Setup(s => s.PauseTransferAsync("abc123")).ReturnsAsync(true);

            // Act
            var result = await _controller.PauseTransfer("abc123");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task PauseTransfer_Returns500_WhenFailed()
        {
            // Arrange
            _serviceMock.Setup(s => s.PauseTransferAsync("abc123")).ReturnsAsync(false);

            // Act
            var result = await _controller.PauseTransfer("abc123");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
        }

        [TestMethod]
        public async Task ResumeTransfer_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            _serviceMock.Setup(s => s.ResumeTransferAsync("abc123")).ReturnsAsync(true);

            // Act
            var result = await _controller.ResumeTransfer("abc123");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task StopTransfer_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            _serviceMock.Setup(s => s.StopTransferAsync("abc123", false)).ReturnsAsync(true);

            // Act
            var result = await _controller.StopTransfer("abc123", false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }
    }
}
