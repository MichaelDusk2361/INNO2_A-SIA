using A_SIA2WebAPI.Models.Attributes;
using System;
using System.Collections.Generic;

namespace A_SIA2WebAPI.Models
{
    /// <summary>
    /// A group can be connected to diffrent other social nodes and
    /// can hold groups in itself
    /// </summary>
    public class Group : SocialNode
    {
        [DatabasePropertyName(PropertyName = "collapsed")]
        public bool Collapsed { get; set; } = false;

        public override bool Equals(object? obj)
        {
            return obj is Group group &&
                   Id == group.Id &&
                   Name == group.Name &&
                   Description == group.Description &&
                   Color == group.Color &&
                   PositionX == group.PositionX &&
                   PositionY == group.PositionY &&
                   EqualityComparer<Dictionary<int, float>>.Default.Equals(SimulationValues, group.SimulationValues) &&
                   Reflection == group.Reflection &&
                   Persistance == group.Persistance &&
                   Collapsed == group.Collapsed;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Name);
            hash.Add(Description);
            hash.Add(Color);
            hash.Add(PositionX);
            hash.Add(PositionY);
            hash.Add(SimulationValues);
            hash.Add(Reflection);
            hash.Add(Persistance);
            hash.Add(Collapsed);
            return hash.ToHashCode();
        }
    }
}
