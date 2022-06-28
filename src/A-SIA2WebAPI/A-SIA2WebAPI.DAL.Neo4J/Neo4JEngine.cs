using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Attributes;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace A_SIA2WebAPI.DAL.Neo4J
{
    /// <summary>
    /// The <see cref="Neo4JEngine"/> is responsible for automatically retrieving and
    /// parsing data from the database into classes. These classes have
    /// to derive from type <see cref="Entity"/>.
    /// </summary>
    public class Neo4JEngine : INeo4JEngine
    {
        public Neo4JDatabase Database { get; }

        private readonly ILogger<Neo4JEngine> _logger;

        public Neo4JEngine(Neo4JDatabase database, ILogger<Neo4JEngine> logger)
        {
            _logger = logger;
            Database = database;

            ConfigureConstraints();
        }

        private void ConfigureConstraints()
        {
            // Nodes
            ConfigureGuidConstraints<Group>();
            ConfigureGuidConstraints<Instance>();
            ConfigureGuidConstraints<Network>();
            ConfigureGuidConstraints<Project>();
            ConfigureGuidConstraints<Person>();
            ConfigureGuidConstraints<User>();
            Database.RunQuery(new Query(
                $"CREATE INDEX {nameof(User)}_EMAIL_INDEX IF NOT EXISTS " +
                $"FOR (e:{nameof(User)}) ON (e.email);"));

            // Relations
            (from t in Assembly.GetExecutingAssembly().GetTypes()
             where t.IsClass && t.Namespace == "A_SIA2WebAPI.Models.Relations"
             select t).ToList().ForEach(t => ConfigureGuidConstraints(t));
        }

        private void ConfigureGuidConstraints<T>() where T : Entity
        {
            ConfigureGuidConstraints(typeof(T));
        }
        private void ConfigureGuidConstraints(Type type)
        {
            try
            {
                if (!type.IsSubclassOf(typeof(Entity)))
                    throw new Exception("Type has to derive from entity!");

                Query constraintQuery;
                Query indexQuery;

                if (type == typeof(Relation))
                {
                    // Check if there is an attrbiute for the type
                    string? relName = Relation.GetRelationTypeName(type);

                    constraintQuery = new(
                        $"CREATE CONSTRAINT {relName}_GUID_CONSTRAINT IF NOT EXISTS " +
                        $"FOR ()-[e:{relName}]-() REQUIRE e.id IS UNIQUE;");
                    indexQuery = new(
                        $"CREATE INDEX {relName}_GUID_INDEX IF NOT EXISTS " +
                        $"FOR ()-[e:{relName}]-() ON (e.id);");
                }
                else
                {
                    constraintQuery = new(
                        $"CREATE CONSTRAINT {type.Name}_GUID_CONSTRAINT IF NOT EXISTS " +
                        $"FOR (e:{type.Name}) REQUIRE e.id IS UNIQUE;");
                    indexQuery = new(
                        $"CREATE INDEX {type.Name}_GUID_INDEX IF NOT EXISTS " +
                        $"FOR(e:{ type.Name }) ON(e.id);");
                }

                Database.RunQuery(constraintQuery);
                Database.RunQuery(indexQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"There was an error creating the GUID constriant or index for {type.Name}! Error: {ex}");
            }
        }

        /// <summary>
        /// Gets all the Entites of the given type T
        /// </summary>
        /// <typeparam name="T">Has to be of type <see cref="Entity"/></typeparam>
        /// <returns>An IEnumerable with entities or an empty IEnumerable on failure</returns>
        public IEnumerable<T> GetAll<T>() where T : Entity
        {
            Query query;
            Type type = typeof(T);
            // Relation
            if (IsRelation(type))
            {
                // Check if there is an attrbiute for the type
                string? relName = Relation.GetRelationTypeName(type);

                query = new(
                @$"MATCH ()-[a{(relName == null ? "" : $":{relName}")}]-()
                RETURN DISTINCT a;");
            }
            // Node
            else
            {
                // Create query
                query = new(
                @$"MATCH (a: {type.Name})
                RETURN a;");
            }

            try
            {
                // Select
                var results = Database
                    .RunQuery(query);

                List<T> resultList = new();
                foreach (var r in results)
                {
                    try
                    {
                        var entity = ParseAs<T>(r["a"].As<IEntity>());
                        if (entity != null)
                            resultList.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"There was an error parsing an entity of type {nameof(T)} from the database result set!");
                    }
                }

                return resultList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error retrieving all of {nameof(T)}!");
                return Enumerable.Empty<T>();
            }
        }

        public IEnumerable<T> GetRelations<T>(Guid nodeId) where T : Relation
        {
            Query query = new(
                $"MATCH (a)-[r:{Relation.GetRelationTypeName<T>()}]-() WHERE a.id=\"{nodeId}\" RETURN r;");

            try
            {
                // Select
                var records = Database
                    .RunQuery(query);

                var relationships = new List<T>();

                foreach (var record in records)
                {
                    var t = ParseAs<T>(
                            record["r"]
                            .As<IRelationship>());

                    if (t != null)
                        relationships.Add(t);
                }

                return relationships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error retrieving relations of type {nameof(T)}!");
                return Enumerable.Empty<T>();
            }
        }

        /// <summary>
        /// Gets all the Relation of the given type T and the relationType name.
        /// It is also possible to parse any relation of the given type string as
        /// a <see cref="Relation"/>.
        /// </summary>
        /// <typeparam name="T">Has to be of type <see cref="Relation"/>.</typeparam>
        /// <param name="relationType">The type parameter of the neo4j relation as string</param>
        /// <returns>An IEnumerable with relations or an empty IEnumerable on failure</returns>
        public IEnumerable<T> GetAllRelations<T>(string relationType) where T : Relation
        {
            try
            {
                Query query = new(
                @$"MATCH ()-[a:{relationType}]-()
                RETURN DISTINCT a;");

                // Select
                var results = Database
                    .RunQuery(query);

                List<T> resultList = new();
                foreach (var r in results)
                {
                    try
                    {
                        var entity = ParseAs<T>(r["a"].As<IEntity>());
                        if (entity != null)
                            resultList.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"There was an error parsing an entity of type {nameof(T)} from the database result set!");
                    }
                }

                return resultList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error retrieving all of {nameof(T)}!");
                return Enumerable.Empty<T>();
            }
        }

        /// <summary>
        /// Tries to retreive the entity with the provided id
        /// and parse it as the given type
        /// </summary>
        /// <typeparam name="T">Has to be of type <see cref="Entity"/> and the result
        /// will be parsed as this type</typeparam>
        /// <param name="id">The uuid id of the entity</param>
        /// <returns>Entity of type T on success or null on failure</returns>
        public T? Get<T>(Guid id) where T : Entity
        {
            Query query;
            // Relation
            if (IsRelation(typeof(T)))
            {
                query = new($"MATCH ()-[a]-() WHERE a.id=\"{id}\" RETURN a;");
            }
            // Node
            else
            {
                // Create query
                query = new($"MATCH (a) WHERE a.id=\"{id}\" RETURN a;");
            }

            try
            {
                // Select
                var result = Database
                    .RunQuery(query)
                    .First()["a"]
                    .As<IEntity>();

                return ParseAs<T>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error retrieving {nameof(T)}!");
                return null;
            }
        }

        /// <summary>
        /// Tries to retrieve 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <returns></returns>
        public IEnumerable<T> GetRelations<T>(Guid fromId, Guid toId) where T : Relation
        {
            Query query = new(
                $"MATCH (a)-[r:{Relation.GetRelationTypeName<T>()}]->(b) WHERE a.id=\"{ fromId}\" AND b.id=\"{ toId}\" RETURN r;");

            try
            {
                // Select
                var records = Database
                    .RunQuery(query);

                var relationships = new List<T>();

                foreach (var record in records)
                {
                    var t = ParseAs<T>(
                            record["r"]
                            .As<IRelationship>());

                    if (t != null)
                        relationships.Add(t);
                }

                return relationships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error retrieving relations of type {nameof(T)}!");
                return Enumerable.Empty<T>();
            }
        }

        /// <summary>
        /// Tries to create the provided entity
        /// </summary>
        /// <typeparam name="T">Has to be of type <see cref="Entity"/> and the result
        /// will be parsed as this type</typeparam>
        /// <param name="obj">The object that will be created</param>
        /// <returns>If the creation process was successful</returns>
        public bool TryCreate<T>(ref T obj) where T : Entity
        {
            IEntity? result = null;

            try
            {
                // Check that id is not Guid.Empty
                if (obj.Id == Guid.Empty)
                    obj.Id = Guid.NewGuid();

                // Get parameters
                var parameters = GetParameters(obj, "n", out string setString);

                Query query;

                // Relationship
                Type type = typeof(T);
                if (obj is Relation rel)
                {
                    // Create query
                    query = new(
                        $"MATCH (a), (b) WHERE a.id=\"{rel.From}\" AND b.id=\"{rel.To}\" " +
                        $"CREATE (a)-[n:{rel.RelationType}]->(b) " +
                        $"{setString} RETURN n;");
                }
                // Node
                else
                {
                    // Create query
                    query = new($"CREATE (n: {type.Name}) {setString} RETURN n;");
                }

                // Add parameters
                foreach (var p in parameters)
                {
                    query.Parameters.Add(p);
                }

                // Get result
                result = Database
                    .RunQuery(query)
                    .First()["n"]
                    .As<IEntity>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error creating {nameof(T)} in the database!");
            }

            // Check return status and reassign entity if possible
            var returnEntity = ParseAs<T>(result);
            if (returnEntity != null)
            {
                obj = returnEntity;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to update the provided entity.
        /// <br/><br/><b>Caution!</b> If a relation type or an entity
        /// id must be updated, create a new relation/entity instead!
        /// </summary>
        /// <typeparam name="T">Has to be of type <see cref="Entity"/></typeparam>
        /// <param name="obj">The object that will be updated in the database</param>
        /// <returns>If the update operation was successful</returns>
        public bool TrySet<T>(ref T obj) where T : Entity
        {
            try
            {
                // Get parameters
                var parameters = GetParameters(obj, "a", out string setString);

                // Create query
                Query query;

                if (obj is Relation rel)
                {
                    // Check if From or To have changed
                    if (Get<T>(obj.Id) is not Relation oldRelation)
                        return false;

                    if (oldRelation.From != rel.From ||
                        oldRelation.To != rel.To)
                    {
                        // Delete old relation and create new
                        if (!TryDelete<T>(obj.Id) || !TryCreate(ref obj))
                            return false;
                        return true;
                    }

                    query = new($"MATCH ()-[a]-() WHERE a.id=\"{rel.Id}\" {setString};");
                }
                else
                {
                    query = new($"MATCH (a) WHERE a.id=\"{obj.Id}\" {setString};");
                }

                // Add parameters
                foreach (var p in parameters)
                {
                    query.Parameters.Add(p);
                }

                Database.RunQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error setting the properties of {nameof(T)} in the database!");
                return false;
            }
        }

        /// <summary>
        /// Tries to delete the entity with the provided id
        /// </summary>
        /// <typeparam name="T">Has to be of type <see cref="Entity"/></typeparam>
        /// <param name="id">The id of the entity that will be deleted</param>
        /// <returns>If the delete operation was successful</returns>
        public bool TryDelete<T>(Guid id, bool cascade = false) where T : Entity
        {
            try
            {
                Query query;
                if (IsRelation(typeof(T)))
                {
                    query = new($"MATCH ()-[a]-() WHERE a.id=\"{id}\" DETACH DELETE a;");
                }
                else
                {
                    string qs = "";

                    if (cascade)
                    {
                        qs = $"MATCH (a)-[*]->(b) WHERE a.id=\"{ id}\" DETACH DELETE(b);";

                        Database.RunQuery(new(qs));
                    }

                    qs = $"MATCH (a) WHERE a.id=\"{ id}\" DETACH DELETE a;";

                    query = new(qs);
                }

                // Delete
                Database.RunQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error deleting {nameof(T)} with id {id} in the database!");
                return false;
            }
        }

        public bool TryBulkDelete<T>(Guid[] ids, bool cascade = false) where T : Entity
        {
            if (!ids.Any())
                return true;

            try
            {
                string queryString;
                bool isRelation = IsRelation(typeof(T));
                if (isRelation)
                {
                    queryString = $"MATCH ()-[a]-() WHERE ";
                }
                else
                {
                    if (cascade)
                    {
                        queryString = $"MATCH (a)-[*]->(b) WHERE ";
                    }
                    else
                    {
                        queryString = $"MATCH (a) WHERE ";
                    }
                }

                // Add ids
                for (int i = 0; i < ids.Length; i++)
                {
                    queryString += $" a.id=\"{ids[i]}\" ";
                    if (i < ids.Length - 1)
                        queryString += " OR ";
                }

                if (cascade && !isRelation)
                    queryString += " DETACH DELETE a, b;";
                else
                    queryString += " DETACH DELETE a;";

                Query query = new(queryString);

                // Delete
                Database.RunQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error deleting {nameof(T)} in bulk in the database!");
                return false;
            }
        }

        public bool TryBulkDeleteRelations<T>(Guid[] fromIds, Guid[] toIds) where T : Relation
        {
            if (!fromIds.Any() || !toIds.Any())
                return true;

            try
            {
                Type type = typeof(T);
                // Check if there is an attrbiute for the type
                string? relName = Relation.GetRelationTypeName<T>();

                string queryString = $"MATCH (a)-[r:{relName ?? ""}]->(b) WHERE ";

                // Add ids
                queryString += " ( ";
                for (int i = 0; i < fromIds.Length; i++)
                {
                    queryString += $" a.id=\"{fromIds[i]}\" ";
                    if (i < fromIds.Length - 1)
                        queryString += " OR ";
                }
                queryString += " ) AND ( ";
                for (int i = 0; i < toIds.Length; i++)
                {
                    queryString += $" b.id=\"{toIds[i]}\" ";
                    if (i < toIds.Length - 1)
                        queryString += " OR ";
                }
                queryString += " ) DETACH DELETE r;";

                Query query = new(queryString);

                // Delete
                Database.RunQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error deleting {nameof(T)} in bulk in the database!");
                return false;
            }
        }


        #region Helper Methods

        private static bool IsRelation<T>(T obj) where T : Entity
        {
            return IsRelation(obj.GetType());
        }
        private static bool IsRelation(Type t)
        {
            return t.GetProperty("RelationType") != null;
        }
        private IDictionary<string, object> GetParameters<T>(T obj, string objQueryName, out string setString) where T : Entity
        {
            setString = "SET ";
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            bool first = true;

            // Try to get mapping from cache
            Type type = typeof(T);
            var mapExists = _propertyFirstMapping.TryGetValue(type, out var mapping);

            if (!mapExists || mapping == null)
            {
                mapping = GetPropertyFirstMapping(type);
                _propertyFirstMapping.Add(type, mapping);
            }

            if (!_typeProperties.TryGetValue(type, out var properties) || properties == null)
            {
                properties = obj.GetType().GetProperties();
                _typeProperties.Add(type, properties);
            }

            foreach (var property in properties)
            {
                if (mapping.TryGetValue(property.Name, out string? databaseName) && databaseName != null)
                {
                    object? value = property.GetValue(obj);
                    if (value == null)
                        continue;

                    if (property.PropertyType.IsEnum)
                    {
                        value = (int)value;
                    }
                    else if (value is float f)
                    {
                        value = Convert.ToDouble(f);
                    }
                    else if (value is int i)
                    {
                        value = Convert.ToInt64(i);
                    }
                    else if (value is Guid guid)
                    {
                        value = value.ToString();
                    }
                    else if (value is not string and not long and not double)
                    {
                        value = JsonConvert.SerializeObject(value);
                    }

                    if (first) { first = false; }
                    else { setString += ","; }

                    var key = $"p_{databaseName}";
                    parameters.Add(key, value);
                    setString += $" {objQueryName}.{databaseName}=${key} ";
                }
            }

            if (setString.Length == 4)
                setString = "";

            return parameters;
        }

        public object? ParseAs(IEntity? entity, Type type)
        {
            if (entity == null)
                return null;

            try
            {
                // Instantiate
                object? obj = Activator.CreateInstance(type);

                if (obj == null)
                    return null;

                // Change type
                obj = Convert.ChangeType(obj, type);

                // Set relationship info
                var relProperty = typeof(Relation).GetProperty("RelationType");
                if (relProperty != null && entity is IRelationship rel)
                {
                    relProperty.SetValue(obj, rel.Type);
                }

                // Get custom type mapping (preferable from cache)
                if (!_databaseFirstMapping.TryGetValue(type, out var mapping) || mapping == null)
                {
                    mapping = GetDatabaseFirstMapping(type);
                    _databaseFirstMapping.Add(type, mapping);
                }

                // Set properties
                foreach (var property in entity.Properties)
                {
                    object? value = property.Value;
                    PropertyInfo? propInfo = mapping[property.Key];

                    if (propInfo == null)
                    {
                        _logger.LogWarning(
                            new NullReferenceException(),
                            $"The property {property.Key} was not found on the object {type.Name}!");
                        continue;
                    }

                    try
                    {
                        var propType = propInfo.PropertyType;

                        if (propType == typeof(float) && value.GetType() == typeof(double))
                        {
                            propInfo.SetValue(obj, Convert.ToSingle(value));
                        }
                        else if (propType == typeof(int) && value.GetType() == typeof(long))
                        {
                            propInfo.SetValue(obj, Convert.ToInt32(value));
                        }
                        else if (propType.IsEnum)
                        {
                            value = Convert.ToInt32(value);
                            propInfo.SetValue(obj, value);
                        }
                        else if (value is string str)
                        {
                            // Is really string
                            if (propType == typeof(string))
                            {
                                propInfo.SetValue(obj, str);
                            }
                            else if (propType == typeof(Guid))
                            {
                                propInfo.SetValue(obj, Guid.Parse(str));
                            }
                            // Is not really string (deserialize)
                            else
                            {
                                value = JsonConvert.DeserializeObject(str, propInfo.PropertyType);
                                propInfo.SetValue(obj, value);
                            }
                        }
                        else
                        {
                            propInfo.SetValue(obj, value);
                        }
                    }
                    catch
                    {
                        _logger.LogError($"There was an error parsing the property {propInfo.Name} from {property.Key}!");
                    }
                }

                return obj;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error parsing the entity of type {type.Name} from the database!");
            }

            return default;
        }
        public T? ParseAs<T>(IEntity? entity) where T : Entity
        {
            return ParseAs(entity, typeof(T)) as T;
        }

        private readonly Dictionary<Type, Dictionary<string, string>> _propertyFirstMapping = new();
        private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _databaseFirstMapping = new();
        private readonly Dictionary<Type, PropertyInfo[]> _typeProperties = new();
        private static Dictionary<string, PropertyInfo> GetDatabaseFirstMapping(Type type)
        {
            Dictionary<string, PropertyInfo> mapping = new();

            foreach (var property in type.GetProperties())
            {
                DatabasePropertyNameAttribute[] attributes =
                    (DatabasePropertyNameAttribute[])
                    property.GetCustomAttributes(
                        typeof(DatabasePropertyNameAttribute),
                        true);

                if (attributes.FirstOrDefault() is DatabasePropertyNameAttribute attr)
                {
                    if (attr.PropertyName is string key)
                        mapping.Add(key, property);
                }
            }

            return mapping;
        }
        private static Dictionary<string, string> GetPropertyFirstMapping(Type type)
        {
            Dictionary<string, string> mapping = new();

            foreach (var property in type.GetProperties())
            {
                DatabasePropertyNameAttribute[] attributes =
                    (DatabasePropertyNameAttribute[])
                    property.GetCustomAttributes(
                        typeof(DatabasePropertyNameAttribute),
                        true);

                if (attributes.FirstOrDefault() is DatabasePropertyNameAttribute attr)
                {
                    if (attr.PropertyName is string value)
                        mapping.Add(property.Name, value);
                }
            }

            return mapping;
        }

        #endregion
    }
}
