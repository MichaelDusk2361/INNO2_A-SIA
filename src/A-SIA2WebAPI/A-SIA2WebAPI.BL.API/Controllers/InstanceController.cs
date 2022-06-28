using Microsoft.AspNetCore.Mvc;
using A_SIA2WebAPI.Models;
using Microsoft.Extensions.Logging;
using A_SIA2WebAPI.Services;
using System;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Newtonsoft.Json;
using A_SIA2WebAPI.Models.Relations;
using A_SIA2WebAPI.DAL.Common;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    /// <summary>
    /// A controller for managing instances. Instances are the most top layer
    /// of the network management hierarchy structure. An instance can contain
    /// multiple projects.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class InstanceController : ExtendedControllerBase
    {
        // Services are retrieved via dependency injection
        private readonly Neo4JInstanceRepository _instanceRepository;
        private readonly Neo4JRelationRepository _relationRepository;
        private readonly Neo4JProjectRepository _projectRepository;
        private readonly ILogger<InstanceController> _logger;

        public InstanceController(
            IRepository<User> userRepository,
            IRepository<Relation> relationRepository,
            IRepository<Project> projectRepository,
            IRepository<Instance> instanceRepository,
            ILogger<InstanceController> logger) : base(userRepository)
        {
            // Get Instance repository
            {
                if (instanceRepository is not Neo4JInstanceRepository repo)
                    throw new ArgumentException(nameof(repo));
                _instanceRepository = repo;
            }
            // Get Relation repository
            {
                if (relationRepository is not Neo4JRelationRepository repo)
                    throw new ArgumentException(nameof(repo));
                _relationRepository = repo;
            }
            // Get Project repository
            {
                if (projectRepository is not Neo4JProjectRepository repo)
                    throw new ArgumentException(nameof(repo));
                _projectRepository = repo;
            }

            _logger = logger;
        }

        /// <summary>
        /// Retrieves an instance if the user has instance <b>view</b> rights
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        [HttpGet("{instanceId}")]
        public IActionResult GetInstance([FromRoute] Guid instanceId)
        {
            try
            {
                if (LoggedInUser == null)
                    return Unauthorized();

                var instances = _userRepository.GetInstancesOfUser(LoggedInUser.Id);

                var result = _instanceRepository.Get(instanceId);

                if (result == null)
                    return NotFound($"No instance with id {instanceId} found!");

                if (instances.Where(i => i.Id == result.Id).FirstOrDefault() == null)
                    return Forbid();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(500, $"Internal error retrieving instance!");
        }

        /// <summary>
        /// Creates a new instance. Every logged in user can perform this action
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateInstance([FromBody] Instance instance)
        {
            try
            {
                // Check if logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                // Create instance
                if (!_instanceRepository.Insert(ref instance))
                    return StatusCode(500, "Server error inserting instance!");

                // Link user as owner from instance
                InstanceRoleRelation instanceRelation = new InstanceRoleRelation()
                {
                    From = LoggedInUser.Id,
                    To = instance.Id,
                    Authority = EntityAuthority.Owner,
                };

                // Try insert
                if(!_relationRepository.Insert(ref instanceRelation))
                {
                    // Rollback and delete instance
                    _instanceRepository.Delete(instance.Id);

                    return StatusCode(500, "Server error inserting instance relation!");
                }

                return StatusCode(201, instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(500, $"Internal error creating instance!");
        }

        /// <summary>
        /// Updates a specific instance. The user needs <b>edit</b> rights to perform this action
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        [HttpPut("{instanceId}")]
        public IActionResult UpdateInstance([FromRoute] Guid instanceId, [FromBody] Instance instance)
        {
            try
            {
                // Check if logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                // Check if user is authorized
                var instanceRelation = _relationRepository
                    .GetRelations<InstanceRoleRelation>(LoggedInUser.Id, instanceId).FirstOrDefault();

                if (instanceRelation == null || 
                    (instanceRelation.Authority != EntityAuthority.Owner &&
                    instanceRelation.Authority != EntityAuthority.Edit))
                {
                    return Forbid();
                }

                // Update instance
                if(!_instanceRepository.Update(ref instance))
                {
                    return StatusCode(500, "Error updating instance!");
                }

                instance.Id = instanceId;

                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(500, "Error updating instance!");
        }

        /// <summary>
        /// Deletes an instance and cascades all projects and networks in this instance.
        /// The user needs <b>ownership</b> to perform this action
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        [HttpDelete("{instanceId}")]
        public IActionResult DeleteInstance([FromRoute] Guid instanceId)
        {
            try
            {
                // Check if logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                // Check if user is authorized
                var instanceRelation = _relationRepository
                    .GetRelations<InstanceRoleRelation>(LoggedInUser.Id, instanceId).FirstOrDefault();

                if (instanceRelation == null || instanceRelation.Authority != EntityAuthority.Owner)
                    return Forbid();

                // Delete instance and cascade
                if (!_instanceRepository.Delete(instanceId))
                {
                    return StatusCode(500, "Error deleting instance!");
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(500, "Error deleting instance!");
        }

        #region Projects

        /// <summary>
        /// Retrieves all projects of an instance that the user can view
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        [HttpGet("{instanceId}/Projects")]
        public IActionResult GetInstanceProjects([FromRoute] Guid instanceId)
        {
            try
            {
                if (LoggedInUser == null)
                    return Unauthorized();

                // Get all instances of the user
                var instances = _userRepository.GetInstancesOfUser(LoggedInUser.Id);

                // Get instance
                var instance = _instanceRepository.Get(instanceId);

                if (instance == null)
                    return NotFound($"No instance with id {instanceId} found!");

                // Check if the user is connected to the instance
                if (instances.Where(i => i.Id == instance.Id).FirstOrDefault() == null)
                    return Forbid();

                // Get projects that are connected to the instances
                var projects = _instanceRepository.GetProjectsOfInstance(instance.Id);

                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(500, $"Internal error retrieving instance!");
        }

        /// <summary>
        /// Creates a new project in the instance.
        /// The user needs instnace <b>edit</b> rights to perform this action
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        [HttpPost("{instanceId}/Project")]
        public IActionResult CreateProject([FromRoute] Guid instanceId, [FromBody] Project project)
        {
            try
            {
                // Check if logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                // Retrieve instance and check if user is connected to instance
                var instances = _userRepository.GetInstancesOfUser(LoggedInUser.Id);
                var result = _instanceRepository.Get(instanceId);

                // No instance was found by this id
                if (result == null)
                    return NotFound($"No instance with id {instanceId} found!");

                // Instance is not related to user
                if (instances.Where(i => i.Id == result.Id).FirstOrDefault() == null)
                    return Forbid();

                // Check if user can edit instance
                var rel = _relationRepository.GetRelations<InstanceRoleRelation>
                    (LoggedInUser.Id, result.Id).FirstOrDefault();
                if(rel == null || (rel.Authority != EntityAuthority.Owner &&
                    rel.Authority != EntityAuthority.Edit))
                    return Forbid();

                // Create project
                if (!_projectRepository.Insert(ref project))
                    return StatusCode(500, "Server error inserting project!");

                // Link user as owner of project
                ProjectRoleRelation projectRoleRelation = new()
                {
                    From = LoggedInUser.Id,
                    To = project.Id,
                    Authority = EntityAuthority.Owner,
                };

                // Link project to instance
                InstanceContainsRelation instanceContainsRelation = new()
                {
                    From = instanceId,
                    To = project.Id,
                };

                // Try insert
                if (!_relationRepository.Insert(ref projectRoleRelation) ||
                    !_relationRepository.Insert(ref instanceContainsRelation))
                {
                    // Rollback and delete project
                    _projectRepository.Delete(project.Id);

                    return StatusCode(500, "Server error inserting user->project or instance->project relation!");
                }

                return Created(nameof(CreateProject), project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(500, $"Internal error creating project!");
        }

        #endregion
    }
}
