using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using A_SIA2WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class RelationController : ExtendedControllerBase
    {
        private readonly Neo4JRelationRepository _relationRepository;
        private readonly Neo4JNetworkRepository _networkRepository;
        private readonly Neo4JPersonRepository _personRepository;

        public RelationController(
            IRepository<User> userRepository,
            IRepository<Relation> relationRepository,
            IRepository<Network> networkRepository,
            IRepository<Person> personRepository) : base(userRepository)
        {
            // Get network repository
            {
                if (networkRepository is not Neo4JNetworkRepository repo)
                    throw new ArgumentException(nameof(repo));
                _networkRepository = repo;
            }
            // Get relation repository
            {
                if (relationRepository is not Neo4JRelationRepository repo)
                    throw new ArgumentException(nameof(repo));
                _relationRepository = repo;
            }
            // Get person repository
            {
                if (personRepository is not Neo4JPersonRepository repo)
                    throw new ArgumentException(nameof(repo));
                _personRepository = repo;
            }
        }

        // General Relation Operation

        [HttpDelete("Influences/{relationId}")]
        public IActionResult DeleteRelation([FromRoute] Guid relationId)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Get the network ids of the people
            Guid relationNetworkId = _relationRepository.GetNetworkId(relationId);

            if (relationNetworkId == Guid.Empty)
                return BadRequest();

            // Check that the user can edit
            var userAuth = _networkRepository.GetNetworkAuthority(relationNetworkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            if (!_relationRepository.Delete<InfluencesRelation>(relationId))
                return StatusCode(StatusCodes.Status500InternalServerError);
            return Ok();
        }

        [HttpPost("{fromPersonId}/Influences/{toPersonId}")]
        public IActionResult AddInfluencesRelation([FromRoute] Guid fromPersonId, [FromRoute] Guid toPersonId,
            [FromBody] InfluencesRelation relation)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Get the network ids of the people
            Guid fromPersonNetworkId = _personRepository.GetPersonNetworkId(fromPersonId);
            Guid toPersonNetworkId = _personRepository.GetPersonNetworkId(toPersonId);

            if (fromPersonNetworkId == Guid.Empty || toPersonNetworkId == Guid.Empty || fromPersonNetworkId != toPersonNetworkId)
                return BadRequest();

            // Check that the user can edit
            var userAuth = _networkRepository.GetNetworkAuthority(fromPersonNetworkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Verify relation model
            relation = GetVerifiedInfluencesRelation(
                relation,
                fromPersonId,
                toPersonId);

            // Create relation
            if (!_relationRepository.Insert(ref relation))
                return StatusCode(StatusCodes.Status500InternalServerError);
            return StatusCode(StatusCodes.Status201Created, relation);
        }

        [HttpPut("Influences/{relationId}")]
        public IActionResult UpdateInfluencesRelation([FromRoute] Guid relationId, [FromBody] InfluencesRelation relation)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Get the network ids of the people
            Guid relationNetworkId = _relationRepository.GetNetworkId(relationId);

            if (relationNetworkId == Guid.Empty || relationId == Guid.Empty)
                return BadRequest();

            // Check that the user can edit
            var userAuth = _networkRepository.GetNetworkAuthority(relationNetworkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            relation.Id = relationId;

            // Update the relation
            if (!_relationRepository.Update(ref relation))
                return StatusCode(StatusCodes.Status500InternalServerError);
            return Ok(relation);
        }

        private InfluencesRelation GetVerifiedInfluencesRelation(
            InfluencesRelation relation,
            Guid from,
            Guid to)
        {
            if (relation.Id == Guid.Empty)
                relation.Id = Guid.NewGuid();

            return new InfluencesRelation()
            {
                Id = relation.Id,
                From = from,
                To = to,
                Influence = relation.Influence,
                Interval = relation.Interval,
                Offset = relation.Offset,
            };
        }
    }
}
