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
    public class Neo4JNetworkRepository : IRepository<Network>
    {
        private readonly INeo4JEngine _engine;
        private readonly ILogger<Neo4JNetworkRepository> _logger;

        public Neo4JNetworkRepository(INeo4JEngine engine, ILogger<Neo4JNetworkRepository> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        public bool Delete(Guid id)
        {
            return _engine.TryDelete<Network>(id, true);
        }

        public Network? Get(Guid id)
        {
            return _engine.Get<Network>(id);
        }

        public IEnumerable<Network> GetAll()
        {
            return _engine.GetAll<Network>();
        }

        public bool Insert(ref Network network)
        {
            return _engine.TryCreate(ref network);
        }

        public bool Update(ref Network network)
        {
            return _engine.TrySet(ref network);
        }
    
        public EntityAuthority? GetNetworkAuthority(Guid networkId, Guid userId)
        {
            try
            {
                List<int?> result = new();

                string? networkRoleName = Relation.GetRelationTypeName<NetworkRoleRelation>();
                string? instanceRoleName = Relation.GetRelationTypeName<InstanceRoleRelation>();
                string? projectRoleName = Relation.GetRelationTypeName<ProjectRoleRelation>();
                if (networkRoleName == null || instanceRoleName == null || projectRoleName == null)
                    throw new NullReferenceException();

                // Direct relation
                result.Add(
                    _engine.Database.RunQuery(
                        GetAuthQuery(networkId, userId, instanceRoleName, "instance"))
                        .FirstOrDefault()?["authority"].As<int?>());

                // Project relation
                result.Add(
                    _engine.Database.RunQuery(
                        GetAuthQuery(networkId, userId, projectRoleName, "project"))
                        .FirstOrDefault()?["authority"].As<int?>());

                // Instance relation
                result.Add(
                    _engine.Database.RunQuery(
                        GetAuthQuery(networkId, userId, networkRoleName, "network"))
                        .FirstOrDefault()?["authority"].As<int?>());

                EntityAuthority? authority = null;
                foreach (var auth in result)
                {
                    if(auth != null)
                    {
                        if (authority == null || auth < (int)authority)
                            authority = (EntityAuthority)auth;
                    }
                }
                return authority;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return null;
        }

        private Query GetAuthQuery(Guid networkId, Guid userId, string relationRoleType, string relationEndNode)
        {
            string? projectContainsName = Relation.GetRelationTypeName<ProjectContainsRelation>();
            string? instanceContainsName = Relation.GetRelationTypeName<InstanceContainsRelation>();

            string queryString = @$"MATCH
                    (network: {nameof(Network)}),
                    (network)<-[:{projectContainsName}]-(project: {nameof(Project)}),
                    (project)<-[:{instanceContainsName}]-(instance: {nameof(Instance)}),
                    (user: {nameof(User)})-[r: {relationRoleType}]->({relationEndNode})
                    WHERE network.id=$network_id AND user.id=$user_id
                    RETURN r.authority as authority;";

            Query query = new(queryString);
            query.Parameters.Add("network_id", networkId.ToString());
            query.Parameters.Add("user_id", userId.ToString());

            return query;
        }
    }
}
