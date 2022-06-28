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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.Test.IntegrationTests.API.Controller
{
    public class RelationControllerTest : ApiIntegrationTestInitializer
    {
        private Network _network1;
        private Project _project;
        private Instance _instance;
        private User _user1;
        private User _user2;

        private Person _person1;
        private Person _person2;

        private InfluencesRelation _influencesRelation1;
        private InfluencesRelation _influencesRelation2;

        private string _password1 = "test1";

        private Neo4JEngine _engine;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Neo4JDatabase db = new();
            _engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);
            var hasher = new PasswordHasher<User>();

            _influencesRelation1 = new InfluencesRelation()
            {
                Influence = 1.4f,
                Interval = 1,
                Offset = 0,
            };
            _influencesRelation2 = new InfluencesRelation()
            {
                Influence = -2.4f,
                Interval = 4,
                Offset = 1,
            };

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
                PositionX = 2.241f,
                PositionY = 1.45f,
                Roles = new List<string>() { PersonRoles.CutPoint },
                AvatarPath = "networkId/personId",
                Color = "FFFFFF",
                Description = "This is the second person",
                Persistance = 0.245f,
                Reflection = 0.8521f,
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

            // Add people
            Assert.True(_engine.TryCreate(ref _person1));
            Assert.True(_engine.TryCreate(ref _person2));

            // Connect people to network
            {
                var rel = new NetworkContainsRelation()
                {
                    From = _network1.Id,
                    To = _person1.Id,
                    Id = Guid.NewGuid(),
                };

                Assert.True(_engine.TryCreate(ref rel));
            }
            {
                var rel = new NetworkContainsRelation()
                {
                    From = _network1.Id,
                    To = _person2.Id,
                    Id = Guid.NewGuid(),
                };

                Assert.True(_engine.TryCreate(ref rel));
            }
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

        // Controller CRUD Operations

        [Test]
        public async Task CreateInfluencesRelationWithCorrectPermission_ShouldReturn201()
        {
            // Arrange
            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(_influencesRelation1));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PostAsync($"api/Relation/{_person1.Id}/Influences/{_person2.Id}", content);
            var inserted = JsonConvert.DeserializeObject<InfluencesRelation>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // Check that relation was inserted
            Assert.IsNotNull(inserted);
            // Check that relation properties were set correctly
            _influencesRelation1.Influence = inserted.Influence;
            // Check that relation was connected to the network
            var relation = _engine.GetRelations<InfluencesRelation>(_person1.Id, _person2.Id).FirstOrDefault();
            Assert.IsNotNull(relation);
        }

        [Test]
        public async Task CreateInfluencesRelationWithInvalidPermission_ShouldReturn403()
        {
            // Arrange
            // Authenticate
            string token = await AuthenticateTestUser(_user2.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(_influencesRelation1));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PostAsync($"api/Relation/{_person1.Id}/Influences/{_person2.Id}", content);
            var inserted = JsonConvert.DeserializeObject<InfluencesRelation>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsNull(inserted);
        }

        [Test]
        public async Task UpdateInfluencesRelationWithCorrectPermission_ShouldReturn200()
        {
            // Arrange
            // Create relation
            _influencesRelation1.From = _person1.Id;
            _influencesRelation1.To = _person2.Id;
            Assert.IsTrue(_engine.TryCreate(ref _influencesRelation1));

            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Change relation properties
            _influencesRelation1.Influence = 0.01425f;

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(_influencesRelation1));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PutAsync($"api/Relation/Influences/{_influencesRelation1.Id}", content);
            var inserted = JsonConvert.DeserializeObject<InfluencesRelation>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Check that group was updated
            Assert.IsNotNull(inserted);
            // Check that relation properties were set correctly
            Assert.AreEqual(JsonConvert.SerializeObject(_influencesRelation1),
                JsonConvert.SerializeObject(inserted));

            var relation = _engine.GetRelations<InfluencesRelation>(_person1.Id, _person2.Id).FirstOrDefault();
            Assert.IsNotNull(relation);
            Assert.AreEqual(_influencesRelation1.Influence, relation.Influence);
        }

        [Test]
        public async Task UpdateInfluencesRelationWithInvalidPermission_ShouldReturn403()
        {
            // Arrange
            // Create relation
            _influencesRelation1.From = _person1.Id;
            _influencesRelation1.To = _person2.Id;
            Assert.IsTrue(_engine.TryCreate(ref _influencesRelation1));

            // Authenticate
            string token = await AuthenticateTestUser(_user2.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(_influencesRelation1));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PutAsync($"api/Relation/Influences/{_influencesRelation1.Id}", content);
            var inserted = JsonConvert.DeserializeObject<InfluencesRelation>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsNull(inserted);
        }

        [Test]
        public async Task DeleteInfluencesRelationCorrectPermission_ShouldReturn200()
        {
            // Arrange
            // Create relation
            _influencesRelation1.From = _person1.Id;
            _influencesRelation1.To = _person2.Id;
            Assert.IsTrue(_engine.TryCreate(ref _influencesRelation1));

            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync($"api/Relation/Influences/{_influencesRelation1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(_engine.Get<InfluencesRelation>(_influencesRelation1.Id));
        }

        [Test]
        public async Task DeleteGroupWithInvalidPermission_ShouldReturn403()
        {
            // Arrange
            // Create relation
            _influencesRelation1.From = _person1.Id;
            _influencesRelation1.To = _person2.Id;
            Assert.IsTrue(_engine.TryCreate(ref _influencesRelation1));

            // Authenticate
            string token = await AuthenticateTestUser(_user2.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync($"api/Relation/Influences/{_influencesRelation1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsNotNull(_engine.Get<InfluencesRelation>(_influencesRelation1.Id));
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