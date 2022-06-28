using A_SIA2WebAPI.Models.Attributes;

namespace A_SIA2WebAPI.Models
{
    public class Instance : Entity
    {
        [DatabasePropertyName(PropertyName = "name")]
        public string? Name { get; set; }

        [DatabasePropertyName(PropertyName = "description")]
        public string? Description { get; set; }
    }
}
