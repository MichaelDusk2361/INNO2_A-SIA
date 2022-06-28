using A_SIA2WebAPI.Models.Attributes;

namespace A_SIA2WebAPI.Models.Relations
{
    [DatabaseRelationType("INSTANCE_ROLE")]
    public class InstanceRoleRelation : Relation
    {
        [DatabasePropertyName(PropertyName = "authority")]
        public EntityAuthority Authority { get; set; } = EntityAuthority.View;
    }
}
