using System.Net;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Derisca il rischio più grosso dell'harness: che <see cref="AgentCityFactory"/> riesca
    /// davvero a ospitare il Service reale in-process (Startup pesante, migrazioni, DI) e a
    /// servire richieste. Se questo passa, il resto della suite A2A ci si costruisce sopra.
    /// </summary>
    [TestClass]
    public class HostingSmoke_Should
    {
        [TestMethod]
        public async Task Boot_the_service_and_answer_a_basic_request()
        {
            using var factory = new AgentCityFactory();
            var client = factory.CreateClient();

            var resp = await client.GetAsync("/api/MdProjects/GetProjects");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }
    }
}
