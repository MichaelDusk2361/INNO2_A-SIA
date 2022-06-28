using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using A_SIA2WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    /// <summary>
    /// This controller is responsible for Project operations as well as
    /// retrieving the networks that are connected to a project.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProjectController : ExtendedControllerBase
    {
        // Services are retrieved via dependency injection
        private readonly Neo4JProjectRepository _projectRepository;
        private readonly Neo4JRelationRepository _relationRepository;
        private readonly Neo4JInstanceRepository _instanceRepository;
        private readonly Neo4JNetworkRepository _networkRepository;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IRepository<User> userRepository,
            IRepository<Relation> relationRepository,
            IRepository<Project> projectRepository,
            IRepository<Instance> instanceRepository,
            IRepository<Network> networkRepository,
            ILogger<ProjectController> logger) : base(userRepository)
        {
            // Get network repository
            {
                if (networkRepository is not Neo4JNetworkRepository repo)
                    throw new ArgumentException(nameof(repo));
                _networkRepository = repo;
            }
            // Get Project repository
            {
                if (projectRepository is not Neo4JProjectRepository repo)
                    throw new ArgumentException(nameof(repo));
                _projectRepository = repo;
            }
            // Get Relation repository
            {
                if (relationRepository is not Neo4JRelationRepository repo)
                    throw new ArgumentException(nameof(repo));
                _relationRepository = repo;
            }
            // Get Instance repository
            {
                if (instanceRepository is not Neo4JInstanceRepository repo)
                    throw new ArgumentException(nameof(repo));
                _instanceRepository = repo;
            }

            _logger = logger;
        }

        /// <summary>
        /// Gets a project if the logged in user has the view authority of
        /// the instance
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("{projectId}")]
        public IActionResult GetProject([FromRoute] Guid projectId)
        {
            try
            {
                if (LoggedInUser == null)
                    return Unauthorized();

                var result = TryGetProjectFromUser(projectId, LoggedInUser.Id,
                    EntityAuthority.View, out Project? project);

                if (project == null)
                    return Forbid();

                // Return result of get operation
                return result switch
                {
                    StatusCodes.Status403Forbidden => Forbid(),
                    StatusCodes.Status404NotFound => NotFound($"No Project with id {projectId} found!"),
                    _ => Ok(project),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(500, $"Internal error retrieving project!");
        }

        /// <summary>
        /// Gets the authority rights of the user related to the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("{projectId}/Authority")]
        public IActionResult GetProjectAuthority([FromRoute] Guid projectId)
        {
            try
            {
                if (LoggedInUser == null)
                    return Unauthorized();

                var authority = GetAuthorityFromProject(projectId, LoggedInUser.Id);

                if (authority == null)
                    return Forbid();

                // Return authority as json object
                return Ok($"{{\"Authority\":\"{ authority }\"}}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(500, $"Internal error retrieving project!");
        }

        /// <summary>
        /// Helper method<br/>
        /// Tries to retrieve the given project from the user.
        /// Fails if the project does on exists or if the user has
        /// no authority over the project!
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        private int TryGetProjectFromUser(Guid projectId, Guid userId, EntityAuthority leastAuthority, out Project? project)
        {
            List<Project> filteredProjects = new();
            project = null;

            // Get all instances of the user with the minimum given authority
            var instances = _userRepository.GetInstancesOfUser(userId);

            foreach (var instance in instances)
            {
                var rel = _relationRepository.GetRelations<InstanceRoleRelation>(userId, instance.Id)
                    .FirstOrDefault();

                // Check if the instance is in the given authority range
                if (rel != null && rel.Authority <= leastAuthority)
                {
                    // Add all projects of this instance to the project list
                    filteredProjects.AddRange(
                        _instanceRepository.GetProjectsOfInstance(instance.Id));
                }
            }

            // Get all projects of the user
            var projects = _userRepository.GetProjectsOfUser(userId);

            // Filter projects with authority type
            foreach (var item in projects)
            {
                var rel = _relationRepository.GetRelations<ProjectRoleRelation>(userId, projectId)
                    .FirstOrDefault();

                // Check if the instance is in the given authority range
                if (rel != null && rel.Authority <= leastAuthority)
                {
                    // Add all projects of this instance to the project list
                    filteredProjects.Add(item);
                }
            }

            // Get the selected project
            var _project = _projectRepository.Get(projectId);

            if (_project == null)
                return StatusCodes.Status404NotFound;

            // Check if user is connected to project
            if (filteredProjects.Where(i => i.Id == _project.Id).FirstOrDefault() == null)
                return StatusCodes.Status403Forbidden;

            project = _project;
            return StatusCodes.Status200OK;
        }

        /// <summary>
        /// Updates a given project if the user has the ownership or edit
        /// authority of the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        [HttpPut("{projectId}")]
        public IActionResult UpdateProject([FromRoute] Guid projectId, [FromBody] Project project)
        {
            try
            {
                // Check if logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                // Check if user is authorized
                var result = TryGetProjectFromUser(projectId, LoggedInUser.Id,
                    EntityAuthority.Edit, out Project? oldProject);

                // Act on the result of the get project method
                switch (result)
                {
                    case StatusCodes.Status403Forbidden:
                        return Forbid();
                    case StatusCodes.Status404NotFound:
                        return NotFound($"No project with id {projectId} found!");
                    default:
                        if (oldProject == null)
                            return NotFound();
                        break;
                };

                // Safely set project id
                project.Id = oldProject.Id;

                // Update instance
                if (!_projectRepository.Update(ref project))
                {
                    throw new Exception("Error updating project!");
                }

                // Return updated project as JSON
                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Error updating project!");
        }

        /// <summary>
        /// Deletes the project with the given id if the user as ownership
        /// over the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpDelete("{projectId}")]
        public IActionResult DeleteProject([FromRoute] Guid projectId)
        {
            try
            {
                // Check if logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                // Check if user is authorized
                var result = TryGetProjectFromUser(projectId, LoggedInUser.Id,
                    EntityAuthority.Owner, out Project? project);

                // Act on the result of the get project method
                switch (result)
                {
                    case StatusCodes.Status403Forbidden:
                        return Forbid();
                    case StatusCodes.Status404NotFound:
                        return NotFound($"No project with id {projectId} found!");
                    default:
                        break;
                };

                if (project == null)
                    return StatusCode(500);

                // Delete instance
                if (!_projectRepository.Delete(project.Id))
                {
                    return StatusCode(500, "Error deleting project!");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(500, "Error deleting project!");
        }

        #region Networks

        /// <summary>
        /// Creates a new network in a project if the user has project <b>edit</b> rights
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        [HttpPost("{projectId}/Network")]
        public IActionResult CreateNetwork([FromRoute] Guid projectId, [FromBody] Network network)
        {
            try
            {
                if (LoggedInUser == null)
                    return Unauthorized();

                var authority = GetAuthorityFromProject(projectId, LoggedInUser.Id);

                if (authority == null || authority == EntityAuthority.View)
                    return Forbid();

                if (!_networkRepository.Insert(ref network))
                {
                    throw new Exception("Error inserting network!");
                }

                // Link user as owner of Network
                NetworkRoleRelation networkRoleRelation = new()
                {
                    From = LoggedInUser.Id,
                    To = network.Id,
                    Authority = EntityAuthority.Owner,
                };

                // Link project to instance
                ProjectContainsRelation projectContainsRelation = new()
                {
                    From = projectId,
                    To = network.Id,
                };

                // Try insert
                if (!_relationRepository.Insert(ref networkRoleRelation) ||
                    !_relationRepository.Insert(ref projectContainsRelation))
                {
                    // Rollback and delete project
                    _networkRepository.Delete(network.Id);

                    return StatusCode(500, "Server error inserting user->project or instance->project relation!");
                }


                return Created(nameof(CreateNetwork), network);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"Internal error creating network!");
        }

        /// <summary>
        /// Retrieves all networks from a specific project if the user
        /// has the right to <b>view</b> the networks
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("{projectId}/Networks")]
        public IActionResult GetProjectNetworks([FromRoute] Guid projectId)
        {
            try
            {
                if (LoggedInUser == null)
                    return Unauthorized();

                // Retireve all networks from project
                var result = GetNetworksFromProject(
                    projectId, LoggedInUser.Id, EntityAuthority.View);

                if (result == null || !result.Any())
                    return Ok(Array.Empty<object>());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"Internal error retrieving networks from project!");
        }

        private IEnumerable<Network> GetNetworksFromProject(Guid projectId, Guid userId, EntityAuthority leastAuth)
        {
            var networks = new List<Network>();
            var curAuth = GetAuthorityFromProject(projectId, userId);

            // Get networks
            var curNetworks = _projectRepository.GetNetworksOfProject(projectId);

            // Return networks if the auth is already inherited
            // through the project or instance
            if (curAuth != null && curAuth <= leastAuth)
                return curNetworks;

            // Get direct network relations to user
            foreach (var network in curNetworks)
            {
                var networkRoleRelation = _relationRepository
                    .GetRelations<NetworkRoleRelation>(userId, network.Id).FirstOrDefault();

                if (networkRoleRelation != null && networkRoleRelation.Authority <= leastAuth)
                    networks.Add(network);
            }

            // Return networks that correspond to the given authority
            return networks;
        }

        private EntityAuthority? GetAuthorityFromProject(Guid projectId, Guid userId)
        {
            EntityAuthority? maxAuth = null;

            // Get instance->project relation
            var instanceContainsRelation = _relationRepository.
                GetRelations<InstanceContainsRelation>(projectId).FirstOrDefault();
            if (instanceContainsRelation == null)
                throw new Exception("Project is not connected to any instance!");

            // Get instance
            var instance = _instanceRepository.Get(instanceContainsRelation.From);
            if (instance == null)
                throw new NullReferenceException("Instance cannot be null in this context!");

            // Get user authority from instance relation
            var instanceRoleRelation = _relationRepository.
                GetRelations<InstanceRoleRelation>(userId, instance.Id).FirstOrDefault();
            if (instanceRoleRelation != null &&
                (maxAuth == null || instanceRoleRelation.Authority < maxAuth))
                maxAuth = instanceRoleRelation.Authority;

            // Get user authority from project relation
            var projectRoleRelation = _relationRepository.
                GetRelations<ProjectRoleRelation>(userId, projectId).FirstOrDefault();
            if (projectRoleRelation != null)
            {
                if (maxAuth == null || projectRoleRelation.Authority < maxAuth)
                    maxAuth = projectRoleRelation.Authority;
            }

            return maxAuth;
        }

        #endregion
    }
}
