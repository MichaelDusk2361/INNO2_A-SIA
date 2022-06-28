using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace A_SIA2WebAPI.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntityAuthority
    {
        Owner,
        Edit,
        View
    }
}
