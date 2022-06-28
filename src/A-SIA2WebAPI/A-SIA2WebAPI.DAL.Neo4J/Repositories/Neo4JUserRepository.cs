using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace A_SIA2WebAPI.DAL.Neo4J.Repositories
{
    public class Neo4JUserRepository : IRepository<User>
    {
        private readonly INeo4JEngine _engine;
        private readonly ILogger<Neo4JUserRepository> _logger;

        public Neo4JUserRepository(INeo4JEngine engine, ILogger<Neo4JUserRepository> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        public bool Delete(Guid id)
        {
            // Delete all instances the user owns aswell
            Query query = new(
                $"MATCH (i: Instance)<-[r: INSTANCE_ROLE]-(u: User) " +
                $"WHERE u.id=\"{id}\" AND r.authority={(int)EntityAuthority.Owner} RETURN i;");

            try
            {
                // Select
                var records = _engine.Database.RunQuery(query);

                foreach (var record in records)
                {
                    var t = _engine.ParseAs<Instance>(
                            record["i"]
                            .As<INode>());

                    if (t != null && !_engine.TryDelete<Instance>(t.Id, true))
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error deleting the instances of user {id}!");
                return false;
            }

            return _engine.TryDelete<User>(id);
        }

        public User? Get(Guid id)
        {
            return _engine.Get<User>(id);
        }

        public User? GetByEmail(string email)
        {
            // Create query 
            Query query = new(
                @$"MATCH (user: User)
                WHERE user.email='{email}'
                RETURN user;");

            try
            {
                // Select
                var result = _engine.Database
                    .RunQuery(query)
                    .First()["user"]
                    .As<IEntity>();

                return _engine.ParseAs<User>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error retrieving user with email: {email}!");
                return null;
            }
        }

        public IEnumerable<User> GetAll()
        {
            return _engine.GetAll<User>();
        }

        public bool Insert(ref User user)
        {
            return _engine.TryCreate(ref user);
        }

        public bool Update(ref User user)
        {
            return _engine.TrySet(ref user);
        }

        public IEnumerable<Instance> GetInstancesOfUser(Guid userId)
        {
            try
            {
                // Get all instances of the user
                List<Instance> instances = new();

                Query query = new(
                    $"MATCH (u: {nameof(User)})-[r: {Relation.GetRelationTypeName<InstanceRoleRelation>()}]->(i :{nameof(Instance)}) " +
                    $"WHERE u.id=$user_id RETURN i;");
                query.Parameters.Add("user_id", userId.ToString());

                var records = _engine.Database.RunQuery(query);
                query = new(
                    @$"MATCH (u: {nameof(User)})-[r: {Relation.GetRelationTypeName<InstanceRoleRelation>()}]->(i :{nameof(Instance)})
                WHERE id(u)=$user_id RETURN i;");
                query.Parameters.Add("user_id", userId.ToString());

                records.AddRange(_engine.Database.RunQuery(query));
                foreach (var record in records)
                {
                    var instance = _engine.ParseAs<Instance>(record["i"].As<INode>());
                    if (instance != null && !instances.Exists(i => i.Id == instance.Id))
                        instances.Add(instance);
                }

                return instances;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Enumerable.Empty<Instance>();
        }

        public IEnumerable<Project> GetProjectsOfUser(Guid userId)
        {
            try
            {
                List<Project> projects = new();
                // Query for retrieving all projects that are directly connected to the user
                Query query = new(
                    $"MATCH (u: {nameof(User)})-[:{Relation.GetRelationTypeName<ProjectRoleRelation>()}]->(p: {nameof(Project)}) " +
                    $"WHERE u.id=$user_id RETURN p;");
                query.Parameters.Add("user_id", userId.ToString());

                var records = _engine.Database.RunQuery(query);

                // Query for indirectly connected projects
                query = new(
                    $"MATCH (u: {nameof(User)})-[:{Relation.GetRelationTypeName<InstanceRoleRelation>()}]->(:{nameof(Instance)})-[:{Relation.GetRelationTypeName<InstanceContainsRelation>()}]->(p :{nameof(Project)}) " +
                    $"WHERE u.id=$user_id RETURN p; ");
                query.Parameters.Add("user_id", userId.ToString());

                records.AddRange(_engine.Database.RunQuery(query));

                foreach (var record in records)
                {
                    var project = _engine.ParseAs<Project>(record["p"].As<INode>());
                    if (project != null && !projects.Exists(p => p.Id == project.Id))
                        projects.Add(project);
                }

                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Enumerable.Empty<Project>();
        }

        public IEnumerable<Network> GetNetworksOfUser(Guid userId)
        {
            try
            {
                List<Network> networks = new();

                // Get directly connected networks
                Query query = new(
                    $"MATCH (u: {nameof(User)})-[:{Relation.GetRelationTypeName<NetworkRoleRelation>()}]->(n: {nameof(Network)}) " +
                    $"WHERE u.id=$user_id RETURN n;");
                query.Parameters.Add("user_id", userId.ToString());

                var records = _engine.Database.RunQuery(query);

                // Get indirectly connected networks from projects that are directly connected
                query = new(
                    $"MATCH (u: {nameof(User)})-[:{Relation.GetRelationTypeName<ProjectRoleRelation>()}]->(:{nameof(Project)})-[:{Relation.GetRelationTypeName<ProjectContainsRelation>()}]->(n: {nameof(Network)}) " +
                    $"WHERE u.id=$user_id RETURN n;");
                query.Parameters.Add("user_id", userId.ToString());

                records.AddRange(_engine.Database.RunQuery(query));

                // Get indirectly connected networks from projects that are indirectly connected to instances
                query = new(
                    $"MATCH (u: {nameof(User)})-[:{Relation.GetRelationTypeName<InstanceRoleRelation>()}]->(:{nameof(Instance)})-[:{Relation.GetRelationTypeName<InstanceContainsRelation>()}]->(:{nameof(Project)})-[:{Relation.GetRelationTypeName<ProjectContainsRelation>()}]->(n: {nameof(Network)}) " +
                    $"WHERE u.id=$user_id RETURN n;");
                query.Parameters.Add("user_id", userId.ToString());

                records.AddRange(_engine.Database.RunQuery(query));
                
                foreach (var record in records)
                {
                    var network = _engine.ParseAs<Network>(record["n"].As<INode>());
                    if (network != null && !networks.Exists(n => n.Id == network.Id))
                        networks.Add(network);
                }

                return networks;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Enumerable.Empty<Network>();
        }

        public InstanceProjectNetworkRelations? GetInstanceProjectNetworkRelationsOfUser(Guid userId)
        {
            try
            {
                InstanceProjectNetworkRelations instanceProjectNetworkRelations = new();

                // Get directly connected networks
                Query query = new(
$@"MATCH (u: {nameof(User)})-[:INSTANCE_ROLE]->(:{nameof(Instance)})-[ic:INSTANCE_CONTAINS]->(p:{nameof(Project)})
OPTIONAL MATCH (p:{nameof(Project)})-[pc:PROJECT_CONTAINS]->(:{nameof(Network)})
WHERE u.id=$user_id RETURN ic,pc;");
                query.Parameters.Add("user_id", userId.ToString());

                var records = _engine.Database.RunQuery(query);

                foreach (var record in records)
                {
                    var instanceContainsRelation = _engine.ParseAs<InstanceContainsRelation>(record["ic"].As<IRelationship>());
                    if (instanceContainsRelation != null && !instanceProjectNetworkRelations.InstanceContainsRelations.Exists(n => n.Id == instanceContainsRelation.Id))
                        instanceProjectNetworkRelations.InstanceContainsRelations.Add(instanceContainsRelation);
                }
                foreach (var record in records)
                {
                    var projectContainsRelation = _engine.ParseAs<ProjectContainsRelation>(record["pc"].As<IRelationship>());
                    if (projectContainsRelation != null && !instanceProjectNetworkRelations.ProjectContainsRelations.Exists(n => n.Id == projectContainsRelation.Id))
                        instanceProjectNetworkRelations.ProjectContainsRelations.Add(projectContainsRelation);
                }

                return instanceProjectNetworkRelations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return null;
        }
    }
}
