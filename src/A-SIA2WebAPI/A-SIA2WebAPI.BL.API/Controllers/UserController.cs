using Microsoft.AspNetCore.Mvc;
using A_SIA2WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using A_SIA2WebAPI.Services;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using System;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using static A_SIA2WebAPI.BL.API.Payloads.UserController;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;
using A_SIA2WebAPI.DAL.Common;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    /// <summary>
    /// Controller that is responsible for user related actions, such as
    /// registering, authenticating and managing users
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public partial class UserController : ExtendedControllerBase
    {
        // Services are retrieved via dependency injection
        private readonly IJwtAuthenticationService _authenticationService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IRepository<User> userRepository,
            IJwtAuthenticationService authenticationService,
            IPasswordHasher<User> hasher,
            ILogger<UserController> logger) : base(userRepository)
        {
            // Assign services
            _authenticationService = authenticationService;
            _passwordHasher = hasher;
            _logger = logger;
        }

        [HttpGet("{identification}")]
        public IActionResult GetUser([FromRoute] string identification)
        {
            try
            {
                // Retrieve user
                User? user = GetUserFromIdentification(identification);

                if (user == null)
                    return BadRequest("Use id or email as identification");

                // Compare emails for authentication
                if (!User.HasClaim(c => c.Value == user.Email))
                    return Unauthorized();

                // Remove hash for user
                user.Hash = "";

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                return StatusCode(500, "An error occured while retrieving the user");
            }
        }

        [HttpGet("loggedIn")]
        public IActionResult GetLoggedInUser()
        {
            try
            {
                if (LoggedInUser == null)
                    return Unauthorized();
                return Ok(LoggedInUser);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                return StatusCode(500, "An error occured while retireving the users instances");
            }
        }

        [HttpGet("instanceProjectNetworkRelations")]
        public IActionResult GetUserInstanceProjectNetworkRelations()
        {
            try
            {
                // Check if logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                var instanceProjectNetworkRelations = _userRepository.GetInstanceProjectNetworkRelationsOfUser(LoggedInUser.Id);

                return Ok(instanceProjectNetworkRelations);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                return StatusCode(500, "An error occured while retireving the users instances");
            }
        }

        [HttpGet("Instances")]
        public IActionResult GetUserInstances()
        {
            try
            {
                // Check if logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                var instances = _userRepository.GetInstancesOfUser(LoggedInUser.Id);

                return Ok(instances);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                return StatusCode(500, "An error occured while retireving the users instances");
            }
        }

        [HttpGet("Projects")]
        public IActionResult GetUserProjects()
        {
            try
            {
                if (LoggedInUser == null)
                    return Unauthorized();

                // Get rest of the projects
                var projects = _userRepository.GetProjectsOfUser(LoggedInUser.Id);
                var lmao = JsonConvert.SerializeObject(projects);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                return StatusCode(500, "An error occured while retireving the users instances");
            }
        }

        [HttpGet("Networks")]
        public IActionResult GetUserNetworks()
        {
            try
            {
                // Check if user is logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                var networks = _userRepository.GetNetworksOfUser(LoggedInUser.Id);

                return Ok(networks);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                return StatusCode(500, "An error occured while retireving the users instances");
            }
        }

        /// <summary>
        /// Authentication endpoint, returns a json web token if the
        /// authentication was successful
        /// </summary>
        /// <param name="credentials">Login credentials</param>
        /// <returns>200 and a token on success, 400 on false body,
        /// 401 on invalid credentials and 500 on an exception</returns>
        [HttpPost("Authenticate")]
        [AllowAnonymous]
        public IActionResult Authenticate([FromBody] CredentialsBody credentials)
        {
            try
            {
                if (credentials.Email == null || credentials.Password == null)
                    return BadRequest();

                string? token = _authenticationService.Authenticate(credentials.Email, credentials.Password);

                if (token == null)
                    return Unauthorized();
                return Ok(JsonConvert.SerializeObject(token));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                return StatusCode(500);
            }
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            return StatusCode(501);
        }

        /// <summary>
        /// Endpoint for registering a user, currently this can by done by everyone
        /// </summary>
        /// <param name="body"></param>
        /// <returns>200 on success, 400 on false body and 500 on an exception</returns>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterBody body)
        {
            try
            {
                // Check for valid body
                if (body.Email == null || body.Password == null ||
                    body.FirstName == null || body.LastName == null)
                    return BadRequest();

                // Check if user exists
                if (_userRepository.GetByEmail(body.Email) != null)
                    return BadRequest();

                // Create user object from body
                User user = new()
                {
                    Email = body.Email,
                    Hash = "",
                    FirstName = body.FirstName,
                    LastName = body.LastName,
                    Id = Guid.Empty,
                };
                user.Hash = _passwordHasher.HashPassword(user, body.Password);

                if (!_userRepository.Insert(ref user))
                    return BadRequest();

                user.Hash = "";

                return Created("api/user", user);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                return StatusCode(500);
            }
        }

        [HttpPut("{userId}")]
        public void UpdateUser([FromRoute] long userId, [FromBody] User user)
        {

        }

        [HttpDelete("{identification}")]
        public IActionResult DeleteUser([FromRoute] string identification)
        {
            try
            {
                // Retrieve user
                User? user = GetUserFromIdentification(identification);

                if (user == null)
                    return BadRequest("Use id or email as identification");

                // Compare emails for authentication
                if (!User.HasClaim(c => c.Value == user.Email))
                    return Unauthorized();

                // Delete user
                if (!_userRepository.Delete(user.Id))
                    throw new Exception("Error deleting user in database!");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                return StatusCode(500, "An error occured while deleting the user");
            }
        }



        private User? GetUserFromIdentification(string identification)
        {
            try
            {
                User? user;
                if (identification == null)
                    return null;

                if (Guid.TryParse(identification, out Guid id))
                {
                    user = _userRepository.Get(id);
                }
                else
                {
                    user = _userRepository.GetByEmail(identification);
                }

                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
