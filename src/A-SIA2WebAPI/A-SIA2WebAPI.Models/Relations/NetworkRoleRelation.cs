using A_SIA2WebAPI.Models.Attributes;

namespace A_SIA2WebAPI.Models.Relations
{
    [DatabaseRelationType("NETWORK_ROLE")]
    public class NetworkRoleRelation : Relation
    {
        [DatabasePropertyName(PropertyName = "authority")]
        public EntityAuthority Authority { get; set; } = EntityAuthority.View;
    }
}
