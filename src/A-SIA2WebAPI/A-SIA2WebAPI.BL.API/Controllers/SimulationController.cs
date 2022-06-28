using Microsoft.AspNetCore.Mvc;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using System;
using Microsoft.Extensions.Logging;
using A_SIA2WebAPI.BL.CalculationSystem;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SimulationController : ExtendedControllerBase
    {
        private readonly Neo4JNetworkStructureRepository _networkStructureRepository;
        private readonly Neo4JNetworkRepository _networkRepository;
        private readonly ILogger<SimulationController> _logger;
        private readonly ICalculationEngine _calculationEngine;

        public SimulationController(
            ICalculationEngine calculationEngine,
            ILogger<SimulationController> logger,
            IRepository<User> userRepository,
            IRepository<NetworkStructure> networkStructureRepository,
            IRepository<Network> networkRepository) : base(userRepository)
        {
            _calculationEngine = calculationEngine;
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
        public IActionResult SimulateNetwork(
            [FromRoute] Guid networkId,
            [FromQuery] int t_end)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            bool canView = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id) != null;
            if (!canView)
                return Forbid();

            var networkStructure = _networkStructureRepository.Get(networkId);

            if (networkStructure == null)
                return StatusCode(500);

            var result = _calculationEngine.CalculateNetwork(networkId, networkStructure, t_end);

            return Ok(result);
        }
    }
}
