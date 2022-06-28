using A_SIA2WebAPI.Models;
using System;
using System.Collections.Generic;

namespace A_SIA2WebAPI.BL.CalculationSystem
{
    public interface ICalculationEngine
    {
        void AddCalculator(ICalculator calculator);
        void AddCalculatorRange(IEnumerable<ICalculator> calculators);
        IEnumerable<NetworkStructure> CalculateNetwork(Guid networkId, NetworkStructure networkStructure, int end, params Type[] filter);
    }
}