using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using System;

namespace A_SIA2WebAPI.BL.CalculationSystem
{
    public interface ICalculator
    {
        public bool AlwaysExecutes { get; set; }
        public int ExecutionTimes { get; set; }
        public int Priority { get; set; }

        public void Calculate(Guid networkId, ref NetworkStructure structure);
    }
}

