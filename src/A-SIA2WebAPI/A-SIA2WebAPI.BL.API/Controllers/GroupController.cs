using Microsoft.AspNetCore.Mvc;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using A_SIA2WebAPI.DAL.Common;
using System;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models.Relations;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class GroupController : ExtendedControllerBase
    {
        private readonly Neo4JNetworkRepository _networkRepository;
        private readonly Neo4JPersonRepository _personRepository;
        private readonly Neo4JGroupRepository _groupRepository;
        private readonly Neo4JRelationRepository _relationRepository;

        public GroupController(
                    IRepository<User> userRepository,
                    IRepository<Person> personRepository,
                    IRepository<Group> groupRepository,
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
            // Get group repository
            {
                if (groupRepository is not Neo4JGroupRepository repo)
                    throw new ArgumentException(nameof(repo));
                _groupRepository = repo;
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
        public IActionResult CreateGroup([FromRoute] Guid networkId, [FromBody] Group group)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can edit the network (add a group)
            var userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Create group
            if (!_groupRepository.Insert(ref group))
                return StatusCode(StatusCodes.Status500InternalServerError);

            // Connect group to network
            NetworkContainsRelation networkContainsRelation = new()
            {
                From = networkId,
                To = group.Id
            };
            if (!_relationRepository.Insert(ref networkContainsRelation))
            {
                _groupRepository.Delete(group.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return StatusCode(201, group);
        }

        [HttpDelete("{groupId}")]
        public IActionResult DeleteGroup([FromRoute] Guid groupId)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can delete the group
            var networkId = _groupRepository.GetGroupNetworkId(groupId);
            if (networkId == Guid.Empty)
                return BadRequest();

            var userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Delete group
            if (_personRepository.Delete(groupId))
                return Ok();
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPut("{groupId}")]
        public IActionResult UpdateGroup([FromRoute] Guid groupId, [FromBody] Group group)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can update the group
            var networkId = _groupRepository.GetGroupNetworkId(groupId);
            if (networkId == Guid.Empty)
                return BadRequest();
            var userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Update group
            if (_groupRepository.Update(ref group))
                return Ok(group);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        // -------------------------
        //          PERSON
        // -------------------------

        [HttpPost("{groupId}/{nodeId}")]
        public IActionResult AddNodeToGroup([FromRoute] Guid nodeId, [FromRoute] Guid groupId)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can edit the person and group and that both are in the same network
            Guid networkId = _personRepository.GetPersonNetworkId(nodeId);
            if (networkId == Guid.Empty)
                    networkId = _groupRepository.GetGroupNetworkId(nodeId);
            if (networkId == Guid.Empty || networkId != _groupRepository.GetGroupNetworkId(groupId))
                return BadRequest();

            EntityAuthority? userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Check that person is not in any other group
            if (_relationRepository.GetRelations<GroupContainsRelation>(nodeId).Count() > 0)
                return BadRequest();

            // Create relation
            GroupContainsRelation relation = new()
            {
                From = groupId,
                To = nodeId
            };
            if (_relationRepository.Insert(ref relation))
                return StatusCode(201);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpDelete("{groupId}/{nodeId}")]
        public IActionResult DetachNodeFromGroup([FromRoute] Guid nodeId, [FromRoute] Guid groupId)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can edit the person and group and that both are in the same network
            Guid networkId = _personRepository.GetPersonNetworkId(nodeId);
            if (networkId == Guid.Empty)
                networkId = _groupRepository.GetGroupNetworkId(nodeId);
            if (networkId == Guid.Empty || networkId != _groupRepository.GetGroupNetworkId(groupId))
                return BadRequest();

            EntityAuthority? userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Get group contains relation between group and person
            var relation = _relationRepository
                .GetRelations<GroupContainsRelation>(groupId, nodeId).FirstOrDefault();
            if (relation == null)
                return BadRequest();

            // Delete relation
            if (_relationRepository.Delete<GroupContainsRelation>(relation.Id))
                return Ok();
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPatch("{groupId}/{nodeId}")]
        public IActionResult ChangeGroupOfNode([FromRoute] Guid nodeId, [FromRoute] Guid groupId)
        {
            // Check that user is logged in
            if (LoggedInUser == null)
                return Unauthorized();

            // Check that the user can edit the person and group and that both are in the same network
            Guid networkId = _personRepository.GetPersonNetworkId(nodeId);
            if (networkId == Guid.Empty)
                networkId = _groupRepository.GetGroupNetworkId(nodeId);
            if (networkId == Guid.Empty || networkId != _groupRepository.GetGroupNetworkId(groupId))
                return BadRequest();

            EntityAuthority? userAuth = _networkRepository.GetNetworkAuthority(networkId, LoggedInUser.Id);
            if (userAuth == null || userAuth == EntityAuthority.View)
                return Forbid();

            // Delete old relation
            // Get group contains relation between group and person
            var oldRelation = _relationRepository
                .GetRelations<GroupContainsRelation>(nodeId).FirstOrDefault();
            if (oldRelation != null)
            {
                if (!_relationRepository.Delete<GroupContainsRelation>(oldRelation.Id))
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // Create new relation
            GroupContainsRelation newRelation = new()
            {
                From = groupId,
                To = nodeId
            };
            if (_relationRepository.Insert(ref newRelation))
                return StatusCode(201);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
