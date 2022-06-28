using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.CalculationSystem.Roles
{
    public abstract class RoleCalculatorBase : CalculatorBase
    {
        protected RoleCalculatorBase(INeo4JEngine neo4JEngine) : base(neo4JEngine)
        {
        }

        protected void ApplyRoles(string role, ref NetworkStructure networkStructure, List<Guid> guids)
        {
            // Apply / Remove role
            networkStructure.People.ForEach(person =>
            {
                if (guids.Contains(person.Id))
                {
                    if (!person.Roles.Contains(role))
                    {
                        person.Roles.Add(role);
                    }
                }
                else
                {
                    if (person.Roles.Contains(role))
                    {
                        person.Roles.Remove(role);
                    }
                }
            });
        }
    }
}
