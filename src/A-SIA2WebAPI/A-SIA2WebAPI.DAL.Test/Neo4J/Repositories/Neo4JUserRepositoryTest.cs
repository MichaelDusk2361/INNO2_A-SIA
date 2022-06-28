using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Neo4j.Driver;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Linq;

namespace A_SIA2WebAPI.DAL.Test.Neo4J.Repositories
{
    public class Neo4JUserRepositoryTest
    {
        private User _entity;
        private User _entity2;
        private Neo4JEngine _engine;
        private Neo4JUserRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _engine = new(
                new Neo4JDatabase(),
                new Mock<ILogger<Neo4JEngine>>().Object);

            _repository = new(
                _engine,
                new Mock<ILogger<Neo4JUserRepository>>().Object);

            _entity = new()
            {
                 FirstName = "Test",
                 LastName = "User",
                 Email = $"{Guid.NewGuid()}",
                Hash = $"{Guid.NewGuid()}",
            };
        }

        [Test]
        public void InsertTest()
        {
            bool success = _repository.Insert(ref _entity);

            // Check that entity was inserted
            Assert.IsTrue(success, "Error inserting entity!");
            Assert.AreNotEqual(Guid.Empty, _entity.Id, "Entity id remains unchanged!");

            // Retrieve inserted user
            var inserted = _repository.Get(_entity.Id);
            if(inserted == null)
            {
                Assert.Fail("Get returned null, but should be user");
                return;
            }

            Assert.AreEqual(
                JsonConvert.SerializeObject(_entity),
                JsonConvert.SerializeObject(inserted),
                "Entity differs from inserted database entity!");
        }

        [Test]
        public void DeleteTest()
        {
            bool success = _repository.Insert(ref _entity);

            // Check that entity was inserted
            Assert.IsTrue(success, "Error inserting entity!");
            Assert.AreNotEqual(Guid.Empty, _entity.Id, "Entity id remains unchanged!");

            success = _repository.Delete(_entity.Id);

            // Confirm removal of entity
            Assert.IsTrue(success, "Error deleting entity!");
            var inserted = _repository.Get(_entity.Id);
            Assert.IsNull(inserted, "Entity has not been deleted!");
        }

        [Test]
        public void UpdateTest()
        {
            bool success = _repository.Insert(ref _entity);

            // Check that entity was inserted
            Assert.IsTrue(success, "Error inserting entity!");
            Assert.AreNotEqual(Guid.Empty, _entity.Id, "Entity id remains unchanged!");

            _entity.LastName = "New LastName";

            // Update
            success = _repository.Update(ref _entity);

            // Confirm update status of entity
            Assert.IsTrue(success, "Error while updating entity!");
            var updated = _repository.Get(_entity.Id);

            Assert.AreEqual(
                _entity.LastName,
                updated?.LastName,
                "Entity differs from updated database entity!");
        }

        [Test]
        public void GetAllTest()
        {
            // Insert entity 0
            bool success = _repository.Insert(ref _entity);
            Assert.IsTrue(success, "Error inserting entity!");
            Assert.AreNotEqual(Guid.Empty, _entity.Id, "Error entity id remains unchanged!");

            // Insert entity 2
            _entity2 = new()
            {
                FirstName = "Test2",
                LastName = "User2",
                Email = $"{Guid.NewGuid()}",
                Hash = $"{Guid.NewGuid()}",
            };
            success = _repository.Insert(ref _entity2);
            Assert.IsTrue(success, "Error inserting entity2!");
            Assert.AreNotEqual(Guid.Empty, _entity2.Id, "Error entity2 id remains unchanged!");

            // Get all entities
            var entities = _repository.GetAll().ToList();

            // Check length of collection
            Assert.IsTrue(entities.Count >= 2, "Entities are missing");
            // Check content of colelction
            Assert.AreEqual(
                JsonConvert.SerializeObject(_entity),
                JsonConvert.SerializeObject(entities.Where(e => e.Id == _entity.Id).FirstOrDefault()),
                "Entity differs from inserted entity");
            Assert.AreEqual(
                JsonConvert.SerializeObject(_entity2),
                JsonConvert.SerializeObject(entities.Where(e => e.Id == _entity2.Id).FirstOrDefault()),
                "Entity2 differs from inserted entity");
        }

        [TearDown]
        public void TearDown()
        {
            Query query = new("MATCH (n) WHERE n.id=$id DETACH DELETE n;");
            query.Parameters.Add("id", _entity.Id.ToString());

            _engine.Database.RunQuery(query);

            if(_entity2 != null)
            {
                query.Parameters["id"] = _entity2.Id.ToString();
                _engine.Database.RunQuery(query);
            }
        }
    }
}
