using A_SIA2WebAPI.Models.Attributes;

namespace A_SIA2WebAPI.Models.Relations
{
    [DatabaseRelationType("PROJECT_ROLE")]
    public class ProjectRoleRelation : Relation
    {
        [DatabasePropertyName(PropertyName = "authority")]
        public EntityAuthority Authority { get; set; } = EntityAuthority.View;
    }
}
