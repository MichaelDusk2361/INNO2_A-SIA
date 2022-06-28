using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.CalculationSystem.Roles
{
    public class OmegaCalculator : RoleCalculatorBase
    {
        public OmegaCalculator(INeo4JEngine neo4JEngine) : base(neo4JEngine) 
        {
            Priority = 100;
            AlwaysExecutes = false;
            ExecutionTimes = 1;
        }

        public override void Calculate(Guid networkId, ref NetworkStructure structure)
        {
            // Get nodes with most incoming relations
            Query query = new($"MATCH (network: {nameof(Network)})-[:{Relation.GetRelationTypeName<NetworkContainsRelation>()}]-" +
                $"(person: {nameof(Person)}), ()-[r:{Relation.GetRelationTypeName<InfluencesRelation>()}]->(person) " +
                $"WHERE network.id=\"{networkId}\" " +
                $"RETURN person.id as id, COUNT(r) AS relationCount " +
                $"ORDER BY relationCount DESC;");

            var results = Neo4JEngine.Database.RunQuery(query);

            List<Guid> guids = new();

            int threshold = 1;
            int i = 0;
            foreach (var record in results)
            {
                if (i >= threshold)
                    break;

                guids.Add(Guid.Parse(record["id"].ToString() ?? ""));
                i++;
            }

            // Apply / Remove role
            ApplyRoles(PersonRoles.Omega, ref structure, guids);
        }
    }
}
