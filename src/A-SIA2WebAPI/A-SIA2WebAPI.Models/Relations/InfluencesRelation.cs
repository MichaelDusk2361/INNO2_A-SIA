using A_SIA2WebAPI.Models.Attributes;
using System;

namespace A_SIA2WebAPI.Models.Relations
{
    /// <summary>
    /// This relationship marks the influence between different social nodes
    /// </summary>
    [DatabaseRelationType("INFLUENCES")]
    public class InfluencesRelation : Relation
    {
        [DatabasePropertyName(PropertyName = "influence")]
        public float Influence { get; set; }

        [DatabasePropertyName(PropertyName = "interval")]
        public int Interval { get; set; }

        [DatabasePropertyName(PropertyName = "offset")]
        public int Offset { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is InfluencesRelation relation &&
                   Id == relation.Id &&
                   RelationType == relation.RelationType &&
                   From == relation.From &&
                   To == relation.To &&
                   Influence == relation.Influence &&
                   Interval == relation.Interval &&
                   Offset == relation.Offset;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, RelationType, From, To, Influence, Interval, Offset);
        }
    }
}
