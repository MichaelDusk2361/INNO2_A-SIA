using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.Test.IntegrationTests.API.Controller
{
    public class InstanceControllerTest : ApiIntegrationTestInitializer
    {
        private Instance _instance;
        private User _user;
        private User _user2;
        private string _password = "test";
        private string _password2 = "test2";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            using Neo4JDatabase db = new();
            Neo4JEngine engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);
            var hasher = new PasswordHasher<User>();

            _instance = new()
            {
                Name = "New Instance!"
            };
            _user = new()
            {
                Email = $"{Guid.NewGuid()}",
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Hash = _password,
                Id = Guid.Empty,
            };
            _user.Hash = hasher.HashPassword(_user, _user.Hash);

            _user2 = new()
            {
                Email = $"{Guid.NewGuid()}",
                FirstName = "Test2FirstName",
                LastName = "Test2LastName",
                Hash = _password2,
                Id = Guid.Empty,
            };
            _user2.Hash = hasher.HashPassword(_user2, _user2.Hash);

            Assert.True(engine.TryCreate(ref _user));
            Assert.True(engine.TryCreate(ref _user2));
            Assert.True(engine.TryCreate(ref _instance));

            var rel1 = new InstanceRoleRelation()
            {
                From = _user.Id,
                To = _instance.Id,
                Authority = EntityAuthority.Owner
            };
            Assert.True(engine.TryCreate(ref rel1));
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            using Neo4JDatabase db = new();
            Neo4JEngine engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);

            engine.TryDelete<User>(_user.Id, true);
            engine.TryDelete<User>(_user2.Id, true);
            engine.TryDelete<Instance>(_instance.Id, true);
        }

        #region GET

        [Test]
        public async Task GetInstanceWithoutTokenShouldReturn401()
        {
            // Act
            var response = await _client.GetAsync(
                "api/instance/0");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task GetInstanceWithCorrectUserShouldReturn200()
        {
            string token = await AuthenticateTestUser(_user.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/instance/{_instance.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetInstanceWithOtherUserShouldReturn401()
        {
            string token = await AuthenticateTestUser(_user2.Email, _password2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/instance/{_instance.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region DELETE

        [Test]
        public async Task DeleteInstanceWithoutTokenShouldReturn401()
        {
            // Act
            var response = await _client.DeleteAsync("api/instance/0");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task DeleteInstanceWithCorrectUserShouldReturn200()
        {
            string token = await AuthenticateTestUser(_user.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/instance/{_instance.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task DeleteInstanceWithOtherUserShouldReturn401()
        {
            string token = await AuthenticateTestUser(_user2.Email, _password2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/instance/{_instance.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region POST

        class InstanceContent : HttpContent
        {
            private Instance _instance;

            public InstanceContent(Instance instance)
            {
                _instance = instance;

                Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                using var sw = new StreamWriter(stream);
                return sw.WriteAsync(JsonConvert.SerializeObject(_instance));
            }

            protected override bool TryComputeLength(out long length)
            {
                length = JsonConvert.SerializeObject(_instance).Length;
                return true;
            }
        }

        [Test]
        public async Task CreateInstanceWithoutTokenShouldReturn401()
        {
            // Arrange
            InstanceContent content = new(_instance);

            // Act
            var response = await _client.PostAsync(
                "api/instance",
                content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task CreateInstanceShouldReturn200()
        {
            using Neo4JDatabase db = new();
            Neo4JEngine engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);

            string token = await AuthenticateTestUser(_user.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Instance instance2 = new()
            {
                Name = "New Instance!!!",
                Description = "This is my new instance"
            };

            // Act
            var response = await _client.PostAsync(
                $"api/instance",
                new InstanceContent(instance2));

            Instance returnedInstance = JsonConvert.DeserializeObject<Instance>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(instance2.Name, returnedInstance.Name);
            Assert.AreEqual(instance2.Description, returnedInstance.Description);

            engine.TryDelete<Instance>(returnedInstance.Id);
        }

        #endregion

        #region PUT

        [Test]
        public async Task UpdateInstanceWithoutTokenShouldReturn401()
        {
            // Arrange
            InstanceContent content = new(_instance);

            // Act
            var response = await _client.PutAsync(
                "api/instance/0",
                content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task UpdateInstanceWithCorrectUserShouldReturn200()
        {
            using Neo4JDatabase db = new();
            Neo4JEngine engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);

            string token = await AuthenticateTestUser(_user.Email, _password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _instance.Name = "TEST_INSTANCE";

            // Act
            var response = await _client.PutAsync(
                $"api/instance/{_instance.Id}",
                new InstanceContent(_instance));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("TEST_INSTANCE", engine.Get<Instance>(_instance.Id)?.Name);
        }

        [Test]
        public async Task UpdateInstanceWithOtherUserShouldReturn401()
        {
            string token = await AuthenticateTestUser(_user2.Email, _password2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _instance.Name = "TEST_INSTANCE";

            // Act
            var response = await _client.PutAsync(
                $"api/instance/{_instance.Id}",
                new InstanceContent(_instance));

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion
    }
}
