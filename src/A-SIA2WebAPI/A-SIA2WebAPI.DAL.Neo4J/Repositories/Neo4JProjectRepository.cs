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
    public class Neo4JProjectRepository : IRepository<Project>
    {
        private readonly INeo4JEngine _engine;
        private readonly ILogger<Neo4JProjectRepository> _logger;

        public Neo4JProjectRepository(
            INeo4JEngine engine,
            ILogger<Neo4JProjectRepository> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        public bool Delete(Guid id) => _engine.TryDelete<Project>(id, true);

        public Project? Get(Guid id) => _engine.Get<Project>(id);

        public IEnumerable<Project> GetAll() => _engine.GetAll<Project>();

        public bool Insert(ref Project project) => _engine.TryCreate(ref project);

        public bool Update(ref Project project) => _engine.TrySet(ref project);

        /// <summary>
        /// Get all networks that are linked to the given project
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<Network> GetNetworksOfProject(Guid id)
        {
            Query query = new(
                $@"MATCH (p: {nameof(Project)})-[r: {
                    Relation.GetRelationTypeName<ProjectContainsRelation>()
                    }]->(n: {nameof(Network)})
                WHERE p.id=""{id}""
                RETURN n;");

            try
            {
                // Select
                var records = _engine.Database.RunQuery(query);

                var networks = new List<Network>();

                foreach (var record in records)
                {
                    var t = _engine.ParseAs<Network>(
                            record["n"]
                            .As<INode>());

                    if (t != null)
                        networks.Add(t);
                }

                return networks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an retrieving the networks of the project {id}!");
            }

            return Enumerable.Empty<Network>();
        }
    }
}
