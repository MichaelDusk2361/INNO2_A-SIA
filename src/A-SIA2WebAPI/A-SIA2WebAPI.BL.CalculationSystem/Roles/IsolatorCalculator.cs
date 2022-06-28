using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A_SIA2WebAPI.BL.CalculationSystem.Roles
{
    public class IsolatorCalculator : RoleCalculatorBase
    {
        public IsolatorCalculator(INeo4JEngine neo4JEngine) : base(neo4JEngine)
        {
            Priority = 100;
            AlwaysExecutes = false;
            ExecutionTimes = 1;
        }

        public override void Calculate(Guid networkId, ref NetworkStructure structure)
        {
            // Get all isolators with query
            string? networkContains = Relation.GetRelationTypeName<NetworkContainsRelation>();
            string? influences = Relation.GetRelationTypeName<InfluencesRelation>();

            Query queryString = new(
                $"MATCH (n:{nameof(Network)})-[:{networkContains}]->(p:{nameof(Person)}) " +
                $"WHERE n.id=\"{networkId}\" AND ()-[:{influences}]->(p) " +
                $"AND NOT (p)-[:{influences}]->() RETURN p.id AS id;");

            // Parse guids of isolators and modify People collection
            var results = Neo4JEngine.Database.RunQuery(queryString);
            var guids = new List<Guid>();
            foreach (var record in results)
            {
                guids.Add(Guid.Parse(record["id"].ToString() ?? ""));
            }

            // Apply / Remove role
            ApplyRoles(PersonRoles.Isolator, ref structure, guids);
        }
    }
}
