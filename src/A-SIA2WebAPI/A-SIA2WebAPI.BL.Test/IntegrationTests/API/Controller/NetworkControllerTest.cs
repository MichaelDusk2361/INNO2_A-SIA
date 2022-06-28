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
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.Test.IntegrationTests.API.Controller
{
    public class NetworkControllerTest : ApiIntegrationTestInitializer
    {
        private Network _network1;
        private Network _network2;
        private Project _project;
        private Instance _instance;
        private User _user1;
        private string _password1 = "test1";

        private Neo4JEngine _engine;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Neo4JDatabase db = new();
            _engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);
            var hasher = new PasswordHasher<User>();

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
            _network2 = new()
            {
                Name = "Network 2"
            };
            _user1 = new()
            {
                Email = $"{Guid.NewGuid()}",
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
            Assert.True(_engine.TryCreate(ref _network2));
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            _engine.TryDelete<User>(_user1.Id, true);
            _engine.TryDelete<Instance>(_instance.Id, true);
            _engine.TryDelete<Project>(_project.Id, true);
            _engine.TryDelete<Network>(_network1.Id, true);
            _engine.TryDelete<Network>(_network2.Id, true);
        }

        #region GET

        /// <summary>
        /// Checks that an unauthorized user cannot use the GET network action
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetSingleNetworkWithoutUserShouldReturn401()
        {
            // Act
            var response = await _client.GetAsync(
                $"api/network/{_network1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Checks that an authorized user cannot GET networks that are not connected
        /// to the user
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetSingleNetworkWithWrongUserShouldReturn403()
        {
            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/network/{_network2.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Checks that a user that is directly connected to a network can view it with
        /// every authority type
        /// </summary>
        /// <param name="authority"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.Owner)]
        public async Task GetSingleNetworkWithDirectRightsUserShouldReturn200(EntityAuthority authority)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network to project
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            {
                // Link user to network
                var rel = new NetworkRoleRelation()
                {
                    From = _user1.Id,
                    To = _network1.Id,
                    Authority = authority
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/network/{_network1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(
                _network1,
                JsonConvert.DeserializeObject<Network>(await response.Content.ReadAsStringAsync()),
                "Returned network does not match network in database!");
        }

        /// <summary>
        /// Checks that a user can view a network with every authority type
        /// if the user is connected to the instance that holds the project
        /// where the network is located
        /// </summary>
        /// <param name="authority"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.Owner)]
        public async Task GetSingleNetworkWithInstanceRightsUserShouldReturn200(EntityAuthority authority)
        {
            // Arrange
            // Link user to instance
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance.Id,
                    Authority = authority
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network to project
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/network/{_network1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(
                _network1,
                JsonConvert.DeserializeObject<Network>(await response.Content.ReadAsStringAsync()),
                "Returned network does not match network in database!");
        }

        /// <summary>
        /// Checks if the user can view a network if the user is connected to the
        /// networks overlaying project with every authority type
        /// </summary>
        /// <param name="authority"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.Owner)]
        public async Task GetSingleNetworkWithProjectRightsUserShouldReturn200(EntityAuthority authority)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network to project
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link user to project
            {
                var rel = new ProjectRoleRelation()
                {
                    From = _user1.Id,
                    To = _project.Id,
                    Authority = authority
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/network/{_network1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(
                _network1,
                JsonConvert.DeserializeObject<Network>(await response.Content.ReadAsStringAsync()),
                "Returned network does not match network in database!");
        }

        [Test]
        public async Task GetAllNetworksFromProjectWithUserNotAuthenticated()
        {
            // Act
            var response = await _client.GetAsync(
                $"api/project/0/networks");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Checks if the user gets zero networks if there are no networks attached to the user
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetAllNetworksFromProjectWithNoProjectAttachedToUser()
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 1
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 2
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network2.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/project/{_project.Id}/networks");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("[]", await response.Content.ReadAsStringAsync());
        }

        [Test]
        [TestCase(EntityAuthority.View)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.Owner)]
        public async Task GetAllNetworksFromProjectWithOneProjectAttachedToUser(EntityAuthority authority)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 1
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 2
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network2.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network1 to user
            {
                var rel = new NetworkRoleRelation()
                {
                    From = _user1.Id,
                    To = _network1.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/project/{_project.Id}/networks");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(
                new Network[] { _network1 },
                JsonConvert.DeserializeObject<Network[]>(await response.Content.ReadAsStringAsync()));
        }

        [Test]
        [TestCase(EntityAuthority.View)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.Owner)]
        public async Task GetAllNetworksFromProjectWithTwoProjectsAttachedToUser(EntityAuthority authority)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 1
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 2
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network2.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network1 to user
            {
                var rel = new NetworkRoleRelation()
                {
                    From = _user1.Id,
                    To = _network1.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network2 to user
            {
                var rel = new NetworkRoleRelation()
                {
                    From = _user1.Id,
                    To = _network2.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/project/{_project.Id}/networks");

            var networks = JsonConvert.DeserializeObject<Network[]>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(_network1, networks.Where(n => n.Id == _network1.Id).FirstOrDefault());
            Assert.AreEqual(_network2, networks.Where(n => n.Id == _network2.Id).FirstOrDefault());
        }
        
        [Test]
        [TestCase(EntityAuthority.View)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.Owner)]
        public async Task GetAllNetworksFromProjectWithUserConnectedToProject(EntityAuthority authority)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 1
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 2
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network2.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to user
            {
                var rel = new ProjectRoleRelation()
                {
                    From = _user1.Id,
                    To = _project.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/project/{_project.Id}/networks");

            var networks = JsonConvert.DeserializeObject<Network[]>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(_network1, networks.Where(n => n.Id == _network1.Id).FirstOrDefault());
            Assert.AreEqual(_network2, networks.Where(n => n.Id == _network2.Id).FirstOrDefault());
        }

        [Test]
        [TestCase(EntityAuthority.View)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.Owner)]
        public async Task GetAllNetworksFromProjectWithUserConnectedToInstance(EntityAuthority authority)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 1
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 2
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network2.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to user
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/project/{_project.Id}/networks");

            var networks = JsonConvert.DeserializeObject<Network[]>(
                await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(_network1, networks.Where(n => n.Id == _network1.Id).FirstOrDefault());
            Assert.AreEqual(_network2, networks.Where(n => n.Id == _network2.Id).FirstOrDefault());
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Tests that a user cannot perform the delete network action
        /// without being authorized
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DeleteNetworkWithUnauthenticatedUser()
        {
            // Act
            var response = await _client.DeleteAsync(
                "api/network/0");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Tests that a user cannot delete a network if not connected to the
        /// network, project or instance
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DeleteWithUserThatIsNotConnectedToNetwork()
        {
            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/network/{_network1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Delete with user that is directly attached to network
        /// </summary>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        public async Task DeleteWithUserThatIsConnectedToNetwork(EntityAuthority authority, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 1
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link user to network
            {
                var rel = new NetworkRoleRelation()
                {
                    From = _user1.Id,
                    To = _network1.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/network/{_network1.Id}");

            // Assert
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
        }

        /// <summary>
        /// Delete with user that is attached to network thorugh project
        /// </summary>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        public async Task DeleteWithUserThatIsConnectedToNetworkThroughProject(EntityAuthority authority, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 1
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link user to network
            {
                var rel = new ProjectRoleRelation()
                {
                    From = _user1.Id,
                    To = _project.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/network/{_network1.Id}");

            // Assert
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
        }

        /// <summary>
        /// Delete with user that is attached to network thorugh project
        /// </summary>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        public async Task DeleteWithUserThatIsConnectedToNetworkThroughInstance(EntityAuthority authority, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link project to network 1
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link user to network
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/network/{_network1.Id}");

            // Assert
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
        }

        // TODO : Test Get Projects From User Action In UserController

        #endregion

        #region POST

        class NetworkContent : HttpContent
        {
            private Network _network;

            public NetworkContent(Network network)
            {
                _network = network;
                Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                using var sw = new StreamWriter(stream);
                return sw.WriteAsync(JsonConvert.SerializeObject(_network));
            }

            protected override bool TryComputeLength(out long length)
            {
                length = JsonConvert.SerializeObject(_network).Length;
                return true;
            }
        }

        /// <summary>
        /// Tests that a user cannot perform the post network action
        /// without being authorized
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task PostNetworkWithUnauthenticatedUser()
        {
            // Act
            var response = await _client.PostAsync(
                $"api/project/{_project.Id}/network",
                new NetworkContent(new Network()));

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Tests that a user cannot post on a project without being connected to it
        /// through direct or indirect (instance) connection
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task PostNetworkWithUserNotAttachedToProject()
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsync(
                $"api/project/{_project.Id}/network",
                new NetworkContent(new Network()));

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Tests that a user with direct (project) ownership or edit rights
        /// can create new networks in the projects
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="expectedStatusCode"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.Created)]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.Created)]
        public async Task PostNetworkWithUserDirectlyAttachedToProject(EntityAuthority authority, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link user to project
            {
                var rel = new ProjectRoleRelation()
                {
                    From = _user1.Id,
                    To = _project.Id,
                    Authority = authority
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var networkToInsert = new Network()
            {
                Name = "New Inserted Network!"
            };
            var response = await _client.PostAsync(
                $"api/project/{_project.Id}/network",
                new NetworkContent(networkToInsert));

            // Partial assert
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            // Rest assert only on success
            if(expectedStatusCode == HttpStatusCode.Created)
            {
                // Parse inserted network from response
                var insertedNetwork = JsonConvert.DeserializeObject<Network>(
                    await response.Content.ReadAsStringAsync());
                networkToInsert.Id = insertedNetwork.Id;

                Assert.AreEqual(networkToInsert, insertedNetwork);
                Assert.AreEqual(networkToInsert, _engine.Get<Network>(insertedNetwork.Id));
            }
            
            _engine.TryDelete<Network>(networkToInsert.Id);
        }

        /// <summary>
        /// Tests that a user with indirect (instance) ownership or edit rights
        /// can create new networks in the projects
        /// </summary>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.Created)]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.Created)]
        public async Task PostNetworkWithUserIndirectlyAttachedToProject(EntityAuthority authority, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link user to project
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance.Id,
                    Authority = authority
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var networkToInsert = new Network()
            {
                Name = "New Inserted Network!"
            };
            var response = await _client.PostAsync(
                $"api/project/{_project.Id}/network",
                new NetworkContent(networkToInsert));

            // Partial assert
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            // Rest assert only on success
            if (expectedStatusCode == HttpStatusCode.Created)
            {
                // Parse inserted network from response
                var insertedNetwork = JsonConvert.DeserializeObject<Network>(
                    await response.Content.ReadAsStringAsync());
                networkToInsert.Id = insertedNetwork.Id;

                Assert.AreEqual(networkToInsert, insertedNetwork);
                Assert.AreEqual(networkToInsert, _engine.Get<Network>(insertedNetwork.Id));
            }

            _engine.TryDelete<Network>(networkToInsert.Id);
        }

        #endregion

        #region PUT

        /// <summary>
        /// Tests that a user cannot perform the update network action
        /// without being authorized
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task UpdateNetworkWithUnauthenticatedUser()
        {
            // Act
            var response = await _client.PutAsync(
                $"api/network/{_network1.Id}",
                new NetworkContent(new Network()));

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Tests that a user cannot update a network without being connected to
        /// the network, project or instance
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task UpdateNetworkWithUserThatIsNotAttached()
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network to project
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PutAsync(
                $"api/network/{_network1.Id}",
                new NetworkContent(new Network()));

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Tests that a user can update the network if being directly
        /// attached to the network
        /// </summary>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.OK)]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        public async Task UpdateNetworkWithUserThatIsDirectlyAttached(EntityAuthority authority, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network to project
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link user to network
            {
                var rel = new NetworkRoleRelation()
                {
                    From = _user1.Id,
                    To = _network1.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            _network1.Description = "New Description";
            var response = await _client.PutAsync(
                $"api/network/{_network1.Id}",
                new NetworkContent(_network1));

            // Assert
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            if(expectedStatusCode == HttpStatusCode.OK)
            {
                // Check if network was also updated in database
                var updatedNetwork = _engine.Get<Network>(_network1.Id);

                Assert.AreEqual(_network1, updatedNetwork);
            }
        }

        /// <summary>
        /// Tests that a user can update the network if being attached
        /// attached to the network through the project
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="expectedStatusCode"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.OK)]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        public async Task UpdateNetworkWithUserThatIsAttachedToProject(EntityAuthority authority, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network to project
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link user to network
            {
                var rel = new ProjectRoleRelation()
                {
                    From = _user1.Id,
                    To = _project.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            _network1.Description = "New Description";
            var response = await _client.PutAsync(
                $"api/network/{_network1.Id}",
                new NetworkContent(_network1));

            // Assert
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            if (expectedStatusCode == HttpStatusCode.OK)
            {
                // Check if network was also updated in database
                var updatedNetwork = _engine.Get<Network>(_network1.Id);

                Assert.AreEqual(_network1, updatedNetwork);
            }
        }

        /// <summary>
        /// Tests that a user can update the network if being attached
        /// attached to the network through the instance
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="expectedStatusCode"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.OK)]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        public async Task UpdateNetworkWithUserThatIsAttachedToInstance(EntityAuthority authority, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            // Link project to instance
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance.Id,
                    To = _project.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link network to project
            {
                var rel = new ProjectContainsRelation()
                {
                    From = _project.Id,
                    To = _network1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link user to network
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance.Id,
                    Authority = authority,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            _network1.Description = "New Description";
            var response = await _client.PutAsync(
                $"api/network/{_network1.Id}",
                new NetworkContent(_network1));

            // Assert
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            if (expectedStatusCode == HttpStatusCode.OK)
            {
                // Check if network was also updated in database
                var updatedNetwork = _engine.Get<Network>(_network1.Id);

                Assert.AreEqual(_network1, updatedNetwork);
            }
        }

        #endregion
    }
}
