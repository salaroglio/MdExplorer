using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using MdExplorer.P2P.Premium.Services;
using MdExplorer.P2P.Premium.Models;

namespace MdExplorer.P2P.Premium.Tests
{
    [TestClass]
    public class P2PServiceProxyTests
    {
        private Mock<ILogger<P2PServiceProxy>> _loggerMock = null!;
        private Mock<HttpMessageHandler> _httpHandlerMock = null!;
        private HttpClient _httpClient = null!;
        private P2PServiceProxy _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<P2PServiceProxy>>();
            _httpHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpHandlerMock.Object)
            {
                BaseAddress = new Uri("http://127.0.0.1:48124")
            };
            _service = new P2PServiceProxy(_httpClient, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _httpClient?.Dispose();
        }

        [TestMethod]
        public async Task IsAvailableAsync_ReturnsTrue_WhenHealthEndpointSucceeds()
        {
            // Arrange
            SetupHttpResponse("/health", HttpStatusCode.OK, new { status = "ok" });

            // Act
            var result = await _service.IsAvailableAsync();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsAvailableAsync_ReturnsFalse_WhenHealthEndpointFails()
        {
            // Arrange
            SetupHttpResponse<object>("/health", HttpStatusCode.ServiceUnavailable, null);

            // Act
            var result = await _service.IsAvailableAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsAvailableAsync_ReturnsFalse_WhenExceptionOccurs()
        {
            // Arrange
            _httpHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            // Act
            var result = await _service.IsAvailableAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetHealthAsync_ReturnsHealthResponse_WhenSuccessful()
        {
            // Arrange
            var healthResponse = new HealthResponse
            {
                Status = "ok",
                Timestamp = DateTime.UtcNow.ToString("o"),
                Version = "1.0.0"
            };
            SetupHttpResponse("/health", HttpStatusCode.OK, healthResponse);

            // Act
            var result = await _service.GetHealthAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ok", result.Status);
        }

        [TestMethod]
        public async Task GetStatsAsync_ReturnsStats_WhenSuccessful()
        {
            // Arrange
            var stats = new P2PStats
            {
                Torrents = 2,
                DownloadSpeed = 100000,
                UploadSpeed = 50000,
                Progress = 75.5,
                Ratio = 1.5
            };
            SetupHttpResponse("/stats", HttpStatusCode.OK, stats);

            // Act
            var result = await _service.GetStatsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Torrents);
            Assert.AreEqual(100000, result.DownloadSpeed);
        }

        [TestMethod]
        public async Task GetTransfersAsync_ReturnsEmptyList_WhenNoTransfers()
        {
            // Arrange
            SetupHttpResponse("/transfers", HttpStatusCode.OK, new List<TransferInfo>());

            // Act
            var result = await _service.GetTransfersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetTransfersAsync_ReturnsTransfers_WhenAvailable()
        {
            // Arrange
            var transfers = new List<TransferInfo>
            {
                new TransferInfo
                {
                    InfoHash = "abc123",
                    Name = "test-file.mp4",
                    Progress = 50.0,
                    DownloadSpeed = 100000,
                    Size = 1000000,
                    Type = "downloading"
                }
            };
            SetupHttpResponse("/transfers", HttpStatusCode.OK, transfers);

            // Act
            var result = await _service.GetTransfersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("test-file.mp4", result[0].Name);
            Assert.AreEqual(50.0, result[0].Progress);
        }

        [TestMethod]
        public async Task ShareFileAsync_ReturnsShareResult_WhenSuccessful()
        {
            // Arrange
            var shareResult = new ShareResult
            {
                InfoHash = "def456",
                MagnetUri = "magnet:?xt=urn:btih:def456&dn=shared-file.zip",
                Name = "shared-file.zip",
                Size = 5000000
            };
            SetupHttpResponse("/share", HttpStatusCode.OK, shareResult);

            // Act
            var result = await _service.ShareFileAsync(@"C:\test\shared-file.zip", "shared-file.zip");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("def456", result.InfoHash);
            Assert.IsTrue(result.MagnetUri.Contains("magnet:"));
        }

        [TestMethod]
        public async Task DownloadAsync_ReturnsShareResult_WhenSuccessful()
        {
            // Arrange
            var shareResult = new ShareResult
            {
                InfoHash = "ghi789",
                Name = "downloaded-file.mp4"
            };
            SetupHttpResponse("/download", HttpStatusCode.OK, shareResult);

            // Act
            var result = await _service.DownloadAsync("magnet:?xt=urn:btih:ghi789");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ghi789", result.InfoHash);
        }

        [TestMethod]
        public async Task PauseTransferAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            SetupHttpResponse("/transfers/abc123/pause", HttpStatusCode.OK, new { success = true });

            // Act
            var result = await _service.PauseTransferAsync("abc123");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ResumeTransferAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            SetupHttpResponse("/transfers/abc123/resume", HttpStatusCode.OK, new { success = true });

            // Act
            var result = await _service.ResumeTransferAsync("abc123");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task StopTransferAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            SetupHttpResponse("/transfers/abc123", HttpStatusCode.OK, new { success = true });

            // Act
            var result = await _service.StopTransferAsync("abc123", false);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task StopTransferAsync_WithDeleteFiles_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            SetupHttpResponse("/transfers/abc123?deleteFiles=true", HttpStatusCode.OK, new { success = true });

            // Act
            var result = await _service.StopTransferAsync("abc123", true);

            // Assert
            Assert.IsTrue(result);
        }

        private void SetupHttpResponse<T>(string path, HttpStatusCode statusCode, T? content)
        {
            var response = new HttpResponseMessage(statusCode);
            if (content != null)
            {
                var json = JsonSerializer.Serialize(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            var pathWithoutQuery = path.Split('?')[0];
            _httpHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.PathAndQuery.Contains(pathWithoutQuery)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }
    }
}
