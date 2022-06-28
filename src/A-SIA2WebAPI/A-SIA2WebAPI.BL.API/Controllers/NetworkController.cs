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
using System.Linq;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NetworkController : ExtendedControllerBase
    {
        private readonly ILogger<NetworkController> _logger;
        private readonly Neo4JNetworkRepository _networkRepository;

        public NetworkController(
            IRepository<User> userRepository,
            IRepository<Network> networkRepository,
            ILogger<NetworkController> logger) : base(userRepository)
        {
            _logger = logger;

            // Get network repository
            {
                if (networkRepository is not Neo4JNetworkRepository repo)
                    throw new ArgumentException(nameof(repo));
                _networkRepository = repo;
            }
        }

        /// <summary>
        /// Retrieves a specific network if the user has network <b>view</b> permissions
        /// </summary>
        /// <param name="networkId"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        [HttpGet("{networkId}")]
        public IActionResult GetNetwork([FromRoute] Guid networkId)
        {
            if (LoggedInUser == null)
                return Unauthorized();

            // Get permissions of user over network
            var authority = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);

            if (authority != null)
            {
                // Try to get the network
                var network = _networkRepository.Get(networkId);

                if (network != null)
                    return Ok(network);
                else
                    return NotFound();
            }
            return Forbid();
        }

        /// <summary>
        /// Updates a specific network if the user has network <b>edit</b> permissions
        /// </summary>
        /// <param name="networkId"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        [HttpPut("{networkId}")]
        public IActionResult UpdateNetwork([FromRoute] Guid networkId, [FromBody] Network network)
        {
            if (LoggedInUser == null)
                return Unauthorized();

            // Get permissions of user over network
            var authority = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);

            if (authority == EntityAuthority.Owner ||
                authority == EntityAuthority.Edit)
            {
                // Assert network id
                network.Id = networkId;

                // Try to update the network
                if (!_networkRepository.Update(ref network))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                return Ok(_networkRepository.Get(network.Id));
            }
            return Forbid();
        }

        /// <summary>
        /// Deletes a specific network if the user has network <b>ownership</b> permissions
        /// </summary>
        /// <param name="networkId"></param>
        /// <returns></returns>
        [HttpDelete("{networkId}")]
        public IActionResult DeleteNetwork([FromRoute] Guid networkId)
        {
            if (LoggedInUser == null)
                return Unauthorized();

            // Get permissions of user over network
            var authority = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);

            if(authority == EntityAuthority.Owner)
            {
                // Try to delete the network (cascades)
                if (!_networkRepository.Delete(networkId))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                return Ok();
            }
            return Forbid();
        }
    }
}