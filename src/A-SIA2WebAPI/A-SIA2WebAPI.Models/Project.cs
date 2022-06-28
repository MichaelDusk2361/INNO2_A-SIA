using A_SIA2WebAPI.Models.Attributes;
using System;

namespace A_SIA2WebAPI.Models
{
    public class Project : Entity
    {
        [DatabasePropertyName(PropertyName = "name")]
        public string? Name { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Project project &&
                   Id == project.Id &&
                   Name == project.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }
}
