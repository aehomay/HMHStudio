using HeuristicStudio.Core.Model.DataStructure.MPCLSPData;
using HeuristicStudio.Core.Model.MPCLSPData;
using HeuristicStudio.Core.Model.Problems;
using HeuristicStudio.Core.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HeuristicStudio.Core.Model.MetaHeuristic.MPCLSPHeuristic
{
    public class MyILS_MPCLSP : IMetaHeuristic<MPCLSPSolution>
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
            ((MPCLSPSolution)_problem.Solution).Dataset = _problem.DataSet;
            double score = Fitness((MPCLSPSolution)_problem.Solution);
            return _problem.Solution.Cost;
        }

        /// <summary>
        /// Objective function to calculate fitness for each solution
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public double Fitness(MPCLSPSolution solution)
        {
            MPCLSPSet dataset = solution.Dataset;
            double score = 0.0;
            double Tstock_cost = 0.0;                       //Total stock cost

            foreach (var product in dataset.Products)       //Presented by i
            {
                foreach (var plant in dataset.Plants)       //Presented by j
                {
                    foreach (var period in dataset.Periods) //Presented by t
                    {
                        int I_ijt = period.Stock.Where(p => p.Key.Product.UID == product.UID && p.Key.Plant.UID == plant.UID).First().Value;        //Stock of product i at plant j at the end of period t
                        double h_ijt = period.StockCost.Where(p => p.Key.Product.UID == product.UID && p.Key.Plant.UID == plant.UID).First().Value; //Stock cost (unitary holding cost) of product i at plant j at the end of period t
                        double Ttransfer_cost = 0.0;                    //Total transfer cost

                        foreach (var r_ijkt in plant.TransferCost)
                        {
                            int W_ijkt = 0;                             //Transfer quantity of product i from plant j to plant k in period t
                            KeyValuePair<PPP, int> transfer_quantity = period.TransferQuantity.ToList().Find(p =>
                            p.Key.Product.UID == product.UID && p.Key.PlantJ.UID == plant.UID && p.Key.PlantK.UID == r_ijkt.Key);
                            W_ijkt = transfer_quantity.Value;

                            Ttransfer_cost += r_ijkt.Value * W_ijkt;    //Unitary transfer cost of product i from plant j to plant k in period t * transfer quantity of product i from plant j to plant k in period t
                        }
                        Tstock_cost += Ttransfer_cost + (h_ijt * I_ijt);         //Stock cost of product i * Number of stoced + (Transfer cost in all periods * Number of transfers in all periods)
                    }
                }
            }

            double Tsetup_cost = 0.0;    //Total setup cost for all families that are in schedule
            foreach (var period in dataset.Periods)
            {
                foreach (var schedule in period.Schedules)
                {
                    Tsetup_cost += schedule.Item3.SetupCost[schedule.Item1.UID];
                }
            }
            score = Tsetup_cost + Tstock_cost; //Total setup cost + Total stock cost
            return score;
        }

    }
}
