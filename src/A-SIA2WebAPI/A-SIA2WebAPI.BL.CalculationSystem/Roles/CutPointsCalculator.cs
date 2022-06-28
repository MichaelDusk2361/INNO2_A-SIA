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
    public class CutPointsCalculator : CalculatorBase
    {
        public CutPointsCalculator(INeo4JEngine neo4JEngine) : base(neo4JEngine)
        {
            Priority = 100;
            ExecutionTimes = 1;
            AlwaysExecutes = false;
        }

        public override void Calculate(Guid networkId, ref NetworkStructure structure)
        {
            // Get all isolators with query
            string? networkContains = Relation.GetRelationTypeName<NetworkContainsRelation>();
            string? influences = Relation.GetRelationTypeName<InfluencesRelation>();

            // Get Graph
            Query query = new(
                $"MATCH (n: {nameof(Network)})-[:{networkContains}]->(p: {nameof(Person)}), " +
                $"(p)-[r:{influences}]-() WHERE n.id=\"{networkId}\" " +
                $"RETURN COLLECT(DISTINCT p) AS nodes, COLLECT(r) AS edges;");

            var result = Neo4JEngine.Database.RunQuery(query);
            var q_nodes = result.First()["nodes"].As<IEnumerable<INode>>();
            var nodes = new Dictionary<Guid, List<Guid>>();
            foreach (var node in q_nodes)
            {
                if(Neo4JEngine.ParseAs<Person>(node) is Person person)
                    nodes.Add(person.Id, new());
            }

            var q_edges = result.First()["edges"].As<IEnumerable<IRelationship>>();
            var edges = new List<InfluencesRelation>();
            foreach (var edge in q_edges)
            {
                if (Neo4JEngine.ParseAs<InfluencesRelation>(edge) is InfluencesRelation rel &&
                    !edges.Any(e => e.To == rel.To && e.From == rel.From ||
                    e.To == rel.From && e.From == rel.To))
                {
                    edges.Add(rel);
                }
            }

            // Create graph
            Graph graph = new(nodes);
            edges.ForEach(e => graph.AddEdge(e.From, e.To));

            // Get cutpoints
            var cutpoints = graph.AP();

            structure.People.ForEach(person =>
            {
                if (cutpoints.Contains(person.Id) && !person.Roles.Contains(PersonRoles.CutPoint))
                    person.Roles.Add(PersonRoles.CutPoint);
            });
        }

        public class Graph
        {
            // Array of lists for Adjacency List Representation
            private readonly Dictionary<Guid, List<Guid>> adj;
            int time = 0;

            // Constructor
            public Graph(Dictionary<Guid, List<Guid>> values)
            {
                adj = values;
            }

            // Function to add an edge into the graph
            public void AddEdge(Guid v, Guid w)
            {
                adj[v].Add(w); // Add w to v's list.
                adj[w].Add(v); // Add v to w's list
            }

            // A recursive function that find articulation points using DFS
            // u --> The vertex to be visited next
            // visited[] --> keeps track of visited vertices
            // disc[] --> Stores discovery times of visited vertices
            // parent[] --> Stores parent vertices in DFS tree
            // ap[] --> Store articulation points
            private void APUtil(
                Guid u,
                Dictionary<Guid, bool> visited,
                Dictionary<Guid, int> disc,
                Dictionary<Guid, int> low,
                Dictionary<Guid, Guid> parent,
                Dictionary<Guid, bool> ap)
            {

                // Count of children in DFS Tree
                int children = 0;

                // Mark the current node as visited
                visited[u] = true;

                // Initialize discovery time and low value
                disc[u] = low[u] = ++time;

                // Go through all vertices adjacent to this
                foreach (Guid i in adj[u])
                {
                    Guid v = i; // v is current adjacent of u

                    // If v is not visited yet, then make it a child of u
                    // in DFS tree and recur for it
                    if (!visited[v])
                    {
                        children++;
                        parent[v] = u;
                        APUtil(v, visited, disc, low, parent, ap);

                        // Check if the subtree rooted with v has
                        // a connection to one of the ancestors of u
                        low[u] = Math.Min(low[u], low[v]);

                        // u is an articulation point in following cases

                        // (1) u is root of DFS tree and has two or more children.
                        if (parent[u] == Guid.Empty && children > 1)
                            ap[u] = true;

                        // (2) If u is not root and low value of one of its child
                        // is more than discovery value of u.
                        if (parent[u] != Guid.Empty && low[v] >= disc[u])
                            ap[u] = true;
                    }

                    // Update low value of u for parent function calls.
                    else if (v != parent[u])
                        low[u] = Math.Min(low[u], disc[v]);
                }
            }

            // The function to do DFS traversal.
            // It uses recursive function APUtil()
            public List<Guid> AP()
            {
                // Mark all the vertices as not visited
                Dictionary<Guid, bool> visited = new();
                Dictionary<Guid, int> disc = new();
                Dictionary<Guid, int> low = new();
                Dictionary<Guid, Guid > parent = new();
                Dictionary<Guid, bool> ap = new(); // To store articulation points

                // Initialize parent and visited, and
                // ap(articulation point) arrays
                foreach (Guid g in adj.Keys)
                {
                    parent[g] = Guid.Empty;
                    visited[g] = false;
                    ap[g] = false;
                }

                // Call the recursive helper function to find articulation
                // points in DFS tree rooted with vertex 'i'
                foreach (Guid g in adj.Keys)
                {
                    if (visited[g] == false)
                        APUtil(g, visited, disc, low, parent, ap);
                }

                // Now ap[] contains articulation points, print them
                return ap.Where(a => a.Value == true).Select(a => a.Key).ToList();
            }
        }
    }
}
