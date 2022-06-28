using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.Models;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A_SIA2WebAPI.DAL.Neo4J.Repositories
{
    public class Neo4JRelationRepository : IRepository<Relation>
    {
        private readonly INeo4JEngine _engine;
        private readonly ILogger<Neo4JRelationRepository> _logger;

        public Neo4JRelationRepository(
            INeo4JEngine engine,
            ILogger<Neo4JRelationRepository> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        [Obsolete("Use overloaded method with specific type")]
        public bool Delete(Guid id)
        {
            return _engine.TryDelete<Relation>(id);
        }

        public bool Delete<T>(Guid id) where T : Relation
        {
            return _engine.TryDelete<Relation>(id);
        }

        [Obsolete("Use overloaded method with specific type")]
        public Relation? Get(Guid id)
        {
            return _engine.Get<Relation>(id);
        }

        public T? Get<T>(Guid id) where T : Relation
        {
            return _engine.Get<T>(id);
        }

        [Obsolete("Use overloaded method with specific type")]
        public IEnumerable<Relation> GetAll()
        {
            return _engine.GetAll<Relation>();
        }

        [Obsolete("Use overloaded method with specific type")]
        public IEnumerable<Relation> GetAll(string relationName)
        {
            return _engine.GetAllRelations<Relation>(relationName);
        }

        public IEnumerable<T> GetAll<T>() where T : Relation
        {
            return _engine.GetAllRelations<T>(Relation.GetRelationTypeName<T>() ?? "");
        }

        [Obsolete("Use overloaded method with specific type")]
        public bool Insert(ref Relation relation)
        {
            return _engine.TryCreate(ref relation);
        }

        public bool Insert<T>(ref T relation) where T : Relation
        {
            return _engine.TryCreate(ref relation);
        }

        /// <summary>
        /// <b>IMPORTANT NOTICE</b><br></br>
        /// Only the properties will be changed on update! <u>The
        /// start node, end node and relation-type wont be updated</u>!
        /// If these parameter should be updated, create a new
        /// relation and delete the old one.
        /// </summary>
        /// <param name="relation"></param>
        [Obsolete("Use overloaded method with specific type")]
        public bool Update(ref Relation relation)
        {
            return _engine.TrySet(ref relation);
        }

        /// <summary>
        /// <b>IMPORTANT NOTICE</b><br></br>
        /// Only the properties will be changed on update! <u>The
        /// start node, end node and relation-type wont be updated</u>!
        /// If these parameter should be updated, create a new
        /// relation and delete the old one.
        /// </summary>
        /// <param name="relation"></param>
        public bool Update<T>(ref T relation) where T : Relation
        {
            return _engine.TrySet(ref relation);
        }

        public IEnumerable<T> GetRelations<T>(Guid fromId, Guid toId) where T : Relation
        {
            return _engine.GetRelations<T>(fromId, toId);
        }

        /// <summary>
        /// Returns all relations that are connected to this node
        /// from a specific relation type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public IEnumerable<T> GetRelations<T>(Guid nodeId) where T : Relation
        {
            return _engine.GetRelations<T>(nodeId);
        }

        public Guid GetNetworkId(Guid relationId)
        {
            try
            {
                Query query = new(
                    $"MATCH (network: Network)-[:NETWORK_CONTAINS]-()-[rel]-() WHERE rel.id=\"{relationId}\" RETURN network.id as NetworkId;");

                // Select
                var result = _engine.Database.RunQuery(query).First()["NetworkId"].ToString();

                if (Guid.TryParse(result, out Guid networkId))
                {
                    return networkId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error getting the network id of relationship with id: {relationId}!");
            }

            return Guid.Empty;
        }
    }
}
