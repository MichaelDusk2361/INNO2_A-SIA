using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Models.Relations;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A_SIA2WebAPI.DAL.Test.Neo4J.Repositories
{
    public class Neo4JNetworkStructureRepositoryTest
    {
        private Neo4JEngine _engine;
        private Mock<ILogger<Neo4JEngine>> _engineLogger;
        private Mock<ILogger<Neo4JNetworkStructureRepository>> _repoLogger;
        private Neo4JNetworkStructureRepository _repo;

        [SetUp]
        public void Setup()
        {
            _engineLogger = new();
            _engine = new(new Neo4JDatabase(), _engineLogger.Object);

            _repoLogger = new();
            _repo = new(_engine, _repoLogger.Object);
        }

        #region Get

        [Test]
        public void GetSinglePersonTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person = new()
            {
                Name = "New Person",
                Description = "This is a new Person!",
                Color = "#FFFFFF",
                Persistance = 2,
                PositionX = 3.214f,
                PositionY = -5.224f,
                Reflection = 2,
                SimulationValues = new()
            };
            if (!_engine.TryCreate(ref person))
                Assert.Fail("Error creating person");

            var containsRel = new NetworkContainsRelation()
            {
                From = network.Id,
                To = person.Id,
            };
            if (!_engine.TryCreate(ref containsRel))
                Assert.Fail("Error creating NETWORK_CONTAINS relationship");

            // Act - Parse structure
            NetworkStructure? networkStructure = _repo.Get(network.Id);

            // Cleanup
            _engine.TryDelete<Person>(person.Id);
            _engine.TryDelete<Network>(network.Id);

            // Assert
            Assert.NotNull(networkStructure);
            Assert.AreEqual(1, networkStructure?.People.Count);
            Assert.AreEqual(0, networkStructure?.Groups.Count);
            Assert.AreEqual(0, networkStructure?.InfluenceRelations.Count);
        }

        [Test]
        public void GetMultiplePersonRelationshipsTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Assert.NotNull(network);

            List<Person> people = new();
            for (int i = 0; i < 10; i++)
            {
                Person p = new()
                {
                    Name = $"New Person {i}",
                    Description = "This is a new Person!",
                    Color = "#FFFFFF",
                    Persistance = 2,
                    PositionX = 3.214f,
                    PositionY = -5.224f,
                    Reflection = 2,
                    SimulationValues = new()
                };
                if (!_engine.TryCreate(ref p))
                    Assert.Fail("Error creating person");

                people.Add(p);

                // Add person to network
                var rel = new NetworkContainsRelation()
                {
                    From = network.Id,
                    To = p.Id,
                };
                if (!_engine.TryCreate(ref rel))
                    Assert.Fail("Error creating NETWORK_CONTAINS relationship");
            }

            List<InfluencesRelation?> relations = new();
            for (int i = 0; i < people.Count - 1; i++)
            {
                Person? p1 = people[i];
                Person? p2 = people[i + 1];
                if (p1 != null && p2 != null)
                {
                    InfluencesRelation rel = new()
                    {
                        From = p1.Id,
                        To = p2.Id,
                        Influence = 2,
                        Interval = 1,
                        Offset = 5
                    };
                    if (!_engine.TryCreate(ref rel))
                        Assert.Fail("Error creating NETWORK_CONTAINS relationship");

                    relations.Add(rel);
                }
            }
            // Act - Parse structure
            NetworkStructure? networkStructure = _repo.Get(network.Id);

            // Cleanup
            _engine.TryDelete<Network>(network.Id);
            foreach (var p in people)
            {
                _engine.TryDelete<Person>(p.Id);
            }
            foreach (var r in relations)
            {
                _engine.TryDelete<Relation>(r.Id);
            }

            // Assert
            Assert.NotNull(networkStructure);
            Assert.AreEqual(people.Count, networkStructure?.People.Count);
            Assert.AreEqual(0, networkStructure?.Groups.Count);
            Assert.AreEqual(relations.Count, networkStructure?.InfluenceRelations.Count);
        }

        [Test]
        public void GetGroupsTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Assert.NotNull(network);

            List<Person> people = new();
            for (int i = 0; i < 10; i++)
            {
                Person p = new()
                {
                    Name = $"New Person {i}",
                    Description = "This is a new Person!",
                    Color = "#FFFFFF",
                    Persistance = 2,
                    PositionX = 3.214f,
                    PositionY = -5.224f,
                    Reflection = 2,
                    SimulationValues = new()
                };

                if (!_engine.TryCreate(ref p))
                    Assert.Fail("Error creating person");

                people.Add(p);

                // Add person to network
                var rel = new NetworkContainsRelation()
                {
                    From = network.Id,
                    To = p.Id,
                };
                if (!_engine.TryCreate(ref rel))
                    Assert.Fail("Error creating NETWORK_CONTAINS relationship");
            }

            List<Group?> groups = new();
            for (int i = 0; i < people.Count / 5; i++)
            {
                Group? g = new()
                {
                    Name = $"New Group {i}",
                    Description = "This is a new Group!",
                    Collapsed = true,
                    Color = "#FFFFFF",
                    Persistance = 2,
                    PositionX = 3.214f,
                    PositionY = -5.224f,
                    Reflection = 2,
                    SimulationValues = new(),
                };

                if (!_engine.TryCreate(ref g))
                    Assert.Fail("Error creating group");

                groups.Add(g);

                // Add person to network
                var rel = new NetworkContainsRelation()
                {
                    From = network.Id,
                    To = g.Id,
                };
                if (!_engine.TryCreate(ref rel))
                    Assert.Fail("Error creating NETWORK_CONTAINS relationship");
            }

            var rand = new Random();
            foreach (var p in people)
            {
                var rel = new GroupContainsRelation()
                {
                    From = groups[rand.Next(0, groups.Count)].Id,
                    To = p.Id,
                };

                if (!_engine.TryCreate(ref rel))
                    Assert.Fail("Error creating GROUP_CONTAINS relationship");
            }

            List<InfluencesRelation?> relations = new();
            for (int i = 0; i < people.Count - 1; i++)
            {
                Person? p1 = people[i];
                Person? p2 = people[i + 1];
                if (p1 != null && p2 != null)
                {
                    InfluencesRelation rel = new()
                    {
                        From = p1.Id,
                        To = p2.Id,
                        Influence = 2,
                        Interval = 1,
                        Offset = 5
                    };

                    if (!_engine.TryCreate(ref rel))
                        Assert.Fail("Error creating INFLUENCES relationship");
                    
                    relations.Add(rel);
                }
            }
            // Act - Parse structure
            NetworkStructure? networkStructure = _repo.Get(network.Id);

            // Cleanup
            _engine.TryDelete<Network>(network.Id);
            foreach (var p in people)
            {
                _engine.TryDelete<Person>(p.Id);
            }
            foreach (var r in relations)
            {
                _engine.TryDelete<Relation>(r.Id);
            }
            foreach (var g in groups)
            {
                _engine.TryDelete<Group>(g.Id);
            }

            // Assert
            Assert.NotNull(networkStructure);
            Assert.AreEqual(people.Count, networkStructure?.People.Count);
            Assert.AreEqual(people.Count / 5, networkStructure?.Groups.Count);
            Assert.AreEqual(relations.Count, networkStructure?.InfluenceRelations.Count);
        }

        #endregion

        #region Update

        [Test]
        public void CreatePersonTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person = new()
            {
                Id = Guid.Empty,
                Name = "New Person",
                Description = "This is a new Person!",
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new(),
                InfluenceRelations = new(),
                People = new()
                {
                    person
                }
            };

            // Act
            bool success = _repo.Update(ref networkStructure, network.Id);

            // Get network structure and compare
            var insertedPerson = _repo.Get(network.Id)?.People.FirstOrDefault();

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(insertedPerson);
            Assert.AreEqual(person.Id, insertedPerson?.Id);
        }

        [Test]
        public void UpdatePersonTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person = new()
            {
                Id = Guid.Empty,
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.Empty,
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new(),
                InfluenceRelations = new(),
                People = new()
                {
                    person,
                    person2
                }
            };
            _repo.Update(ref networkStructure, network.Id);

            // Act
            person2.Name = "Other Person!";
            bool success = _repo.Update(ref networkStructure, network.Id);

            // Get network structure and compare
            var insertedPerson = _repo.Get(network.Id)?.People
                .Where(p => p.Id == person2.Id).FirstOrDefault();

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(insertedPerson);
            Assert.AreEqual(person2.Name, insertedPerson?.Name);
        }

        [Test]
        public void DeletePersonTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person = new()
            {
                Id = Guid.Empty,
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.Empty,
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new(),
                InfluenceRelations = new(),
                People = new()
                {
                    person,
                    person2
                }
            };
            _repo.Update(ref networkStructure, network.Id);

            // Act
            networkStructure.People.RemoveAll(p => p.Id == person2.Id);
            bool success = _repo.Update(ref networkStructure, network.Id);

            var newCollection = _repo.Get(network.Id)?.People;

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.True(success);
            Assert.AreEqual(1, newCollection?.Count);
            Assert.AreEqual(person.Id, newCollection?.FirstOrDefault()?.Id);
        }

        [Test]
        public void CreateGroupTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };

            GroupEntry group = new()
            {
                Nodes = new()
                {
                    person.Id,
                    person2.Id
                },
                Group = new()
                {
                    Name = "New Group 1"
                }
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new()
                {
                    group
                },
                InfluenceRelations = new(),
                People = new()
                {
                    person,
                    person2
                }
            };

            // Act
            bool success = _repo.Update(ref networkStructure, network.Id);

            var insertedNetwork = _repo.Get(network.Id);

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.NotNull(insertedNetwork);
            Assert.AreEqual(1, insertedNetwork?.Groups.Count);
            Assert.AreEqual(2, insertedNetwork?.Groups.FirstOrDefault()?.Nodes.Count);
            Assert.AreEqual(group.Group.Name, insertedNetwork?.Groups.FirstOrDefault()?.Group?.Name);
            Assert.AreEqual(group.Group.Id, insertedNetwork?.Groups.FirstOrDefault()?.Group?.Id);
        }

        [Test]
        public void UpdateGroupPropertiesTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };

            GroupEntry group = new()
            {
                Nodes = new()
                {
                    person.Id,
                    person2.Id
                },
                Group = new()
                {
                    Name = "New Group 1"
                }
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new()
                {
                    group
                },
                InfluenceRelations = new(),
                People = new()
                {
                    person,
                    person2
                }
            };
            _repo.Update(ref networkStructure, network.Id);

            // Act
            group.Group.Name = "Other name for Group!";

            bool success = _repo.Update(ref networkStructure, network.Id);
            
            var insertedNetwork = _repo.Get(network.Id);

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.NotNull(insertedNetwork);
            Assert.AreEqual(1, insertedNetwork?.Groups.Count);
            Assert.AreEqual(2, insertedNetwork?.Groups.FirstOrDefault()?.Nodes.Count);
            Assert.AreEqual(group.Group.Name, insertedNetwork?.Groups.FirstOrDefault()?.Group?.Name);
            Assert.AreEqual(group.Group.Id, insertedNetwork?.Groups.FirstOrDefault()?.Group?.Id);
        }

        [Test]
        public void UpdateGroupNodesTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };
            Person person3 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 3",
                Description = "This is a new Person 3!",
            };
            Person person4 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 4",
                Description = "This is a new Person 4!",
            };

            GroupEntry group = new()
            {
                Nodes = new()
                {
                    person.Id,
                    person2.Id,
                    person3.Id,
                },
                Group = new()
                {
                    Name = "New Group 1"
                }
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new()
                {
                    group
                },
                InfluenceRelations = new(),
                People = new()
                {
                    person,
                    person2,
                    person3,
                }
            };
            _repo.Update(ref networkStructure, network.Id);

            // Act
            // Remove some people
            group.Nodes.RemoveAll(p => p == person2.Id || p == person3.Id);
            // Add person
            networkStructure.People.Add(person4);
            group.Nodes.Add(person4.Id);

            bool success = _repo.Update(ref networkStructure, network.Id);

            var insertedNetwork = _repo.Get(network.Id);

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.NotNull(insertedNetwork);
            Assert.AreEqual(1, insertedNetwork?.Groups.Count);
            Assert.AreEqual(group.Nodes.Count, insertedNetwork?.Groups.FirstOrDefault()?.Nodes.Count);
            Assert.NotNull(insertedNetwork?.Groups.FirstOrDefault()?.Nodes.FirstOrDefault());
            Assert.AreEqual(group.Nodes.Where(n => n == person4.Id).First(),
                insertedNetwork?.Groups.FirstOrDefault()?.Nodes.Where(n => n == person4.Id).First());
        }

        [Test]
        public void DeleteGroupTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };

            GroupEntry group = new()
            {
                Nodes = new()
                {
                    person.Id,
                    person2.Id
                },
                Group = new()
                {
                    Name = "New Group 1"
                }
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new()
                {
                    group
                },
                InfluenceRelations = new(),
                People = new()
                {
                    person,
                    person2
                }
            };
            _repo.Update(ref networkStructure, network.Id);

            // Act
            networkStructure.Groups.Clear();

            bool success = _repo.Update(ref networkStructure, network.Id);

            var newCollection = _repo.Get(network.Id)?.Groups;

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.NotNull(newCollection);
            Assert.AreEqual(0, newCollection?.Count);
        }

        [Test]
        public void CreateRelationshipTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person1 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };
            Person person3 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 3",
                Description = "This is a new Person! 3",
            };

            InfluencesRelation relation = new()
            {
                From = person1.Id,
                To = person2.Id,
                Influence = 3.45f,
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new(),
                InfluenceRelations = new()
                {
                    relation
                },
                People = new()
                {
                    person1,
                    person2,
                    person3,
                }
            };

            // Act
            bool success = _repo.Update(ref networkStructure, network.Id);

            // Get network structure and compare
            var insertedRelation = _repo.Get(network.Id)?.InfluenceRelations.FirstOrDefault();

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(insertedRelation);
            Assert.AreEqual(relation.Id, insertedRelation?.Id);
        }

        [Test]
        public void UpdateRelationshipPropertiesTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person1 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };
            Person person3 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 3",
                Description = "This is a new Person! 3",
            };

            InfluencesRelation relation = new()
            {
                From = person1.Id,
                To = person2.Id,
                Influence = 3.45f,
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new(),
                InfluenceRelations = new()
                {
                    relation
                },
                People = new()
                {
                    person1,
                    person2,
                    person3,
                }
            };
            _repo.Update(ref networkStructure, network.Id);

            // Act
            relation.Influence = -34.3f;

            bool success = _repo.Update(ref networkStructure, network.Id);

            // Get network structure and compare
            var insertedRelation = _repo.Get(network.Id)?.InfluenceRelations.FirstOrDefault();

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(insertedRelation);
            Assert.AreEqual(relation.Id, insertedRelation?.Id);
            Assert.AreEqual(relation.Influence, insertedRelation?.Influence);
        }

        [Test]
        public void UpdateRelationshipFromToTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person1 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };
            Person person3 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 3",
                Description = "This is a new Person! 3",
            };

            InfluencesRelation relation = new()
            {
                From = person1.Id,
                To = person2.Id,
                Influence = 3.45f,
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new(),
                InfluenceRelations = new()
                {
                    relation
                },
                People = new()
                {
                    person1,
                    person2,
                    person3,
                }
            };
            _repo.Update(ref networkStructure, network.Id);

            // Act
            relation.From = person3.Id;
            relation.To = person1.Id;

            bool success = _repo.Update(ref networkStructure, network.Id);

            // Get network structure and compare
            var insertedRelation = _repo.Get(network.Id)?.InfluenceRelations.FirstOrDefault();

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(insertedRelation);
            Assert.AreEqual(relation.Id, insertedRelation?.Id);
            Assert.AreEqual(relation.Influence, insertedRelation?.Influence);
            Assert.AreEqual(relation.From, insertedRelation?.From);
            Assert.AreEqual(relation.To, insertedRelation?.To);
        }

        [Test]
        public void DeleteRelationshipTest()
        {
            // Arrange
            Network network = new()
            {
                Name = "New Network"
            };
            if (!_engine.TryCreate(ref network))
                Assert.Fail("Error creating network");

            Person person1 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 1",
                Description = "This is a new Person 1!",
            };
            Person person2 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 2",
                Description = "This is a new Person 2!",
            };
            Person person3 = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Person 3",
                Description = "This is a new Person! 3",
            };

            InfluencesRelation relation = new()
            {
                From = person1.Id,
                To = person2.Id,
                Influence = 3.45f,
            };

            NetworkStructure networkStructure = new()
            {
                Groups = new(),
                InfluenceRelations = new()
                {
                    relation
                },
                People = new()
                {
                    person1,
                    person2,
                    person3,
                }
            };
            _repo.Update(ref networkStructure, network.Id);

            // Act
            networkStructure.InfluenceRelations.Clear();

            bool success = _repo.Update(ref networkStructure, network.Id);

            // Get network structure and compare
            var insertedRelation = _repo.Get(network.Id)?.InfluenceRelations.FirstOrDefault();

            // Cleanup
            _engine.TryDelete<Network>(network.Id, true);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNull(insertedRelation);
        }

        #endregion
    }
}
