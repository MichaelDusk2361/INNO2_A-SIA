using A_SIA2WebAPI.Models.Attributes;
using System;

namespace A_SIA2WebAPI.Models
{
    /// <summary>
    /// The base entity of the models, every other model has to derive from it
    /// </summary>
    public abstract class Entity
    {
        [DatabasePropertyName(PropertyName = "id")]
        public Guid Id { get; set; }
    }
}
