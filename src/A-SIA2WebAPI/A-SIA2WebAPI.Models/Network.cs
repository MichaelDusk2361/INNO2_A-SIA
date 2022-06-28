using A_SIA2WebAPI.Models.Attributes;
using System;

namespace A_SIA2WebAPI.Models
{
    public class Network : Entity
    {
        [DatabasePropertyName(PropertyName = "name")]
        public string Name { get; set; } = "New Network";

        [DatabasePropertyName(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        // Network specific settings

        [DatabasePropertyName(PropertyName = "anonymous")]
        public bool Anonymous { get; set; } = false;

        [DatabasePropertyName(PropertyName = "label_type")]
        public int LabelType { get; set; } = 0;

        // Defaults for Relations

        [DatabasePropertyName(PropertyName = "default_interval")]
        public int DefaultInterval { get; set; } = 1;

        [DatabasePropertyName(PropertyName = "default_offset")]
        public int DefaultOffset { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "default_influence")]
        public int DefaultInfluence { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "min_interval")]
        public int MinInterval { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "min_offset")]
        public int MinOffset { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "min_influence")]
        public int MinInfluence { get; set; } = 0;

        // Defaults for social nodes

        [DatabasePropertyName(PropertyName = "default_nodevalue")]
        public int DefaultNodevalue { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "default_reflection")]
        public int DefaultReflection { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "default_persistance")]
        public int DefaultPersistance { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "min_nodevalue")]
        public int MinNodevalue { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "max_nodevalue")]
        public int MaxNodevalue { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "min_reflection")]
        public int MinReflection { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "max_reflection")]
        public int MaxReflection { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "min_persistance")]
        public int MinPersistance { get; set; } = 0;

        [DatabasePropertyName(PropertyName = "max_persistance")]
        public int MaxPersistance { get; set; } = 0;

        //public Dictionary<int, Operation> Operations;

        public override bool Equals(object? obj)
        {
            return obj is Network network &&
                   Id == network.Id &&
                   Name == network.Name &&
                   Description == network.Description &&
                   Anonymous == network.Anonymous &&
                   LabelType == network.LabelType &&
                   DefaultInterval == network.DefaultInterval &&
                   DefaultOffset == network.DefaultOffset &&
                   DefaultInfluence == network.DefaultInfluence &&
                   MinInterval == network.MinInterval &&
                   MinOffset == network.MinOffset &&
                   MinInfluence == network.MinInfluence &&
                   DefaultNodevalue == network.DefaultNodevalue &&
                   DefaultReflection == network.DefaultReflection &&
                   DefaultPersistance == network.DefaultPersistance &&
                   MinNodevalue == network.MinNodevalue &&
                   MaxNodevalue == network.MaxNodevalue &&
                   MinReflection == network.MinReflection &&
                   MaxReflection == network.MaxReflection &&
                   MinPersistance == network.MinPersistance &&
                   MaxPersistance == network.MaxPersistance;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Name);
            hash.Add(Description);
            hash.Add(Anonymous);
            hash.Add(LabelType);
            hash.Add(DefaultInterval);
            hash.Add(DefaultOffset);
            hash.Add(DefaultInfluence);
            hash.Add(MinInterval);
            hash.Add(MinOffset);
            hash.Add(MinInfluence);
            hash.Add(DefaultNodevalue);
            hash.Add(DefaultReflection);
            hash.Add(DefaultPersistance);
            hash.Add(MinNodevalue);
            hash.Add(MaxNodevalue);
            hash.Add(MinReflection);
            hash.Add(MaxReflection);
            hash.Add(MinPersistance);
            hash.Add(MaxPersistance);
            return hash.ToHashCode();
        }
    }
}
