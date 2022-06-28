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
    public class Neo4JPersonRepository : IRepository<Person>
    {
        private readonly INeo4JEngine _engine;
        private readonly ILogger<Neo4JPersonRepository> _logger;

        public Neo4JPersonRepository(
            INeo4JEngine engine,
            ILogger<Neo4JPersonRepository> logger)
        {
            _logger = logger;
            _engine = engine;
        }

        public bool Delete(Guid id)
        {
            return _engine.TryDelete<Person>(id);
        }

        public Person? Get(Guid id)
        {
            return _engine.Get<Person>(id);
        }

        public IEnumerable<Person> GetAll()
        {
            return _engine.GetAll<Person>();
        }

        public bool Insert(ref Person person)
        {
            return _engine.TryCreate(ref person);
        }

        public bool Update(ref Person person)
        {
            return _engine.TrySet(ref person);
        }

        public Guid GetPersonNetworkId(Guid personId)
        {
            try
            {
                Query query = new(
                    $"MATCH (p: {nameof(Person)})<-[:{Relation.GetRelationTypeName<NetworkContainsRelation>()}]-(n: {nameof(Network)}) WHERE p.id=\"{personId}\" RETURN n.id as NetworkId;");

                // Select
                var result = _engine.Database.RunQuery(query).First()["NetworkId"].ToString();

                if(Guid.TryParse(result, out Guid networkId))
                {
                    return networkId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error getting the network id of person with id: {personId}!");
            }

            return Guid.Empty;
        }
    }
}
