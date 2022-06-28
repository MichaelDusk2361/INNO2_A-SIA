using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A_SIA2WebAPI.DAL.Neo4J.Repositories
{
    public class Neo4JGroupRepository : IRepository<Group>
    {
        private readonly INeo4JEngine _engine;
        private readonly ILogger<Neo4JGroupRepository> _logger;

        public Neo4JGroupRepository(
            INeo4JEngine engine,
            ILogger<Neo4JGroupRepository> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        public bool Delete(Guid id)
        {
            return _engine.TryDelete<Group>(id);
        }

        public Group? Get(Guid id)
        {
            return _engine.Get<Group>(id);
        }

        public IEnumerable<Group> GetAll()
        {
            return _engine.GetAll<Group>();
        }

        public bool Insert(ref Group group)
        {
            return _engine.TryCreate(ref group);
        }

        public bool Update(ref Group group)
        {
            return _engine.TrySet(ref group);
        }

        public Guid GetGroupNetworkId(Guid groupId)
        {
            try
            {
                Query query = new(
                    $"MATCH (g: {nameof(Group)})<-[:{Relation.GetRelationTypeName<NetworkContainsRelation>()}]-(n: {nameof(Network)}) WHERE g.id=\"{groupId}\" RETURN n.id as NetworkId;");

                // Select
                var result = _engine.Database.RunQuery(query).First()["NetworkId"].ToString();

                if (Guid.TryParse(result, out Guid networkId))
                {
                    return networkId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error getting the network id of group with id: {groupId}!");
            }

            return Guid.Empty;
        }
    }
}
