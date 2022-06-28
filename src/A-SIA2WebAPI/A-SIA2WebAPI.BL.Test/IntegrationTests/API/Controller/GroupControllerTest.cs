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
    public class GroupControllerTest : ApiIntegrationTestInitializer
    {
        private Network _network1;
        private Project _project;
        private Instance _instance;
        private User _user1;
        private User _user2;

        private Person _person1;
        private Group _group1;
        private Group _group2;

        private string _password1 = "test1";

        private Neo4JEngine _engine;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Neo4JDatabase db = new();
            _engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);
            var hasher = new PasswordHasher<User>();

            _group1 = new()
            {
                Name = "Group 1",
                PositionX = -2.241f,
                PositionY = 5.45f,
                Color = "AC42E1",
                Description = "This is the first group!",
                SimulationValues = new()
            };
            _group2 = new()
            {
                Name = "Group 2",
                PositionX = -2.241f,
                PositionY = 5.45f,
                Color = "AC42E1",
                Description = "This is the second group!",
                SimulationValues = new()
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
            _engine.TryDelete<Group>(_group1.Id, true);
        }

        // Controller CRUD Operations

        [Test]
        public async Task CreateGroupWithCorrectPermission_ShouldReturn201()
        {
            // Arrange
            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(_group1));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PostAsync($"api/Group/{_network1.Id}", content);
            var inserted = JsonConvert.DeserializeObject<Group>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // Check that group was inserted
            Assert.IsNotNull(inserted);
            // Check that group properties were set correctly
            _group1.Id = inserted.Id;
            Assert.AreEqual(JsonConvert.SerializeObject(_group1),
                JsonConvert.SerializeObject(inserted));
            // Check that group was connected to the network
            var relation = _engine.GetRelations<NetworkContainsRelation>(_network1.Id, _group1.Id).FirstOrDefault();
            Assert.IsNotNull(relation);
        }

        [Test]
        public async Task CreateGroupWithInvalidPermission_ShouldReturn403()
        {
            // Arrange
            // Authenticate
            string token = await AuthenticateTestUser(_user2.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(_group1));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PostAsync($"api/Group/{_network1.Id}", content);
            var inserted = JsonConvert.DeserializeObject<Group>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsNull(inserted);
        }

        [Test]
        public async Task UpdateGroupWithCorrectPermission_ShouldReturn200()
        {
            // Arrange
            // Create and connect group to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _group1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _group1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Change group properties
            _group1.Persistance = 0.01425f;
            _group1.Name = "Another group!";

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(_group1));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PutAsync($"api/Group/{_group1.Id}", content);
            var inserted = JsonConvert.DeserializeObject<Group>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Check that group was updated
            Assert.IsNotNull(inserted);
            // Check that group properties were set correctly
            _group1.Id = inserted.Id;
            Assert.AreEqual(JsonConvert.SerializeObject(_group1),
                JsonConvert.SerializeObject(inserted));
            // Check that group was connected to the network
            var relation = _engine.GetRelations<NetworkContainsRelation>(_network1.Id, _group1.Id).FirstOrDefault();
            Assert.IsNotNull(relation);
        }

        [Test]
        public async Task UpdateGroupWithInvalidPermission_ShouldReturn403()
        {
            // Arrange
            // Create and connect group to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _group1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _group1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            // Authenticate
            string token = await AuthenticateTestUser(_user2.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent(JsonConvert.SerializeObject(_group1));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PutAsync($"api/Group/{_group1.Id}", content);
            var inserted = JsonConvert.DeserializeObject<Group>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsNull(inserted);
        }
        
        [Test]
        public async Task DeleteGroupWithCorrectPermission_ShouldReturn200()
        {
            // Arrange
            // Create and connect group to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _group1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _group1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync($"api/Group/{_group1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(_engine.Get<Group>(_group1.Id));
        }

        [Test]
        public async Task DeleteGroupWithInvalidPermission_ShouldReturn403()
        {
            // Arrange
            // Create and connect group to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _group1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _group1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            // Authenticate
            string token = await AuthenticateTestUser(_user2.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync($"api/Group/{_group1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsNotNull(_engine.Get<Group>(_group1.Id));
        }

        // Person Actions

        [Test]
        public async Task AddPersonToGroupWithCorrectPermission_ShouldReturn201()
        {
            // Arrange
            // Create and connect person to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _person1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _person1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }
            // Create and connect group to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _group1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _group1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent("");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PostAsync($"api/Group/{_group1.Id}/{_person1.Id}", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            
            // Get relation between person and group
            var relation = _engine.GetRelations<GroupContainsRelation>(_group1.Id, _person1.Id).FirstOrDefault();
            Assert.NotNull(relation);
        }

        [Test]
        public async Task AddPersonToNonExistentGroup_ShouldReturn400()
        {
            // Arrange
            // Create and connect person to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _person1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _person1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent("");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PostAsync($"api/Group/{Guid.NewGuid()}/{_person1.Id}", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            // Get relation between person and group
            var relation = _engine.GetRelations<GroupContainsRelation>(_group1.Id, _person1.Id).FirstOrDefault();
            Assert.Null(relation);
        }

        [Test]
        public async Task RemovePersonFromGroupWithCorrectPermission_ShouldReturn200()
        {
            // Arrange
            // Create and connect person to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _person1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _person1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }
            // Create and connect group to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _group1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _group1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }
            // Connect person to group
            GroupContainsRelation groupContainsRelation = new()
            {
                From = _group1.Id,
                To = _person1.Id
            };
            Assert.IsTrue(_engine.TryCreate(ref groupContainsRelation));

            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync($"api/Group/{_group1.Id}/{_person1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Get relation between person and group
            var relation = _engine.GetRelations<GroupContainsRelation>(_group1.Id, _person1.Id).FirstOrDefault();
            Assert.IsNull(relation);
            Assert.IsNull(_engine.Get<GroupContainsRelation>(groupContainsRelation.Id));
        }

        [Test]
        public async Task ChangePersonGroupWithCorrectPermission_ShouldReturn201()
        {
            // Arrange
            // Create and connect person to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _person1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _person1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }
            // Create and connect group to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _group1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _group1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }
            // Create and connect group 2 to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _group2));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _group2.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }
            // Connect person to group 1
            GroupContainsRelation group1ContainsRelation = new()
            {
                From = _group1.Id,
                To = _person1.Id
            };
            Assert.IsTrue(_engine.TryCreate(ref group1ContainsRelation));


            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent("");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PatchAsync($"api/Group/{_group2.Id}/{_person1.Id}", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // Get relation between person and group 1 (should be null)
            Assert.IsNull(_engine.GetRelations<GroupContainsRelation>(_group1.Id, _person1.Id).FirstOrDefault());
            Assert.IsNull(_engine.Get<GroupContainsRelation>(group1ContainsRelation.Id));
            // Get relation between person and group 2 (should be not null)
            Assert.IsNotNull(_engine.GetRelations<GroupContainsRelation>(_group2.Id, _person1.Id).FirstOrDefault());
        }

        [Test]
        public async Task ChangePersonToInvalidGroup_ShouldReturn400()
        {
            // Arrange
            // Create and connect person to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _person1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _person1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }
            // Create and connect group to network
            {
                Assert.IsTrue(_engine.TryCreate(ref _group1));
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = _group1.Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }
            // Connect person to group 1
            GroupContainsRelation group1ContainsRelation = new()
            {
                From = _group1.Id,
                To = _person1.Id
            };
            Assert.IsTrue(_engine.TryCreate(ref group1ContainsRelation));


            // Authenticate
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var content = new StringContent("");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _client.PatchAsync($"api/Group/{Guid.NewGuid()}/{_person1.Id}", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
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