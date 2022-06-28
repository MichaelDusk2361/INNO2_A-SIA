using A_SIA2WebAPI.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.Models.Relations
{
    public class InstanceProjectNetworkRelations
    {
        [DatabasePropertyName(PropertyName = "instance_contains_relations")]
        public List<InstanceContainsRelation> InstanceContainsRelations { get; set; } = new List<InstanceContainsRelation>();
        [DatabasePropertyName(PropertyName = "project_contains_relations")]
        public List<ProjectContainsRelation> ProjectContainsRelations { get; set; } = new List<ProjectContainsRelation>();
    }
}
