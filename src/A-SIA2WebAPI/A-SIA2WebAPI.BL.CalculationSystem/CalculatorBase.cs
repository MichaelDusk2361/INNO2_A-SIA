using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_SIA2WebAPI.BL.CalculationSystem
{
    public abstract class CalculatorBase : ICalculator
    {
        public bool AlwaysExecutes { get; set; }
        public int ExecutionTimes { get; set; }
        public int Priority { get; set; }
        protected INeo4JEngine Neo4JEngine { get; }

        protected CalculatorBase(INeo4JEngine neo4JEngine)
        {
            Neo4JEngine = neo4JEngine;
        }

        public abstract void Calculate(Guid networkId, ref NetworkStructure structure);
    }
}
