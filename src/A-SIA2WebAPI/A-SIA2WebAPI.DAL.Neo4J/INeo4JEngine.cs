using A_SIA2WebAPI.Models;
using Neo4j.Driver;
using System;
using System.Collections.Generic;

namespace A_SIA2WebAPI.DAL.Neo4J
{
    public interface INeo4JEngine
    {
        Neo4JDatabase Database { get; }

        T? Get<T>(Guid id) where T : Entity;
        IEnumerable<T> GetAll<T>() where T : Entity;
        IEnumerable<T> GetAllRelations<T>(string relationType) where T : Relation;
        IEnumerable<T> GetRelations<T>(Guid nodeId) where T : Relation;
        IEnumerable<T> GetRelations<T>(Guid fromId, Guid toId) where T : Relation;
        object? ParseAs(IEntity? entity, Type type);
        T? ParseAs<T>(IEntity? entity) where T : Entity;
        bool TryBulkDelete<T>(Guid[] ids, bool cascade = false) where T : Entity;
        bool TryBulkDeleteRelations<T>(Guid[] fromIds, Guid[] toIds) where T : Relation;
        bool TryCreate<T>(ref T obj) where T : Entity;
        bool TryDelete<T>(Guid id, bool cascade = false) where T : Entity;
        bool TrySet<T>(ref T obj) where T : Entity;
    }
}