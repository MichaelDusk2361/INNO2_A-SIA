using Microsoft.AspNetCore.Mvc;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using System;
using Microsoft.AspNetCore.Http;
using A_SIA2WebAPI.Models.Relations;
using System.Linq;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PersonController : ExtendedControllerBase
    {
        private readonly Neo4JNetworkRepository _networkRepository;
        private readonly Neo4JPersonRepository _personRepository;
        private readonly Neo4JRelationRepository _relationRepository;

        public PersonController(
            IRepository<User> userRepository,
            IRepository<Person> personRepository,
            IRepository<Network> networkRepository,
            IRepository<Relation> relationRepository
            ) : base(userRepository)
        {
            // Get network repository
            {
                if (networkRepository is not Neo4JNetworkRepository repo)
                    throw new ArgumentException(nameof(repo));
                _networkRepository = repo;
            }
            // Get person repository
            {
                if (personRepository is not Neo4JPersonRepository repo)
                    throw new ArgumentException(nameof(repo));
                _personRepository = repo;
            }
            // Get relation repository
            {
                if (relationRepository is not Neo4JRelationRepository repo)
                    throw new ArgumentException(nameof(repo));
                _relationRepository = repo;
            }
        }

        [HttpPost("{networkId}")]
        public IActionResult CreatePerson([FromRoute] Guid networkId, [FromBody] Person person)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can edit
            var userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Create person
            if(!_personRepository.Insert(ref person))
                return StatusCode(StatusCodes.Status500InternalServerError);

            // Connect person to network
            NetworkContainsRelation rel = new()
            {
                From = networkId,
                To = person.Id
            };

            if (!_relationRepository.Insert(ref rel))
            {
                _personRepository.Delete(person.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return StatusCode(201, person);
        }

        [HttpPut("{personId}")]
        public IActionResult UpdatePerson([FromRoute] Guid personId, [FromBody] Person person)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can edit the person
            var networkId = _personRepository.GetPersonNetworkId(personId);
            if (networkId == null)
                return BadRequest();

            var userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Update person
            if (_personRepository.Update(ref person))
                return Ok(person);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpDelete("{personId}")]
        public IActionResult DeletePerson([FromRoute] Guid personId)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can edit the person
            Guid networkId = _personRepository.GetPersonNetworkId(personId);
            if (networkId == Guid.Empty)
                return BadRequest();

            EntityAuthority? userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Delete person
            if (_personRepository.Delete(personId))
                return Ok();
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
