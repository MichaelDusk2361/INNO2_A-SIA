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
    public class ProjectControllerTest : ApiIntegrationTestInitializer
    {
        private Project _project1;
        private Project _project2;
        private Instance _instance1;
        private Instance _instance2;
        private User _user1;
        private User _user2;
        private string _password1 = "test1";
        private string _password2 = "test2";

        private Neo4JEngine _engine;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Neo4JDatabase db = new();
            _engine = new(db, new Mock<ILogger<Neo4JEngine>>().Object);
            var hasher = new PasswordHasher<User>();

            _instance1 = new()
            {
                Name = "Instance 1"
            };
            _instance2 = new()
            {
                Name = "Instance 2"
            };
            _project1 = new()
            {
                Name = "Project 1"
            };
            _project2 = new()
            {
                Name = "Project 2"
            };
            _user1 = new()
            {
                Email = $"{Guid.NewGuid()}@test.test",
                FirstName = "Test1FirstName",
                LastName = "Test1LastName",
                Hash = _password1,
                Id = Guid.Empty,
            };
            _user1.Hash = hasher.HashPassword(_user1, _user1.Hash);

            _user2 = new()
            {
                Email = $"{Guid.NewGuid()}@test.test",
                FirstName = "Test2FirstName",
                LastName = "Test2LastName",
                Hash = _password2,
                Id = Guid.Empty,
            };
            _user2.Hash = hasher.HashPassword(_user2, _user2.Hash);

            Assert.True(_engine.TryCreate(ref _user1));
            Assert.True(_engine.TryCreate(ref _user2));
            Assert.True(_engine.TryCreate(ref _instance1));
            Assert.True(_engine.TryCreate(ref _instance2));
            Assert.True(_engine.TryCreate(ref _project1));
            Assert.True(_engine.TryCreate(ref _project2));
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            _engine.TryDelete<User>(_user1.Id, true);
            _engine.TryDelete<User>(_user2.Id, true);
            _engine.TryDelete<Instance>(_instance1.Id, true);
            _engine.TryDelete<Instance>(_instance2.Id, true);
            _engine.TryDelete<Project>(_project1.Id, true);
            _engine.TryDelete<Project>(_project2.Id, true);
        }

        #region GET

        /// <summary>
        /// Tests that the user cannot view projects without auth token
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetProjectWithoutTokenShouldReturn401()
        {
            // Act
            var response = await _client.GetAsync(
                "api/project/0");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Tests if the user can view his/her projects with the correct token
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetProjectWithCorrectUserShouldReturn200AndProject()
        {
            // Arrange
            // Link user to project
            var rel1 = new ProjectRoleRelation()
            {
                From = _user1.Id,
                To = _project1.Id,
                Authority = EntityAuthority.Owner
            };
            Assert.True(_engine.TryCreate(ref rel1));

            // Get login token
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/project/{_project1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(
                _project1,
                JsonConvert.DeserializeObject<Project>(await response.Content.ReadAsStringAsync()), "Returned project does not match project in database!");
        }

        /// <summary>
        /// Tests if a user cannot view antoher users projects
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetProjectWithOtherUserShouldReturn401()
        {
            string token = await AuthenticateTestUser(_user2.Email, _password2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/project/{_project1.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Tests that the user can get all projects of an instance that he/her is assigned to
        /// </summary>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.Owner)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.View)]
        public async Task GetProjectsFromInstanceShouldReturn200AndProjects(EntityAuthority instanceRole)
        {
            // Arrange
            // Setup links between user and instance
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance1.Id,
                    Authority = instanceRole
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Setup instance link to projects
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance1.Id,
                    To = _project1.Id
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance1.Id,
                    To = _project2.Id
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/instance/{_instance1.Id}/projects");
            var projects = JsonConvert.DeserializeObject<Project[]>(await response.Content.ReadAsStringAsync());
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(_project1, projects.Where(p => p.Id == _project1.Id).FirstOrDefault());
            Assert.AreEqual(_project2, projects.Where(p => p.Id == _project2.Id).FirstOrDefault());
        }

        /// <summary>
        /// Tests the ability to get all projects that are in/directly related to the user
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.Owner)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.View)]
        public async Task GetProjectsFromUserShouldReturn200AndProjects(EntityAuthority role)
        {
            // Arrange
            // Setup links between user and instance
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance1.Id,
                    Authority = role
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Setup instance link to project 1
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance1.Id,
                    To = _project1.Id
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Link user directly to project 2
            {
                var rel = new ProjectRoleRelation()
                {
                    From = _user1.Id,
                    To = _project2.Id,
                    Authority = role
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"api/user/projects");
            var projects = JsonConvert.DeserializeObject<Project[]>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(_project1, projects.Where(p => p.Id == _project1.Id).FirstOrDefault());
            Assert.AreEqual(_project2, projects.Where(p => p.Id == _project2.Id).FirstOrDefault());
        }

        /// <summary>
        /// Tests the ability to get the authority of a user to a project
        /// </summary>
        /// <param name="authority"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.Owner)]
        [TestCase(EntityAuthority.Edit)]
        [TestCase(EntityAuthority.View)]
        public async Task GetAuthorityFromProjectShouldReturnHighestAuthority(EntityAuthority authority)
        {
            // Arrange
            // Setup links between user and instance
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance1.Id,
                    Authority = EntityAuthority.Edit
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Setup instance link to project 1
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance1.Id,
                    To = _project1.Id
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Link user directly to project 2 and instance
            {
                var rel = new ProjectRoleRelation()
                {
                    From = _user1.Id,
                    To = _project2.Id,
                    Authority = authority
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance1.Id,
                    To = _project2.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act on project 1
            var response1 = await _client.GetAsync(
                $"api/project/{_project1.Id}/authority");
            var content1 = await response1.Content.ReadAsStringAsync();
            var authority1 = JsonConvert.DeserializeObject<GetAuthorityResponse>(
                content1).Authority;

            // Act on project 2
            var response2 = await _client.GetAsync(
                $"api/project/{_project2.Id}/authority");
            var content2 = await response2.Content.ReadAsStringAsync();
            var authority2 = JsonConvert.DeserializeObject<GetAuthorityResponse>(
                content2).Authority;

            // Assert both
            Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
            Assert.IsTrue(authority1 < EntityAuthority.View);
            Assert.IsTrue(authority2 < EntityAuthority.View);
            if(authority != EntityAuthority.View)
            {
                Assert.AreEqual(authority, authority2);
            }
        }

        private class GetAuthorityResponse
        {
            public EntityAuthority Authority { get; set; }
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Tests that the user cannot delete a project without an auth token
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DeleteProjectWithoutTokenShouldReturn401()
        {
            // Act
            var response = await _client.DeleteAsync("api/project/0");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Tests that the user can delete a project if he/she has
        /// the correct role assigned to the project
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        public async Task DeleteSingleProjectWithUser(EntityAuthority role, HttpStatusCode expected)
        {
            // Arrange
            // Link user to project
            {
                var rel = new ProjectRoleRelation()
                {
                    From = _user1.Id,
                    To = _project1.Id,
                    Authority = role
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/project/{_project1.Id}");

            // Assert
            Assert.AreEqual(expected, response.StatusCode);
        }

        /// <summary>
        /// Tests that the user can delete projects that are linked to
        /// a instance that the user is connected to
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [Test]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.Forbidden)]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        public async Task DeleteProjectWithIndirectRoleLinkThroughInstance(EntityAuthority role, HttpStatusCode expected)
        {
            // Arrange
            // Setup links between user and instance
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance1.Id,
                    Authority = role
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            // Setup instance link to project 1
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance1.Id,
                    To = _project1.Id
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync(
                $"api/project/{_project1.Id}");

            // Assert
            Assert.AreEqual(expected, response.StatusCode);

        }

        #endregion

        #region POST

        class ProjectContent : HttpContent
        {
            private Project _project;

            public ProjectContent(Project project)
            {
                _project = project;
                Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                using var sw = new StreamWriter(stream);
                return sw.WriteAsync(JsonConvert.SerializeObject(_project));
            }

            protected override bool TryComputeLength(out long length)
            {
                length = JsonConvert.SerializeObject(_project).Length;
                return true;
            }
        }

        [Test]
        public async Task CreateProjectWithoutTokenShouldReturn401()
        {
            // Arrange
            Project project = new Project()
            {
                Name = "My new cool project!",
            };

            ProjectContent content = new(project);

            // Act
            var response = await _client.PostAsync(
                $"api/instance/{_instance1.Id}/project/",
                content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task CreateProjectOnWrongInstanceShouldReturn403()
        {
            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Project project = new Project()
            {
                Name = "My new cool project!",
            };

            // Act
            var response = await _client.PostAsync(
                $"api/instance/{_instance2.Id}/project",
                new ProjectContent(project));

            var returnString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.AreEqual(0, returnString.Length);
        }

        [Test]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.Created)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.Created)]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        public async Task CreateProjectShouldReturn200(EntityAuthority authority, HttpStatusCode expected)
        {
            // Arrange
            Project project = new()
            {
                Id = Guid.Empty,
                Name = "My new cool project!",
            };
            // Setup instance relation
            {
                InstanceRoleRelation rel = new()
                {
                    From = _user1.Id,
                    To = _instance1.Id,
                    Authority = authority
                };
                Assert.IsTrue(_engine.TryCreate(ref rel));
            }

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsync(
                $"api/instance/{_instance1.Id}/project/",
                new ProjectContent(project));

            // Assert
            Assert.AreEqual(expected, response.StatusCode);
            if(response.StatusCode == HttpStatusCode.Created)
            {
                // Get content / created project
                Project returnedProject = JsonConvert.DeserializeObject<Project>(
                    await response.Content.ReadAsStringAsync());

                Assert.AreEqual(project.Name, returnedProject.Name);
                Assert.AreNotEqual(project.Id, returnedProject.Id);

                _engine.TryDelete<Project>(returnedProject.Id);
            }
        }

        #endregion

        #region PUT

        [Test]
        public async Task UpdateProjectWithoutTokenShouldReturn401()
        {
            // Arrange
            Project project = new()
            {
                Id = Guid.Empty,
                Name = "My updated cool project!",
            };
            ProjectContent content = new(project);

            // Act
            var response = await _client.PutAsync(
                "api/project/0",
                content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.OK)]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        public async Task UpdateProjectWithCorrectUserAndAuth(EntityAuthority authority, HttpStatusCode expected)
        {
            // Arrange
            // Link user directly to project 1
            {
                var rel = new ProjectRoleRelation()
                {
                    From = _user1.Id,
                    To = _project1.Id,
                    Authority = authority
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _project1.Name = "Other new project";

            // Act
            var response = await _client.PutAsync(
                $"api/project/{_project1.Id}",
                new ProjectContent(_project1));

            // Assert
            Assert.AreEqual(expected, response.StatusCode);
            if(expected == HttpStatusCode.OK)
                Assert.AreEqual(_project1.Name, _engine.Get<Project>(_project1.Id)?.Name);
        }

        [Test]
        [TestCase(EntityAuthority.Owner, HttpStatusCode.OK)]
        [TestCase(EntityAuthority.Edit, HttpStatusCode.OK)]
        [TestCase(EntityAuthority.View, HttpStatusCode.Forbidden)]
        public async Task UpdateProjectWithCorrectUserAndIndirectAuth(EntityAuthority authority, HttpStatusCode expected)
        {
            // Arrange
            // Link user to instance
            {
                var rel = new InstanceRoleRelation()
                {
                    From = _user1.Id,
                    To = _instance1.Id,
                    Authority = authority
                };
                Assert.True(_engine.TryCreate(ref rel));
            }
            // Link instance to project
            {
                var rel = new InstanceContainsRelation()
                {
                    From = _instance1.Id,
                    To = _project1.Id,
                };
                Assert.True(_engine.TryCreate(ref rel));
            }

            string token = await AuthenticateTestUser(_user1.Email, _password1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _project1.Name = "Other new project";

            // Act
            var response = await _client.PutAsync(
                $"api/project/{_project1.Id}",
                new ProjectContent(_project1));

            // Assert
            Assert.AreEqual(expected, response.StatusCode);
            if (expected == HttpStatusCode.OK)
                Assert.AreEqual(_project1.Name, _engine.Get<Project>(_project1.Id)?.Name);
        }

        #endregion
    }
}
