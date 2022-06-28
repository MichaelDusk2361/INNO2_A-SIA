using A_SIA2WebAPI.Models.Attributes;
using System;
using System.Collections.Generic;

namespace A_SIA2WebAPI.Models
{
    public class Person : SocialNode
    {
        public List<string> Roles { get; set; } = new List<string>();

        [DatabasePropertyName(PropertyName = "avatar_path")]
        public string AvatarPath { get; set; } = "";

        public override bool Equals(object? obj)
        {
            return obj is Person person &&
                   Id == person.Id &&
                   Name == person.Name &&
                   Description == person.Description &&
                   Color == person.Color &&
                   PositionX == person.PositionX &&
                   PositionY == person.PositionY &&
                   EqualityComparer<Dictionary<int, float>>.Default.Equals(SimulationValues, person.SimulationValues) &&
                   Reflection == person.Reflection &&
                   Persistance == person.Persistance &&
                   EqualityComparer<List<string>>.Default.Equals(Roles, person.Roles) &&
                   AvatarPath == person.AvatarPath;
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
            hash.Add(Roles);
            hash.Add(AvatarPath);
            return hash.ToHashCode();
        }
    }
}
