using A_SIA2WebAPI.BL.CalculationSystem.Roles;
using A_SIA2WebAPI.BL.PluginSystem;
using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace A_SIA2WebAPI.BL.CalculationSystem
{
    public class CalculationEngine : ICalculationEngine
    {
        private readonly List<ICalculator> _calculators = new();

        public CalculationEngine(
            IPluginLoader<ICalculator> pluginLoader,
            INeo4JEngine neo4JEngine) 
        {
            // Load plugins
            string pluginFolder = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

            // Create plugin folder if it does not exist
            if (!Directory.Exists(pluginFolder))
            {
                Directory.CreateDirectory(pluginFolder);
            }
            // Load plugins
            else
            {
                // Add plugins to calculation engine
                AddCalculatorRange(pluginLoader.LoadPlugins(pluginFolder));
            }

            // Add built in calculators
            AddCalculator(new AlphaCalculator(neo4JEngine));
            AddCalculator(new OmegaCalculator(neo4JEngine));
            AddCalculator(new CarrierCalculator(neo4JEngine));
            AddCalculator(new IsolatorCalculator(neo4JEngine));
            AddCalculator(new CutPointsCalculator(neo4JEngine));
        }

        public void AddCalculator(ICalculator calculator)
        {
            _calculators.Add(calculator);
            _calculators.Sort(new CalculatorComparer());
        }
        public void AddCalculatorRange(IEnumerable<ICalculator> calculators)
        {
            _calculators.AddRange(calculators);
            _calculators.Sort(new CalculatorComparer());
        }

        public IEnumerable<NetworkStructure> CalculateNetwork(
            Guid networkId, NetworkStructure networkStructure, int end, params Type[] filter)
        {
            List<NetworkStructure> results = new();

            // Copy calc set
            List<ICalculator> calcSet = new(_calculators.ToArray());
            List<ICalculator> removables = new();
            // Remove filter
            calcSet.RemoveAll(c => filter.Contains(c.GetType()));

            // Calculate
            int iteration = 0;
            do
            {
                foreach (ICalculator calculator in calcSet)
                {
                    if (calculator.AlwaysExecutes || calculator.ExecutionTimes > iteration)
                        calculator.Calculate(networkId, ref networkStructure);
                    else removables.Add(calculator);
                }

                calcSet.RemoveAll(c => removables.Contains(c));
                removables.Clear();

                if (JsonConvert.DeserializeObject<NetworkStructure>(
                        JsonConvert.SerializeObject(networkStructure)) is NetworkStructure result)
                    results.Add(result);

                iteration++;
            }
            while (iteration < end);

            return results;
        }

        private class CalculatorComparer : IComparer<ICalculator>
        {
            public int Compare(ICalculator? x, ICalculator? y)
            {
                return x?.Priority == y?.Priority ? 0 : (x?.Priority < y?.Priority ? 1 : -1);
            }
        }
    }


}
