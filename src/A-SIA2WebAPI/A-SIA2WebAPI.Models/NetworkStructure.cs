using A_SIA2WebAPI.Models.Relations;
using System;
using System.Collections.Generic;

namespace A_SIA2WebAPI.Models
{
    public class NetworkStructure
    {
        public List<Person> People { get; set; } = new();
        public List<InfluencesRelation> InfluenceRelations { get; set; } = new();
        public List<GroupEntry> Groups { get; set; } = new();

        public override bool Equals(object? obj)
        {
            return obj is NetworkStructure structure &&
                   EqualityComparer<List<Person>>.Default.Equals(People, structure.People) &&
                   EqualityComparer<List<InfluencesRelation>>.Default.Equals(InfluenceRelations, structure.InfluenceRelations) &&
                   EqualityComparer<List<GroupEntry>>.Default.Equals(Groups, structure.Groups);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(People, InfluenceRelations, Groups);
        }
    }

    public class GroupEntry
    {
        public Group? Group { get; set; }
        public List<Guid> Nodes { get; set; } = new();

        public override bool Equals(object? obj)
        {
            return obj is GroupEntry entry &&
                   EqualityComparer<Group?>.Default.Equals(Group, entry.Group) &&
                   EqualityComparer<List<Guid>>.Default.Equals(Nodes, entry.Nodes);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Group, Nodes);
        }
    }
}