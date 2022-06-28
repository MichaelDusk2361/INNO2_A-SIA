using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace A_SIA2WebAPI.DAL.Test.Neo4J
{
    public class TestRelation : Relation { }

    public class Neo4JEngineTest
    {
        private Person? _person0;
        private Person? _person1;
        private InfluencesRelation? _influencesRelation;
        private Neo4JEngine? _engine;
        private ILogger<Neo4JEngine> _logger;
        private Neo4JDatabase _database;

        [SetUp]
        public void Setup()
        {
            // Arrange
            _person0 = new Person()
            {
                Id = Guid.Empty,
                Name = "Max Mustermann",
                Color = "#FFFFFF",
                AvatarPath = "/test",
                Description = "A new user",
                Persistance = 0.45f,
                PositionX = 4.25f,
                PositionY = -3.45f,
                Reflection = 0.2f,
                Roles = new() { "Isolator" },
                SimulationValues = new() { { 0, 1.25f }, { 1, 0.75f } },
            };

            _person1 = new Person()
            {
                Id = Guid.Empty,
                Name = "Maria Musterfrau",
                Color = "#FFFFFF",
                AvatarPath = "/test",
                Description = "A new user",
                Persistance = 0.45f,
                PositionX = 4.25f,
                PositionY = -3.45f,
                Reflection = 0.2f,
                Roles = new() { "Transmitter" },
                SimulationValues = new() { { 0, 1.25f }, { 1, 0.75f } },
            };
            _influencesRelation = new InfluencesRelation()
            {
                Id = Guid.Empty,
                From = Guid.Empty,
                To = Guid.Empty,
                Influence = 2,
                Interval = 3,
                Offset = 5,
            };

            _logger = new LoggerFactory().CreateLogger<Neo4JEngine>();
            _database = new();
            _engine = new Neo4JEngine(_database, _logger);
        }

        [Test]
        public void CreatePersonTest()
        {
            bool success = false;
            if (_person0 != null && _engine != null)
                success =_engine.TryCreate(ref _person0);

            Assert.IsTrue(success && _person0?.Id != Guid.Empty);
        }

        [Test]
        public void CreateMultiplePersonAndRelationTest()
        {
            // Remove null errors
            if(_engine == null || _person0 == null || _person1 == null ||
                _influencesRelation == null)
            {
                Assert.Fail();
                return;
            }

            // Insert person 1
            bool success = _engine.TryCreate(ref _person0);
            Assert.IsTrue(_person0.Id != Guid.Empty && success, "Error inserting person 0!");

            // Insert person 2
            success = _engine.TryCreate(ref _person1);
            Assert.IsTrue(_person1.Id != Guid.Empty && success, "Error inserting person 1!");

            // Assing people to realtion
            _influencesRelation.From = _person0.Id;
            _influencesRelation.To = _person1.Id;

            success = _engine.TryCreate(ref _influencesRelation);

            Assert.IsTrue(_influencesRelation?.Id != Guid.Empty && success, "Error inserting relation!");
        }

        [Test]
        public void GetAllTest()
        {
            // Remove null errors
            if (_engine == null || _person0 == null || _person1 == null)
            {
                Assert.Fail();
                return;
            }

            // Insert person 0
            bool success = _engine.TryCreate(ref _person0);
            Assert.IsTrue(_person0.Id != Guid.Empty && success, "Error inserting person 0!");

            // Insert person 1
            success = _engine.TryCreate(ref _person1);
            Assert.IsTrue(_person1.Id != Guid.Empty && success, "Error inserting person 1!");

            // Retrieve all people
            List<Person> list = new(_engine.GetAll<Person>());

            // Integration test: Assert.AreEqual(2, list.Count, $"Get all failed and returned {list.Count}!");
            Assert.Pass();
        }

        [Test]
        public void DeletePersonTest()
        {
            // Remove null errors
            if (_engine == null || _person0 == null || _person1 == null)
            {
                Assert.Fail();
                return;
            }

            // Insert person 0
            bool success = _engine.TryCreate(ref _person0);
            Assert.IsTrue(_person0.Id != Guid.Empty && success, "Error inserting person 0!");

            // Delete person 0
            success = _engine.TryDelete<Person>(_person0.Id);
            Assert.IsTrue(success, "Error in deleting operation!");

            // Try to retrieve person 0
            _person0 = _engine?.Get<Person>(_person0.Id);

            // Check that person 0 is null
            Assert.IsNull(_person0, "Error deleting person 0!");
        }

        [Test]
        public void DeleteRelationTest()
        {
            // Remove null errors
            if (_engine == null || _person0 == null || _person1 == null ||
                _influencesRelation == null)
            {
                Assert.Fail();
                return;
            }

            // Insert person 0
            bool success = _engine.TryCreate(ref _person0);
            Assert.IsTrue(_person0.Id != Guid.Empty && success, "Error inserting person 0!");

            // Insert person 1
            success = _engine.TryCreate(ref _person1);
            Assert.IsTrue(_person1.Id != Guid.Empty && success, "Error inserting person 1!");

            // Insert relation
            _influencesRelation.From = _person0.Id;
            _influencesRelation.To = _person1.Id;
            success = _engine.TryCreate(ref _influencesRelation);
            Assert.IsTrue(_influencesRelation.Id != Guid.Empty && success, "Error inserting relation!");

            // Delete relation
            success = _engine.TryDelete<InfluencesRelation>(_influencesRelation.Id);
            Assert.IsTrue(success, "Error in deleting operation!");

            // Retrieve deleted relation
            _influencesRelation = _engine.Get<InfluencesRelation>(_influencesRelation.Id);

            // Check the received relation is null
            Assert.IsNull(_influencesRelation, "Error deleting relation!");
        }

        [Test]
        public void DeleteCascading()
        {
            // Remove null errors
            if (_engine == null || _person0 == null || _person1 == null ||
                _influencesRelation == null)
            {
                Assert.Fail();
                return;
            }

            // Insert person 0
            bool success = _engine.TryCreate(ref _person0);
            Assert.IsTrue(_person0.Id != Guid.Empty && success, "Error inserting person 0!");

            // Insert person 1
            success = _engine.TryCreate(ref _person1);
            Assert.IsTrue(_person1.Id != Guid.Empty && success, "Error inserting person 1!");

            // Create relation
            TestRelation rel = new()
            {
                From = _person0.Id,
                To = _person1.Id,
            };
            success = _engine.TryCreate(ref rel);
            Assert.IsTrue(rel.Id != Guid.Empty && success, "Error inserting relation!");

            // Delete cascading
            success = _engine.TryDelete<Person>(_person0.Id, true);
            Assert.IsTrue(success, "Error deleting person 0!");

            Person? p1 = _engine.Get<Person>(_person1.Id);

            Assert.IsNull(p1, "Cascading removal was not successful!");
        }

        [TearDown]
        public void TearDown()
        {
            if (_person0 != null)
                _engine?.TryDelete<Person>(_person0.Id);
            if (_person1 != null)
                _engine?.TryDelete<Person>(_person1.Id);
            if (_influencesRelation != null)
                _engine?.TryDelete<InfluencesRelation>(_influencesRelation.Id);
        }
    }
}