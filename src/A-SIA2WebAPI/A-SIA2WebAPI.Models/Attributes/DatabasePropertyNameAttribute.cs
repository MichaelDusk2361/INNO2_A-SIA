using System;

namespace A_SIA2WebAPI.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class DatabasePropertyNameAttribute : Attribute
    {
        public string? PropertyName { get; set; }
    }
}
