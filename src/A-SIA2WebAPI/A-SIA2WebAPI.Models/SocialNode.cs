using A_SIA2WebAPI.Models.Attributes;
using System;
using System.Collections.Generic;

namespace A_SIA2WebAPI.Models
{
    public abstract class SocialNode : Entity
    {
        [DatabasePropertyName(PropertyName = "name")]
        public string Name { get; set; } = "New Node";

        [DatabasePropertyName(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [DatabasePropertyName(PropertyName = "color")]
        public string? Color { get; set; } = "#FFFFFF";

        [DatabasePropertyName(PropertyName = "position_x")]
        public float PositionX { get; set; }

        [DatabasePropertyName(PropertyName = "position_y")]
        public float PositionY { get; set; }

        [DatabasePropertyName(PropertyName = "simulation_values")]
        public Dictionary<int, float> SimulationValues { get; set; } = new Dictionary<int, float>();

        private float reflection;
        [DatabasePropertyName(PropertyName = "reflection")]
        public float Reflection { get => reflection; set => reflection = Math.Clamp(value, 0, 1); }

        private float persistance;
        [DatabasePropertyName(PropertyName = "persistance")]
        public float Persistance { get => persistance; set => persistance = Math.Clamp(value, 0, 1); }
    }
}
