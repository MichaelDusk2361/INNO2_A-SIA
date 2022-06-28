using A_SIA2WebAPI.BL.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.Test.IntegrationTests.API.Controller
{
    /// <summary>
    /// All ASP.Net integration test classes must derive from this class. <br/>
    /// It provides a test version of the api server and a http client to communicate with it.
    /// </summary>
    public abstract class ApiIntegrationTestInitializer : WebApplicationFactory<Startup>
    {
        protected HttpClient _client;
        protected TestServer _server;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing")
                .UseStartup<Startup>();
            base.ConfigureWebHost(builder);
        }

        [SetUp]
        public virtual void SetUp()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Testing")
                .UseStartup<Startup>();

            _server = new TestServer(builder);

            _client = _server.CreateClient();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _client.Dispose();
            _server.Dispose();
        }

        /// <summary>
        /// Use for authenticated actions
        /// </summary>
        /// <returns></returns>
        protected async Task<string> AuthenticateTestUser(string email, string password)
        {
            // Arrange
            LoginContent content = new(email, password);

            // Act
            var response = await _client.PostAsync(
                "api/user/authenticate",
                content);
            response.EnsureSuccessStatusCode();

            // Return
            return (await response.Content.ReadAsStringAsync()).Replace("\"", "");
        }
    }
}
