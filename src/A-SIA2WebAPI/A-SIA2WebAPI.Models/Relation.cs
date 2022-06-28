using A_SIA2WebAPI.Models.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace A_SIA2WebAPI.Models
{
    public abstract class Relation : Entity
    {
        private string? relationType;
        public string? RelationType 
        {
            get
            {
                if (relationType == null)
                    relationType = GetRelationTypeName(GetType());
                return relationType;
            }
            private set 
            {
                relationType = value;
            } 
        }

        [DatabasePropertyName(PropertyName = "from")]
        public Guid From { get; set; }

        [DatabasePropertyName(PropertyName = "to")]
        public Guid To { get; set; }

        public Relation() : base()
        {
            RelationType = GetRelationTypeName(GetType());
        }

        public static string? GetRelationTypeName<T>() where T : Relation
        {
            return GetRelationTypeName(typeof(T));
        }

        public static string? GetRelationTypeName(Type type)
        {
            string? name = type.GetCustomAttribute<DatabaseRelationTypeAttribute>()?.Type;

            if (name == null)
            {

                name = string.Concat(
                    type.Name.Select(
                        (x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())
                    ).ToUpper()
                    .Replace("_RELATION", "");
            }
            return name;
        }
    }
}
