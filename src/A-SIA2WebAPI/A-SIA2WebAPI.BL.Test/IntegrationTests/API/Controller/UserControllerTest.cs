using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static A_SIA2WebAPI.BL.API.Payloads.UserController;

namespace A_SIA2WebAPI.BL.Test.IntegrationTests.API.Controller
{
    public class UserControllerTest : ApiIntegrationTestInitializer
    {
        private User _testUser;
        private User _testUser2;
        private string _password = "s1SFvx79LPgmuyP2";
        private PasswordHasher<User> _hasher = new();

        [SetUp]
        public override void SetUp()
        {
            // Create test data in database
            CreateTestData();

            base.SetUp();
        }

        #region Test Data SetUp

        private void CreateTestData()
        {
            using Neo4JDatabase db = new();
            Neo4JEngine engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);

            _testUser = new()
            {
                Email = $"{Guid.NewGuid()}@test.test",
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Hash = "",
                Id = Guid.Empty,
            };
            _testUser.Hash = _hasher.HashPassword(_testUser, _password);

            if(!engine.TryCreate(ref _testUser))
            {
                throw new Exception("Could not create user in database!");
            }

            _testUser2 = null;
        }
        private void DeleteTestData()
        {
            using Neo4JDatabase db = new();
            Neo4JEngine engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);

            engine.TryDelete<User>(_testUser.Id);

            if(_testUser2 != null)
                engine.TryDelete<User>(_testUser2.Id);
        }

        #endregion

        #region POST Authentication

        [Test]
        public async Task AuthInvalidCredentialsShouldReturn401()
        {
            // Arrange
            LoginContent content = new("INVALID_USER", "INVALID_PASSWORD");

            // Act
            var response = await _client.PostAsync(
                "api/user/authenticate",
                content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task AuthInvalidPasswordShouldReturn401()
        {
            // Arrange
            LoginContent content = new(_testUser.Email, "INVALID_PASSWORD");

            // Act
            var response = await _client.PostAsync(
                "api/user/authenticate",
                content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task AuthCorrectCredentialsShouldReturnToken()
        {
            // Arrange
            LoginContent content = new(_testUser.Email, _password);

            // Act
            var response = await _client.PostAsync(
                "api/user/authenticate",
                content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region POST Register

        /// <summary>
        /// Test content for registering
        /// </summary>
        class RegisterContent : HttpContent
        {
            public RegisterBody Body;

            public RegisterContent(
                string email,
                string password,
                string firstName,
                string lastName)
            {
                Body = new()
                {
                    Email = email,
                    Password = password,
                    FirstName = firstName,
                    LastName = lastName
                };

                Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            private byte[] GetPayload()
            {
                return Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(Body));
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                using var writer = new StreamWriter(stream);
                return stream.WriteAsync(GetPayload()).AsTask();
            }

            protected override bool TryComputeLength(out long length)
            {
                length = GetPayload().Length;
                return length > 0;
            }
        }

        [Test]
        public async Task RegisterUserInvalidBodyShouldReturn400()
        {
            // Arrange
            RegisterContent content = new("test@test1", null, null, null);

            // Act
            var response = await _client.PostAsync(
                "api/user",
                content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task RegisterUserDuplicateEmailShouldReturn400()
        {
            // Arrange
            RegisterContent content = new(
                _testUser.Email, "TestUser2Pwd", "TestUser2FirstName",
                "TestUser2LastName");

            // Act
            var response = await _client.PostAsync(
                "api/user",
                content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task RegisterUserShouldReturn201()
        {
            // Arrange
            RegisterContent content = new(
                $"{Guid.NewGuid()}", "TestUser2Pwd", "TestUser2FirstName",
                "TestUser2LastName");

            // Act
            var response = await _client.PostAsync(
                "api/user",
                content);
            _testUser2 = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(content.Body.Email, _testUser2.Email);
        }

        #endregion

        #region GET User

        [Test]
        public async Task GetUserWithWrongEmailShouldReturn400()
        {
            // Arrange
            string token = await AuthenticateTestUser(_testUser.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/user/FALSE@FALSE.FALSE");

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetUserWithEmailShouldReturn200()
        {
            // Arrange
            string token = await AuthenticateTestUser(_testUser.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/user/{_testUser.Email}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetUserWithWrongIdShouldReturn400()
        {
            // Arrange
            string token = await AuthenticateTestUser(_testUser.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/user/{Guid.NewGuid()}");

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetUserWithIdShouldReturn200()
        {
            // Arrange
            string token = await AuthenticateTestUser(_testUser.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/user/{_testUser.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region DELETE User

        [Test]
        public async Task DeleteUserWithWrongEmailShouldReturn400()
        {            
            // Arrange
            string token = await AuthenticateTestUser(_testUser.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/user/FALSE@FALSE.FALSE");

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task DeleteUserWithEmailShouldReturn200()
        {
            // Arrange
            string token = await AuthenticateTestUser(_testUser.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            // Act
            var response = await _client.DeleteAsync(
                $"api/user/{_testUser.Email}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task DeleteUserWithWrongIdShouldReturn400()
        {
            // Arrange
            string token = await AuthenticateTestUser(_testUser.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/user/{Guid.NewGuid()}");

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task DeleteUserWithIdShouldReturn200()
        {
            // Arrange
            string token = await AuthenticateTestUser(_testUser.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/user/{_testUser.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            DeleteTestData();
        }
    }

    /// <summary>
    /// Test content for logging in
    /// </summary>
    class LoginContent : HttpContent
    {
        private CredentialsBody _body;

        public LoginContent(string email, string password)
        {
            _body = new()
            {
                Email = email,
                Password = password
            };

            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        private byte[] GetPayload()
        {
            return Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(_body));
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using var writer = new StreamWriter(stream);
            return stream.WriteAsync(GetPayload()).AsTask();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = GetPayload().Length;
            return length > 0;
        }
    }
}