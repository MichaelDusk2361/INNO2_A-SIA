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
    public class Neo4JInstanceRepositoryTest
    {
        private Instance _entity;
        private Instance _entity2;
        private Neo4JEngine _engine;
        private Neo4JInstanceRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _engine = new(
                new Neo4JDatabase(),
                new Mock<ILogger<Neo4JEngine>>().Object);

            _repository = new(_engine, new Mock<ILogger<Neo4JInstanceRepository>>().Object);

            _entity = new()
            {
                Name = "New Instance",
                Description = ""
            };
        }

        [Test]
        public void InsertTest()
        {
            bool success = _repository.Insert(ref _entity);

            // Check that entity was inserted
            Assert.IsTrue(success);
            Assert.AreNotEqual(Guid.Empty, _entity.Id);

            // Retrieve inserted user
            var inserted = _repository.Get(_entity.Id);
            if(inserted == null)
            {
                Assert.Fail("Get returned null, but should be project");
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
            var inserted = _repository.Get(_entity.Id);
            Assert.IsNull(inserted);
        }

        [Test]
        public void UpdateTest()
        {
            bool success = _repository.Insert(ref _entity);

            // Check that entity was inserted
            Assert.IsTrue(success);
            Assert.AreNotEqual(Guid.Empty, _entity.Id);

            _entity.Description = "New Description";

            // Update
            success = _repository.Update(ref _entity);

            // Confirm update status of entity
            Assert.IsTrue(success);
            var updated = _repository.Get(_entity.Id);

            Assert.AreEqual(_entity.Description, updated?.Description);
        }

        [Test]
        public void GetAllTest()
        {
            // Insert entities
            bool success  = _repository.Insert(ref _entity);

            Assert.IsTrue(success);
            Assert.AreNotEqual(Guid.Empty, _entity.Id);
            
            _entity2 = new()
            {
                Name = "Instance 2",
                Description = "This is a new Instance!",
            };
            success = _repository.Insert(ref _entity2);

            Assert.IsTrue(success);
            Assert.AreNotEqual(Guid.Empty, _entity2.Id);

            // Get all
            var entities = _repository.GetAll().ToList();

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
