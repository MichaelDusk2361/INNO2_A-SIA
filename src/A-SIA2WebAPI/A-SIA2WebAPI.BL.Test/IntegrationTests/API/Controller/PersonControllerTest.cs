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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.Test.IntegrationTests.API.Controller
{
    public class PersonControllerTest : ApiIntegrationTestInitializer
    {

        private Network _network1;
        private Project _project;
        private Instance _instance;
        private User _user1;
        private User _user2;

        private Person _person1;
        private Person _person2;

        private string _password1 = "test1";

        private Neo4JEngine _engine;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Neo4JDatabase db = new();
            _engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);
            var hasher = new PasswordHasher<User>();

            _person1 = new()
            {
                Name = "Person 1",
                PositionX = 2.241f,
                PositionY = 1.45f,
                Roles = new List<string>() { PersonRoles.Alpha },
                AvatarPath = "networkId/personId",
                Color = "FFFFFF",
                Description = "This is the first person",
                Persistance = 0.245f,
                Reflection = 0.8521f,
                SimulationValues = new()
            };
            _person2 = new()
            {
                Name = "Person 2",
                PositionX = 2.2241f,
                PositionY = -4.845f,
                Roles = new List<string>() { PersonRoles.Carrier },
                AvatarPath = "networkId/personId",
                Color = "AAFF15",
                Description = "This is the second person",
                Persistance = 0.845f,
                Reflection = 0.2521f,
                SimulationValues = new()
            };
            _instance = new()
            {
                Name = "Instance 1"
            };
            _project = new()
            {
                Name = "Project 1"
            };
            _network1 = new()
            {
                Name = "Network 1"
            };
            _user1 = new()
            {
                Email = $"{Guid.NewGuid()}@test1.com",
                FirstName = "Test1FirstName",
                LastName = "Test1LastName",
                Hash = _password1,
                Id = Guid.Empty,
            };
            _user1.Hash = hasher.HashPassword(_user1, _user1.Hash);
            _user2 = new()
            {
                Email = $"{Guid.NewGuid()}@test2.com",
                FirstName = "Test2FirstName",
                LastName = "Test2LastName",
                Hash = _password1,
                Id = Guid.Empty,
            };
            _user2.Hash = hasher.HashPassword(_user2, _user2.Hash);

            Assert.True(_engine.TryCreate(ref _user2));
            Assert.True(_engine.TryCreate(ref _user1));
            Assert.True(_engine.TryCreate(ref _instance));
            Assert.True(_engine.TryCreate(ref _project));
            Assert.True(_engine.TryCreate(ref _network1));

            // Create relation connections between user, instance, project and network
            CreateTestNetwork();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            _engine.TryDelete<User>(_user1.Id, true);
            _engine.TryDelete<User>(_user2.Id, true);
            _engine.TryDelete<Instance>(_instance.Id, true);
            _engine.TryDelete<Project>(_project.Id, true);
            _engine.TryDelete<Network>(_network1.Id, true);
            _engine.TryDelete<Person>(_person1.Id, true);
            _engine.TryDelete<Person>(_person2.Id, true);
        }

        [Test]
        public async Task CreatePersonWithCorrectPermissionTest_ShouldReturn200()
        {
            // Arrange
            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new StringContent(JsonConvert.SerializeObject(_person1));
            body.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            var response = await _client.PostAsync($"api/Person/{_network1.Id}", body);

            // Assert
            var returnedPerson = JsonConvert.DeserializeObject<Person>(
                await response.Content.ReadAsStringAsync());

            _person1.Id = returnedPerson.Id;

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(returnedPerson.Name, _person1.Name);
        }

        [Test]
        public async Task CreatePersonWithInvalidPermissionTest_ShouldReturn403()
        {
            // Arrange
            // Authenticate
            string token = await AuthenticateTestUser(_user2.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new StringContent(JsonConvert.SerializeObject(_person1));
            body.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            var response = await _client.PostAsync($"api/Person/{_network1.Id}", body);

            // Assert
            var returnedPerson = JsonConvert.DeserializeObject<Person>(
                await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsNull(returnedPerson);
        }

        [Test]
        public async Task UpdatePersonWithCorrectPermissionTest_ShouldReturn200()
        {
            // Arrange
            // Create and connect person to network
            Assert.IsTrue(_engine.TryCreate(ref _person1));
            NetworkContainsRelation rel = new()
            {
                From = _network1.Id,
                To = _person1.Id
            };
            Assert.IsTrue(_engine.TryCreate(ref rel));

            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _person1.Name = "Updated person 1!";

            var body = new StringContent(JsonConvert.SerializeObject(_person1));
            body.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            var response = await _client.PutAsync($"api/Person/{_person1.Id}", body);

            // Assert
            var returnedPerson = JsonConvert.DeserializeObject<Person>(
                await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(returnedPerson.Name, _person1.Name);
        }

        [Test]
        public async Task UpdatePersonWithInvalidPermissionTest_ShouldReturn403()
        {
            // Arrange
            // Create and connect person to network
            Assert.IsTrue(_engine.TryCreate(ref _person1));
            NetworkContainsRelation rel = new()
            {
                From = _network1.Id,
                To = _person1.Id
            };
            Assert.IsTrue(_engine.TryCreate(ref rel));

            // Authenticate
            string token = await AuthenticateTestUser(_user2.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _person1.Name = "Updated person 1!";

            var body = new StringContent(JsonConvert.SerializeObject(_person1));
            body.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            var response = await _client.PutAsync($"api/Person/{_person1.Id}", body);

            // Assert
            var returnedPerson = JsonConvert.DeserializeObject<Person>(
                await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsNull(returnedPerson);
        }

        [Test]
        public async Task DeletePersonWithCorrectPermissionTest_ShouldReturn200()
        {
            // Arrange
            // Create and connect person to network
            Assert.IsTrue(_engine.TryCreate(ref _person1));
            NetworkContainsRelation rel = new()
            {
                From = _network1.Id,
                To = _person1.Id
            };
            Assert.IsTrue(_engine.TryCreate(ref rel));

            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync($"api/Person/{_person1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(_engine.Get<Person>(_person1.Id));
        }

        [Test]
        public async Task DeletePersonWithInvalidPermissionTest_ShouldReturn403()
        {
            // Arrange
            // Create and connect person to network
            Assert.IsTrue(_engine.TryCreate(ref _person1));
            NetworkContainsRelation rel = new()
            {
                From = _network1.Id,
                To = _person1.Id
            };
            Assert.IsTrue(_engine.TryCreate(ref rel));

            // Authenticate
            string token = await AuthenticateTestUser(_user2.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync($"api/Person/{_person1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsNotNull(_engine.Get<Person>(_person1.Id));
        }

        private void CreateTestNetwork()
        {
            // Connect user to instance
            InstanceRoleRelation userInstanceRole = new()
            {
                From = _user1.Id,
                To = _instance.Id,
                Authority = EntityAuthority.Owner
            };
            _engine.TryCreate(ref userInstanceRole);

            // Connect project to instance
            InstanceContainsRelation instanceContains = new()
            {
                From = _instance.Id,
                To = _project.Id,
            };
            _engine.TryCreate(ref instanceContains);

            // Connect network to project
            ProjectContainsRelation projectContains = new()
            {
                From = _project.Id,
                To = _network1.Id,
            };
            _engine.TryCreate(ref projectContains);

            // Connect user2 with view permissions
            ProjectRoleRelation projectRoleRelation = new()
            {
                From = _user2.Id,
                To = _project.Id,
                Authority = EntityAuthority.View
            };
            _engine.TryCreate(ref projectRoleRelation);
        }
    }
}