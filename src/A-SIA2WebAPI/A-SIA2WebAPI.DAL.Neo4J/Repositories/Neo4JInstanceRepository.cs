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
    public class Neo4JInstanceRepository : IRepository<Instance>
    {
        private readonly INeo4JEngine _engine;
        private readonly ILogger<Neo4JInstanceRepository> _logger;

        public Neo4JInstanceRepository(
            INeo4JEngine engine,
            ILogger<Neo4JInstanceRepository> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        public bool Delete(Guid id) => _engine.TryDelete<Instance>(id, true);

        public Instance? Get(Guid id) => _engine.Get<Instance>(id);

        public IEnumerable<Instance> GetAll() => _engine.GetAll<Instance>();

        public bool Insert(ref Instance instance) => _engine.TryCreate(ref instance);

        public bool Update(ref Instance instance) => _engine.TrySet(ref instance);

        public IEnumerable<Project> GetProjectsOfInstance(Guid id)
        {
            // Delete all instances the user owns aswell
            Query query = new(
                $@"MATCH (i: {nameof(Instance)})-[r: {
                    Relation.GetRelationTypeName<InstanceContainsRelation>()
                    }]->(p: {nameof(Project)})
                WHERE i.id=""{id}""
                RETURN p;");

            try
            {
                // Select
                var records = _engine.Database.RunQuery(query);

                var projects = new List<Project>();

                foreach (var record in records)
                {
                    var t = _engine.ParseAs<Project>(
                            record["p"]
                            .As<INode>());

                    if (t != null)
                        projects.Add(t);
                }

                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an retrieving the projects of the instance {id}!");
            }

            return Enumerable.Empty<Project>();
        }
    }
}
