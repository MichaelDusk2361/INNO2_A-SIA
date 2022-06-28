using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Attributes;
using A_SIA2WebAPI.Models.Relations;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace A_SIA2WebAPI.DAL.Neo4J.Repositories
{
    public class Neo4JNetworkStructureRepository : IRepository<NetworkStructure>
    {
        private readonly INeo4JEngine _engine;
        private readonly ILogger<Neo4JNetworkStructureRepository> _logger;

        public Neo4JNetworkStructureRepository(INeo4JEngine engine, ILogger<Neo4JNetworkStructureRepository> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        public NetworkStructure? Get(Guid networkId)
        {
            try
            {
                Network? network = _engine.Get<Network>(networkId);

                // Network not found
                if (network == null)
                    return null;

                var types = Assembly.GetAssembly(typeof(Entity))?
                    .GetTypes().Where(t => typeof(Entity).IsAssignableFrom(t)).ToList();

                if (types == null)
                    return null;

                // Get all nodes from network
                List<SocialNode> nodes = new();

                Query query = new(
                    @$"MATCH (network: Network), (n)
                    WHERE network.id=$network_id
                    AND (network)-[:{Relation.GetRelationTypeName<NetworkContainsRelation>()}]->(n)
                    RETURN n;");
                query.Parameters.Add("network_id", networkId.ToString());

                var result = _engine.Database.RunQuery(query);

                foreach (var node in result)
                {
                    if (node.Keys.Contains("n"))
                    {
                        INode n = node["n"].As<INode>();

                        Type? type = types.Find(t => n.Labels.Contains(t.Name));
                        if (type != null)
                        {
                            object? obj = _engine.ParseAs(n, type);

                            if (obj != null)
                                nodes.Add((SocialNode)obj); // TO BE REVIEWED
                        }
                    }
                }

                // Get all relations from network
                List<Relation> relationships = new();

                query = new(
                    @"MATCH (network: Network), (n)-[r]-()
                    WHERE network.id=$network_id
                    AND (network)-[:NETWORK_CONTAINS]->(n)
                    RETURN DISTINCT r;");
                query.Parameters.Add("network_id", networkId.ToString());

                result = _engine.Database.RunQuery(query);

                foreach (var relationship in result)
                {
                    if (relationship.Keys.Contains("r"))
                    {
                        IRelationship r = relationship["r"].As<IRelationship>();

                        // Try to get type from mapping
                        Type? type = types.Where(t => t?.GetCustomAttribute<DatabaseRelationTypeAttribute>()?.Type == r.Type)
                            .FirstOrDefault();

                        // Catch any non mapped relation types and parse them as normal relations
                        if (type == null)
                            type = typeof(Relation);

                        // Add to collection
                        if (type != null)
                        {
                            object? obj = _engine.ParseAs(r, type);

                            if (obj != null)
                                relationships.Add((Relation)obj);
                        }
                    }
                }

                // Create network structure from data
                NetworkStructure networkStructure = new()
                {
                    People = new(),
                    Groups = new(),
                    InfluenceRelations = new()
                };

                // Add people
                nodes.ForEach(node => { if (node is Person) networkStructure.People.Add(node.As<Person>()); });

                // Add groups
                nodes.ForEach(node =>
                {
                    if (node is Group)
                    {
                        List<Guid> ids = new();
                        nodes.Where(n => relationships
                            .Where(r => r.RelationType == "GROUP_CONTAINS" && r.To == n.Id && r.From == node.Id)
                            .Any()).ToList().ForEach(n => ids.Add(n.Id));

                        networkStructure.Groups.Add(new()
                        {
                            Group = node.As<Group>(),
                            Nodes = ids,
                        });
                    }
                });

                // Add influence matrix
                relationships.ForEach(rel =>
                {
                    if (rel is InfluencesRelation irel)
                        networkStructure.InfluenceRelations.Add(irel);
                });

                return networkStructure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error retrieving the NetworkStructure from neo4j database!");
            }

            return null;
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("Cannot delete network structures")]
        public bool Delete(Guid id)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("Cannot retrieve multiple network structures")]
        public IEnumerable<NetworkStructure> GetAll()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("Cannot insert network structure as a whole")]
        public bool Insert(ref NetworkStructure entity)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("Need to use specific update method")]
        public bool Update(ref NetworkStructure entity)
        {
            throw new InvalidOperationException();
        }

        public bool Update(ref NetworkStructure networkStructure, Guid networkId)
        {
            // Get current network structure
            var oldNetworkStructure = Get(networkId);
            if (oldNetworkStructure == null)
                return false;

            /// People
            // Compare Person Nodes
            var newPeople = networkStructure.People;
            var oldPeople = oldNetworkStructure.People;

            // Delete people that are not in the new collection but in the old
            var peopleToRemove = oldPeople.Where(
                p => !newPeople.Any(p2 => p2.Id == p.Id)).ToArray();

            if (!_engine.TryBulkDelete<Person>(peopleToRemove.Select(p => p.Id).ToArray()))
            {
                _logger.LogError("There was an error deleting nodes!");
                return false;
            }

            // Insert people that are in the new collection but not in the old
            var newPeopleToInsert = newPeople.Where(
                p => !oldPeople.Any(p2 => p2.Id == p.Id)).ToList();

            for (int i = 0; i < newPeopleToInsert.Count; i++)
            {
                var person = newPeopleToInsert[i];
                // Create person
                if(!_engine.TryCreate(ref person))
                {
                    _logger.LogError("There was an error inserting person!");
                    return false;
                }
                // Link person to network
                var rel = new NetworkContainsRelation() { From = networkId, To = person.Id };
                if(!_engine.TryCreate(ref rel))
                {
                    // Delete person to avoid floating nodes
                    _engine.TryDelete<Person>(person.Id);
                    _logger.LogError("There was an error connecting person to network!");
                    return false;
                }
            }

            // Update people that are in both collections
            var newPeopleToUpdate = newPeople.Where(
                p => oldPeople.Any(p2 => p2.Id == p.Id && !p.Equals(p2))).ToList();

            for (int i = 0; i < newPeopleToUpdate.Count; i++)
            {
                var person = newPeopleToUpdate[i];
                if(!_engine.TrySet(ref person))
                {
                    _logger.LogError("There was an error updating person!");
                    return false;
                }
            }

            /// Compare Groups
            var newGroups = networkStructure.Groups;
            var oldGroups = oldNetworkStructure.Groups;

            // Delete old groups that do not exist in new groups collection
            var groupsToRemove = oldGroups.Where(
                g => !newGroups.Any(g2 => g2.Group != null && g.Group != null && g2.Group.Id == g.Group.Id))
                .Select(g => g.Group != null ? g.Group.Id : Guid.Empty).ToArray();

            if (!_engine.TryBulkDelete<Group>(groupsToRemove))
            {
                _logger.LogError("There was an error deleting groups!");
                return false;
            }

            // Insert new groups that do not exist in old groups collection
            var newGroupsToInsert = newGroups.Where(
                g => !oldGroups
                .Any(g2 => g2.Group != null && g.Group != null && g2.Group.Id == g.Group.Id))
                .ToList();

            for (int i = 0; i < newGroupsToInsert.Count; i++)
            {
                var groupEntry = newGroupsToInsert[i];
                if (groupEntry.Group == null)
                    continue;

                // Create group
                Group group = groupEntry.Group;
                if(!_engine.TryCreate(ref group))
                {
                    _logger.LogError("There was an error inserting group!");
                    return false;
                }

                // Link group to network
                var rel = new NetworkContainsRelation() { From = networkId, To = group.Id };
                if(!_engine.TryCreate(ref rel))
                {
                    // Delete group to avoid floating group in db
                    _engine.TryDelete<Group>(group.Id);
                    _logger.LogError("There was an error connecting group to network!");
                    return false;
                }

                // Link people to group
                foreach (var node in groupEntry.Nodes)
                {
                    var groupContais = new GroupContainsRelation()
                    {
                        From = group.Id,
                        To = node
                    };
                    if(!_engine.TryCreate(ref groupContais))
                    {
                        _logger.LogError("There was an error connecting node to group!");
                        return false;
                    }
                }
            }

            // Update groups that exist in both collections
            var groupsToUpdate = newGroups.Where(
                g => oldGroups.Any(g2 => g2.Group != null && g.Group != null && 
                g2.Group.Id == g.Group.Id && !g.Equals(g2))).ToList();

            foreach (var groupEntry in groupsToUpdate)
            {
                Group? group = groupEntry.Group;
                if (group == null)
                    continue;

                // Update group properties
                if (!_engine.TrySet(ref group))
                {
                    _logger.LogError("There was an error updating group!");
                    return false;
                }

                var newNodes = groupEntry.Nodes;
                // Get old group entry
                var oldNodes = oldGroups.Where(g => g.Group?.Id == group.Id).FirstOrDefault()?.Nodes;
                if (oldNodes == null)
                    continue;

                // Delete old nodes that do not exist in new nodes list
                var oldRelationsToDelete = oldNodes.Where(
                    n => !newNodes.Any(n2 => n2 == n)).ToArray();

                if (!_engine.TryBulkDeleteRelations<GroupContainsRelation>(
                    new Guid[] { group.Id}, oldRelationsToDelete))
                {
                    _logger.LogError("There was an error deleting GroupContainsRelation!");
                    return false;
                }

                // Insert new nodes that do not exist in old nodes list
                var newGroupRelationsToInsert = newNodes.Where(
                    n => !oldNodes.Any(n2 => n2 == n)).ToList();

                foreach (var nId in newGroupRelationsToInsert)
                {
                    var rel = new GroupContainsRelation()
                    {
                        From = group.Id,
                        To = nId
                    };
                    if (!_engine.TryCreate(ref rel))
                    {
                        _logger.LogError("There was an error inserting GroupContainsRelation!");
                        return false;
                    }
                }
            }

            /// Compare Relationships
            var newRelations = networkStructure.InfluenceRelations;
            var oldRelations = oldNetworkStructure.InfluenceRelations;

            // Update / Insert / Delete
            // Delete people that are not in the new collection but in the old
            var relationsToRemove = oldRelations.Where(
                r => !newRelations.Any(r2 => r2.Id == r.Id)).ToArray();

            if (!_engine.TryBulkDelete<InfluencesRelation>(
                relationsToRemove.Select(r => r.Id).ToArray()))
            {
                _logger.LogError("There was an error deleting influence relation!");
                return false;
            }

            // Insert people that are in the new collection but not in the old
            var newRelationsToInsert = newRelations.Where(
                r => !oldRelations.Any(r2 => r2.Id == r.Id)).ToList();

            for (int i = 0; i < newRelationsToInsert.Count; i++)
            {
                var rel = newRelationsToInsert[i];
                if(!_engine.TryCreate(ref rel))
                {
                    _logger.LogError("There was an error inserting influence relation!");
                    return false;
                }
            }

            // Update people that are in both collections
            var newRelationsToUpdate = newRelations.Where(
                r => oldRelations.Any(r2 => r2.Id == r.Id && !r.Equals(r2))).ToList();

            for (int i = 0; i < newRelationsToUpdate.Count; i++)
            {
                var rel = newRelationsToUpdate[i];
                if (!_engine.TrySet(ref rel))
                {
                    _logger.LogError("There was an error updating influence relation!");
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<Person> GetPeopleFromNetwork(long networkId)
        {
            try
            {
                string? networkContains = Relation.GetRelationTypeName<NetworkContainsRelation>();
                Query query = new(
                    @$"MATCH (person: {nameof(Person)})<-[{networkContains}]-(network: {nameof(Network)})
                    WHERE network.id=$network_id
                    RETURN DISTINCT person;");
                query.Parameters.Add("network_id", networkId.ToString());

                var records = _engine.Database.RunQuery(query);
                var people = new List<Person>();

                foreach (var record in records)
                {
                    var person = _engine.ParseAs<Person>(
                            record["person"]
                            .As<INode>());

                    if (person != null)
                        people.Add(person);
                }

                return people;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return Enumerable.Empty<Person>();
        }

        public IEnumerable<InfluencesRelation> GetInfluencesRelationsFromNetwork(long networkId)
        {
            try
            {
                Query query = new(
                @"MATCH (n: Network)-[:NETWORK_CONTAINS]->(: Person)-[r:INFLUENCES]-(:Person)
                WHERE n.id=$network_id
                RETURN COLLECT(DISTINCT r) AS relations;");
                query.Parameters.Add("network_id", networkId.ToString());

                var relationships = _engine.Database.RunQuery(query).FirstOrDefault()?["relations"].As<IRelationship[]>();
                if (relationships == null)
                    throw new Exception("Relationships was null");

                var i_rels = new List<InfluencesRelation>();
                foreach (var relationship in relationships)
                {
                    var rel = _engine.ParseAs<InfluencesRelation>(relationship);

                    if (rel != null)
                        i_rels.Add(rel);
                }

                return i_rels;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Enumerable.Empty<InfluencesRelation>();
        }

        public IEnumerable<GroupEntry> GetGroupsFromNetwork(long networkId)
        {
            try
            {
                Query query = new(
                    @"MATCH (n: Network)-[:NETWORK_CONTAINS]->(g :Group)
                    WHERE n.id=$network_id
                    RETURN COLLECT(DISTINCT g) AS groups;");
                query.Parameters.Add("network_id", networkId.ToString());

                var nodes = _engine.Database.RunQuery(query)
                    .FirstOrDefault()?["groups"].As<INode[]>();
                if (nodes == null)
                    throw new Exception("Group was null");

                // Create query for retrieving connected group nodes
                query = new(
                    @"MATCH (g: Group)-[:GROUP_CONTAINS]->(n)
                    WHERE g.id=$group_id
                    RETURN COLLECT(DISTINCT n) AS nodes;");

                var groupEntries = new List<GroupEntry>();
                foreach (var node in nodes)
                {
                    var group = _engine.ParseAs<Group>(node);

                    if (group != null)
                    {
                        query.Parameters.Add("group_id", networkId.ToString());

                        var groupNodes = _engine.Database.RunQuery(query)
                            .FirstOrDefault()?["nodes"].As<INode[]>();
                        if (groupNodes == null)
                            throw new Exception("Nodes was null");

                        GroupEntry entry = new()
                        {
                            Group = group,
                            Nodes = new(),
                        };

                        foreach (var n in groupNodes)
                        {
                            var parsedN = _engine.ParseAs<SocialNode>(n);

                            if(parsedN != null)
                                entry.Nodes.Add(parsedN.Id);
                        }
                        groupEntries.Add(entry);

                        query.Parameters.Clear();
                    }

                }

                return groupEntries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Enumerable.Empty<GroupEntry>();
        }

        /// <summary>
        /// Returns the network id of the network this person is connected to.
        /// If it is not connected to any network, this method will return -1!
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public Guid GetNodeNetworkId(Guid from)
        {
            var rel = _engine.GetRelations<NetworkContainsRelation>(from).FirstOrDefault();
            if (rel == null)
                return Guid.Empty;
            return rel.From;
        }
    }
}
