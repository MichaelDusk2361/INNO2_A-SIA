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
    public class CarrierCalculator : RoleCalculatorBase
    {
        public CarrierCalculator(INeo4JEngine neo4JEngine) : base(neo4JEngine) 
        {
            Priority = 100;
            AlwaysExecutes = false;
            ExecutionTimes = 1;
        }

        public override void Calculate(Guid networkId, ref NetworkStructure structure)
        {
            // Get all presitge nodes with query
            string? networkContains = Relation.GetRelationTypeName<NetworkContainsRelation>();
            string? influences = Relation.GetRelationTypeName<InfluencesRelation>();

            // Get total relationship count
            Query query = new(
                $"MATCH (network: {nameof(Network)})-[:{networkContains}]->(n:{nameof(Person)}), (n)-[r:{influences}]-() " +
                $"WHERE network.id=\"{networkId}\" " +
                $"RETURN COUNT(DISTINCT r) AS totalRelations, COUNT(DISTINCT n) AS totalPeople;");

            // Get total nodes count

            int totalRelations = int.Parse(
                Neo4JEngine.Database.RunQuery(query).FirstOrDefault()?["totalRelations"].ToString() ?? "");
            int totalPeople = int.Parse(
                Neo4JEngine.Database.RunQuery(query).FirstOrDefault()?["totalPeople"].ToString() ?? "");

            double carrierRequirement = (1 + totalRelations / totalPeople) * 2;

            // Get presitge nodes
            query = new(
                $"MATCH (network: {nameof(Network)})-[:{networkContains}]-(n:{nameof(Person)}), (n)-[r:{influences}]-() " +
                $"WHERE network.id=\"{networkId}\" " +
                $"WITH COUNT(r) AS totalR, n AS person " +
                $"MATCH (person) " +
                $"WHERE totalR >= {carrierRequirement} " +
                $"RETURN DISTINCT person.id AS id;");

            var guids = new List<Guid>();
            var results = Neo4JEngine.Database.RunQuery(query);

            foreach (var record in results)
            {
                guids.Add(
                    Guid.Parse(record["id"].ToString() ?? ""));
            }          

            // Apply / Remove role
            ApplyRoles(PersonRoles.Carrier, ref structure, guids);
        }
    }
}
