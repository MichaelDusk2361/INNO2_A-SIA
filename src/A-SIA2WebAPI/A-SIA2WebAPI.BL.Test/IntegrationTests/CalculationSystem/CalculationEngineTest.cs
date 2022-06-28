using A_SIA2WebAPI.BL.CalculationSystem;
using A_SIA2WebAPI.BL.PluginSystem;
using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.Test.IntegrationTests.CalculationSystem
{
    public class CalculationEngineTest
    {
        private CalculationEngine _calculationEngine;

        private Neo4JNetworkStructureRepository _networkStructureRepository;
        private Neo4JNetworkRepository _networkRepository;

        private Network _network;
        private NetworkStructure _networkStructure;

        [SetUp]
        public void SetUp()
        {
            Neo4JEngine engine = new(
                new Neo4JDatabase(),
                new Mock<ILogger<Neo4JEngine>>().Object);
            Mock<IPluginLoader<ICalculator>> pluginLoader = new();

            _calculationEngine = new CalculationEngine(
                pluginLoader.Object,
                engine);

            _networkStructureRepository = new(
                engine,
                new Mock<ILogger<Neo4JNetworkStructureRepository>>().Object);
            _networkRepository = new(
                engine,
                new Mock<ILogger<Neo4JNetworkRepository>>().Object);

            SetUpNetworkStructure();
        }

        // Setup test network
        public void SetUpNetworkStructure()
        {
            _network = new()
            {
                Id = Guid.NewGuid(),
                Name = "Test Network",
                Anonymous = false,
            };

            // Insert network
            Assert.IsTrue(
                _networkRepository.Insert(ref _network),
                "Could not insert network into database");

            _networkStructure = new();

            // Generate people
            Random random = new();

            for (int i = 0; i < 10; i++)
            {
                Person person = new()
                {
                    Id = Guid.NewGuid(),
                    Name = $"Person {i}",
                    Description = $"This is person {i}",
                    Reflection = (float)random.NextDouble(),
                    Persistance = (float)random.NextDouble(),
                };
                _networkStructure.People.Add(person);
            }

            // Insert network structure
                Assert.IsTrue(
                    _networkStructureRepository.Update(ref _networkStructure, _network.Id),
                    "Could not update network structure in database");
        }

        [Test]
        public void TestCarrier()
        {
            // Arrange
            Guid carrierId = _networkStructure.People[1].Id;

            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[0].Id,
                    To = _networkStructure.People[1].Id,
                });
            _networkStructure.InfluenceRelations.Add(
            new()
            {
                From = _networkStructure.People[1].Id,
                To = _networkStructure.People[2].Id,
            });
            Assert.IsTrue(
                _networkStructureRepository.Update(ref _networkStructure, _network.Id),
                "Could not update network structure in database");

            // Act
            var calculations =_calculationEngine.CalculateNetwork(_network.Id, _networkStructure, 3);
            var lastNetworkCalc = calculations.Last();

            Person carrier = lastNetworkCalc.People
                .Where(p => p.Roles.Contains(PersonRoles.Carrier)).FirstOrDefault();

            // Assert
            Assert.AreEqual(3, calculations.Count());
            Assert.IsNotNull(carrier);
            Assert.AreEqual(carrierId, carrier.Id);
            Assert.AreEqual(1, carrier.Roles.Where(t => t == PersonRoles.Carrier).Count());
        }

        [Test]
        public void TestIsolator()
        {
            // Arrange
            Guid isolatorId = _networkStructure.People[1].Id;

            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[0].Id,
                    To = _networkStructure.People[1].Id,
                });
            _networkStructure.InfluenceRelations.Add(
            new()
            {
                From = _networkStructure.People[2].Id,
                To = _networkStructure.People[1].Id,
            });
            Assert.IsTrue(
                _networkStructureRepository.Update(ref _networkStructure, _network.Id),
                "Could not update network structure in database");

            // Act
            var calculations = _calculationEngine.CalculateNetwork(_network.Id, _networkStructure, 3);
            var lastNetworkCalc = calculations.Last();

            Person isolator = lastNetworkCalc.People
                .Where(p => p.Roles.Contains(PersonRoles.Isolator)).FirstOrDefault();

            // Assert
            Assert.AreEqual(3, calculations.Count());
            Assert.IsNotNull(isolator);
            Assert.AreEqual(isolatorId, isolator.Id);
            Assert.AreEqual(1, isolator.Roles.Where(i => i == PersonRoles.Isolator).Count());
        }

        [Test]
        public void TestCutPoints()
        {
            // Arrange

            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[0].Id,
                    To = _networkStructure.People[1].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[1].Id,
                    To = _networkStructure.People[2].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[2].Id,
                    To = _networkStructure.People[3].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[3].Id,
                    To = _networkStructure.People[4].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[3].Id,
                    To = _networkStructure.People[5].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[4].Id,
                    To = _networkStructure.People[2].Id,
                });

            Assert.IsTrue(
                _networkStructureRepository.Update(ref _networkStructure, _network.Id),
                "Could not update network structure in database");

            // Act
            var calculations = _calculationEngine.CalculateNetwork(_network.Id, _networkStructure, 3);
            var lastNetworkCalc = calculations.Last();

            var cutpoints = lastNetworkCalc.People
                .Where(p => p.Roles.Contains(PersonRoles.CutPoint));

            // Assert
            Assert.AreEqual(3, calculations.Count());
            Assert.AreEqual(3, cutpoints.Count());
            Assert.IsTrue(cutpoints.Any(p => p.Id == _networkStructure.People[1].Id));
            Assert.IsTrue(cutpoints.Any(p => p.Id == _networkStructure.People[2].Id));
            Assert.IsTrue(cutpoints.Any(p => p.Id == _networkStructure.People[3].Id));
            Assert.AreEqual(1, _networkStructure.People[1].Roles.Where(r => r == PersonRoles.CutPoint).Count());
            Assert.AreEqual(1, _networkStructure.People[2].Roles.Where(r => r == PersonRoles.CutPoint).Count());
            Assert.AreEqual(1, _networkStructure.People[3].Roles.Where(r => r == PersonRoles.CutPoint).Count());
        }

        [Test]
        public void TestAlpha()
        {
            // Arrange

            // Person 7 has most outgoing
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[7].Id,
                    To = _networkStructure.People[1].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[7].Id,
                    To = _networkStructure.People[2].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[7].Id,
                    To = _networkStructure.People[3].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[7].Id,
                    To = _networkStructure.People[4].Id,
                });

            // Person 3 has second most outgoing
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[3].Id,
                    To = _networkStructure.People[0].Id,
                });

            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[3].Id,
                    To = _networkStructure.People[1].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[3].Id,
                    To = _networkStructure.People[2].Id,
                });

            // Person 2 and 4 have least outgoing
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[2].Id,
                    To = _networkStructure.People[4].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[4].Id,
                    To = _networkStructure.People[2].Id,
                });

            Assert.IsTrue(
                _networkStructureRepository.Update(ref _networkStructure, _network.Id),
                "Could not update network structure in database");

            // Act
            var calculations = _calculationEngine.CalculateNetwork(_network.Id, _networkStructure, 3);
            var lastNetworkCalc = calculations.Last();

            var alpha = lastNetworkCalc.People
                .Where(p => p.Roles.Contains(PersonRoles.Alpha));

            // Assert
            Assert.AreEqual(3, calculations.Count());
            Assert.AreEqual(1, alpha.Count());
            Assert.IsTrue(alpha.First()?.Id == _networkStructure.People[7].Id);
            Assert.AreEqual(1, _networkStructure.People[7].Roles.Where(r => r == PersonRoles.Alpha).Count());
        }

        [Test]
        public void TestOmega()
        {
            // Arrange

            // Person 7 has most incoming
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[1].Id,
                    To = _networkStructure.People[7].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[2].Id,
                    To = _networkStructure.People[7].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[3].Id,
                    To = _networkStructure.People[7].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[4].Id,
                    To = _networkStructure.People[7].Id,
                });

            // Person 3 has second most incoming
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[0].Id,
                    To = _networkStructure.People[3].Id,
                });

            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[1].Id,
                    To = _networkStructure.People[3].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[2].Id,
                    To = _networkStructure.People[3].Id,
                });

            // Person 2 and 4 have least incoming
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[2].Id,
                    To = _networkStructure.People[4].Id,
                });
            _networkStructure.InfluenceRelations.Add(
                new()
                {
                    From = _networkStructure.People[4].Id,
                    To = _networkStructure.People[2].Id,
                });

            Assert.IsTrue(
                _networkStructureRepository.Update(ref _networkStructure, _network.Id),
                "Could not update network structure in database");

            // Act
            var calculations = _calculationEngine.CalculateNetwork(_network.Id, _networkStructure, 3);
            var lastNetworkCalc = calculations.Last();

            var omegas = lastNetworkCalc.People
                .Where(p => p.Roles.Contains(PersonRoles.Omega));

            // Assert
            Assert.AreEqual(3, calculations.Count());
            Assert.AreEqual(1, omegas.Count());
            Assert.IsTrue(omegas.First()?.Id == _networkStructure.People[7].Id);
            Assert.AreEqual(1, _networkStructure.People[7].Roles.Where(r => r == PersonRoles.Omega).Count());
        }

        [TearDown]
        public void TearDown()
        {
            Assert.IsTrue(_networkRepository.Delete(_network.Id), "Could not delete network!");
        }
    }
}
