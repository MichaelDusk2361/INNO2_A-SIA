using Microsoft.AspNetCore.Mvc;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using A_SIA2WebAPI.Models.Relations;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using A_SIA2WebAPI.DAL.Common;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NetworkStructureController : ExtendedControllerBase
    {
        private readonly Neo4JNetworkStructureRepository _networkStructureRepository;
        private readonly Neo4JNetworkRepository _networkRepository;
        private readonly ILogger<NetworkStructureController> _logger;

        public NetworkStructureController(
            ILogger<NetworkStructureController> logger,
            IRepository<User> userRepository,
            IRepository<NetworkStructure> networkStructureRepository,
            IRepository<Network> networkRepository) : base(userRepository)
        {
            _logger = logger;

            // Get NetworkStructure repository
            {
                if (networkStructureRepository is not Neo4JNetworkStructureRepository repo)
                    throw new ArgumentException(nameof(repo));
                _networkStructureRepository = repo;
            }
            // Get Network repository
            {
                if (networkRepository is not Neo4JNetworkRepository repo)
                    throw new ArgumentException(nameof(repo));
                _networkRepository = repo;
            }
        }

        [HttpGet("{networkId}")]
        public IActionResult GetNetworkStructure([FromRoute] Guid networkId)
        {
            try
            {
                // Check that user is logged in
                if (LoggedInUser == null)
                    return Unauthorized();

                bool canView = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id) != null;
                if (!canView)
                    return Forbid();

                var networkStructure = _networkStructureRepository.Get(networkId);

                return Ok(networkStructure);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    
        [HttpPut("{networkId}")]
        public IActionResult UpdateNetworkStructure(
            [FromRoute] Guid networkId, [FromBody] NetworkStructure structure)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can edit
            var userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            _networkStructureRepository.Update(ref structure, networkId);

            return Ok(JsonConvert.SerializeObject(structure));
        }
    }
}
