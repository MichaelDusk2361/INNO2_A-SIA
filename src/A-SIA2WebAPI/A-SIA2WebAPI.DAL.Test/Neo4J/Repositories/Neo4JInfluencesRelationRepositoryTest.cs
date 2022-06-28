﻿using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using Microsoft.Extensions.Logging;
using Moq;
using Neo4j.Driver;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Linq;

namespace A_SIA2WebAPI.DAL.Test.Neo4J.Repositories
{
    public class Neo4JInfluencesRelationRepositoryTest
    {
        private InfluencesRelation _entity;
        private InfluencesRelation _entity2;

        private INode _node1;
        private INode _node2;

        private Neo4JEngine _engine;
        private Neo4JRelationRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _engine = new(
                new Neo4JDatabase(),
                new Mock<ILogger<Neo4JEngine>>().Object);

            _repository = new(_engine, new Mock<ILogger<Neo4JRelationRepository>>().Object);

            // Insert nodes to connect to
            Query query = new(
                $"CREATE (n1: TestNode) SET n1.id=\"{Guid.NewGuid()}\" " +
                $"CREATE (n2: TestNode) SET n2.id=\"{Guid.NewGuid()}\" " +
                $"RETURN n1, n2;");

            var result = _engine.Database.RunQuery(query);

            _node1 = result[0]["n1"].As<INode>();
            _node2 = result[0]["n2"].As<INode>();

            _entity = new()
            {
                From = Guid.Parse(_node1.Properties["id"].ToString() ?? ""),
                To = Guid.Parse(_node2.Properties["id"].ToString() ?? ""),
                Influence = 3,
                Interval = 1,
                Offset = 0,
            };
        }

        [Test]
        public void InsertTest()
        {
            bool success = _repository.Insert(ref _entity);

            // Check that entity was inserted
            Assert.IsTrue(success);
            Assert.AreNotEqual(Guid.Empty, _entity.Id);

            // Retrieve inserted influences relation
            var inserted = _repository.Get<InfluencesRelation>(_entity.Id);
            if (inserted == null)
            {
                Assert.Fail("Get returned null, but should be influences relation");
                return;
            }

            Assert.AreEqual(
                JsonConvert.SerializeObject(_entity),
                JsonConvert.SerializeObject(inserted));
        }

        [Test]
        public void DeleteTest()
        {
            bool success = _repository.Insert(ref _entity);

            // Check that entity was inserted
            Assert.IsTrue(success);
            Assert.AreNotEqual(Guid.Empty, _entity.Id);

            success = _repository.Delete(_entity.Id);

            // Confirm removal of entity
            Assert.IsTrue(success);
            var inserted = _repository.Get<InfluencesRelation>(_entity.Id);
            Assert.IsNull(inserted);
        }

        [Test]
        public void UpdateTest()
        {
            // Arrage
            bool success = _repository.Insert(ref _entity);
            Assert.IsTrue(success);

            // Act
            _entity.Influence = -2.4f;
            _entity.Interval = 100;

            success = _repository.Update(ref _entity);
            Assert.IsTrue(success);

            // Assert
            var updated = _repository.Get<InfluencesRelation>(_entity.Id);

            Assert.AreEqual(
                JsonConvert.SerializeObject(_entity),
                JsonConvert.SerializeObject(updated));
        }

        [Test]
        public void GetAllTest()
        {
            // Insert entities
            bool success = _repository.Insert(ref _entity);

            Assert.IsTrue(success);
            Assert.AreNotEqual(Guid.Empty, _entity.Id);

            _entity2 = new()
            {
                From = Guid.Parse(_node1.Properties["id"].ToString() ?? ""),
                To = Guid.Parse(_node2.Properties["id"].ToString() ?? ""),
                Influence = 0.25f,
                Interval = 5,
                Offset = 3,
            };
            success = _repository.Insert(ref _entity2);

            Assert.IsTrue(success);
            Assert.AreNotEqual(Guid.Empty, _entity2.Id);

            // Get all
            var entities = _repository.GetAll<InfluencesRelation>().ToList();

            // Check length of collection
            Assert.IsTrue(entities.Count >= 2, "Entities are missing");
            // Check content of colelction
            Assert.AreEqual(
                JsonConvert.SerializeObject(_entity),
                JsonConvert.SerializeObject(entities.Where(e => e.Id == _entity.Id).FirstOrDefault()));
            Assert.AreEqual(
                JsonConvert.SerializeObject(_entity2),
                JsonConvert.SerializeObject(entities.Where(e => e.Id == _entity2.Id).FirstOrDefault()));
        }

        [TearDown]
        public void TearDown()
        {
            // Delete nodes
            Query query = new("MATCH (n) WHERE n.id=$id DETACH DELETE n;");

            if (_node1 != null)
            {
                query.Parameters["id"] = _node1.Properties["id"].ToString();
                _engine.Database.RunQuery(query);
            }
            if (_node2 != null)
            {
                query.Parameters["id"] = _node2.Properties["id"].ToString();
                _engine.Database.RunQuery(query);
            }
        }
    }
}
