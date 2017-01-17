using HeuristicStudio.Core.Model.DataStructure.MPCLSPData;
using HeuristicStudio.Core.Model.MPCLSPData;
using HeuristicStudio.Core.Model.Problems;
using HeuristicStudio.Core.Service;
using System;
using System.Diagnostics;
using System.Linq;

namespace HeuristicStudio.Core.Model.MetaHeuristic.MPCLSPHeuristic
{
    public class MyIL : IMetaHeuristic<MPCLSPSolution>
    {
        Stopwatch sw = new Stopwatch();
        MPCLSP _problem = null;
        public TimeSpan Elapsed
        {
            get
            {
                return sw.Elapsed;
            }
        }

        public IProblem Problem
        {
            get
            {
                return _problem;
            }
        }

        public double Execute(IProblem problem)
        {
            _problem =(MPCLSP)problem;
        

            return _problem.Solution.Cost;
        }


        public double Fitness(MPCLSPSet dataset)
        {
            double score = 0.0;

            foreach (var product in dataset.Products)       //presented by i
            {
                foreach (var plant in dataset.Plants)       //presented by j
                {
                    foreach (var period in dataset.Periods) //presented by t
                    {
                        int I_ijt = period.Stock.Where(p => p.Key.Product.UID == product.UID && p.Key.Plant.UID == plant.UID).First().Value;        //Stock of product i at plant j at the end of period t
                        double h_ijt = period.StockCost.Where(p => p.Key.Product.UID == product.UID && p.Key.Plant.UID == plant.UID).First().Value; //Stock cost (unitary holding cost) of product i at plant j at the end of period t
                    }
                }
            }

            return score;
        }

    }
}
