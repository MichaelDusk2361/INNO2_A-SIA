using System;

namespace A_SIA2WebAPI.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DatabaseRelationTypeAttribute : Attribute
    {
        public string Type { get; set; }

        public DatabaseRelationTypeAttribute(string type)
        {
            Type = type;
        }
    }
}
