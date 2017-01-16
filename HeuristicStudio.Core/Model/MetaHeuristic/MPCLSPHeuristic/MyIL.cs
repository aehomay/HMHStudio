using HeuristicStudio.Core.Model.DataStructure.MPCLSPData;
using HeuristicStudio.Core.Model.MPCLSPData;
using HeuristicStudio.Core.Service;
using System;

namespace HeuristicStudio.Core.Model.MetaHeuristic.MPCLSPHeuristic
{
    public class MyIL : IMetaHeuristic<MPCLSPSolution>
    {
        public TimeSpan Elapsed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IProblem Problem
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double Execute(IProblem problem)
        {
            throw new NotImplementedException();
        }


        public double Fitness(MPCLSPSet dataset)
        {
            double score = 0.0;

            dataset.Periods.ForEach(period => 
            {
                foreach (var demand in period.Demands)
                {

                }
            });

            return score;
        }

    }
}
