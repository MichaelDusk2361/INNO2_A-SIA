using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.Test.IntegrationTests.API.Controller
{
    public class NetworkStructureControllerTest : ApiIntegrationTestInitializer
    {
        private Network _network1;
        private Project _project;
        private Instance _instance;
        private User _user1;

        // Network structure to test
        private NetworkStructure _networkStructure;

        private string _password1 = "test1";

        private Neo4JEngine _engine;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Neo4JDatabase db = new();
            _engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);
            var hasher = new PasswordHasher<User>();

            _networkStructure = new()
            {
                Groups = new(),
                InfluenceRelations = new(),
                People = new(),
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

            Assert.True(_engine.TryCreate(ref _user1));
            Assert.True(_engine.TryCreate(ref _instance));
            Assert.True(_engine.TryCreate(ref _project));
            Assert.True(_engine.TryCreate(ref _network1));
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            _engine.TryDelete<User>(_user1.Id, true);
            _engine.TryDelete<Instance>(_instance.Id, true);
            _engine.TryDelete<Project>(_project.Id, true);
            _engine.TryDelete<Network>(_network1.Id, true);
        }

        [Test]
        public async Task GetNetworkStructureTest()
        {
            // Arrange
            CreateTestNetwork();

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/NetworkStructure/{_network1.Id}");

            // Assert
            var returnedStructure = JsonConvert.DeserializeObject<NetworkStructure>(
                await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(_networkStructure.People.Count,
                returnedStructure.People.Count);
            Assert.AreEqual(_networkStructure.InfluenceRelations.Count,
                returnedStructure.InfluenceRelations.Count);
            Assert.AreEqual(_networkStructure.Groups.Count,
                returnedStructure.Groups.Count);
        }

        [Test]
        public async Task UpdateNetworkStructureTest()
        {
            // Arrange
            CreateTestNetwork();

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            Person person = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person !!!"
            };

            _networkStructure.People.Add(person);

            var response = await _client.PutAsync(
                $"api/NetworkStructure/{_network1.Id}",
                new StringContent(JsonConvert.SerializeObject(_networkStructure), Encoding.UTF8, "application/json"));

            // Assert
            var returnedNetworkStructure = JsonConvert.DeserializeObject<NetworkStructure>(
                await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(returnedNetworkStructure);
            Assert.AreEqual(_networkStructure.People.Count, returnedNetworkStructure.People.Count);
            Assert.NotNull(returnedNetworkStructure.People.Where(p => p.Id == person.Id).FirstOrDefault());
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

            // Create people
            for (int i = 0; i < 10; i++)
            {
                Person person = new()
                {
                    Name = $"Person {i}",
                    Description = $"This is person {i}",
                };
                Assert.IsTrue(_engine.TryCreate(ref person));

                // Add person to network
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = person.Id,
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));

                _networkStructure.People.Add(person);
            }

            // Create relationships
            for (int i = 0; i < 9; i++)
            {
                InfluencesRelation rel = new()
                {
                    From = _networkStructure.People[i].Id,
                    To = _networkStructure.People[i + 1].Id
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));

                _networkStructure.InfluenceRelations.Add(rel);
            }

            // Create a small group
            Group smallGroup = new()
            {
                Name = "Small group"
            };
            Assert.IsTrue(_engine.TryCreate(ref smallGroup));

            // Add small group to network
            {
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = smallGroup.Id,
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            GroupEntry smallGroupEntry = new()
            {
                Group = smallGroup,
                Nodes = new()
                {
                    _networkStructure.People[0].Id,
                    _networkStructure.People[1].Id,
                }
            };
            ConnectGroupEntries(smallGroupEntry);
            _networkStructure.Groups.Add(smallGroupEntry);

            // Create a group that contains the small group
            Group bigGroup = new()
            {
                Name = "Big group"
            };
            Assert.IsTrue(_engine.TryCreate(ref bigGroup));
            // Add big group to network
            {
                NetworkContainsRelation rel = new()
                {
                    From = _network1.Id,
                    To = bigGroup.Id,
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            GroupEntry bigGroupEntry = new()
            {
                Group = bigGroup,
                Nodes = new()
                {
                    _networkStructure.People[5].Id,
                    _networkStructure.People[6].Id,
                    _networkStructure.People[7].Id,
                    _networkStructure.People[8].Id,
                    smallGroupEntry.Group.Id,
                }
            };
            ConnectGroupEntries(bigGroupEntry);
            _networkStructure.Groups.Add(bigGroupEntry);
        }

        private void ConnectGroupEntries(GroupEntry entry)
        {
            if (entry == null || entry.Group == null || entry.Nodes == null)
                Assert.Fail();

            for (int i = 0; i < entry.Nodes.Count; i++)
            {
                var n = entry.Nodes[i];
                GroupContainsRelation rel = new()
                {
                    From = entry.Group.Id,
                    To = n
                };

                Assert.IsTrue(_engine.TryCreate(ref rel));
            }
        }
    }
}
