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

            HeuristicPreProcessing();

            HeuristicScheduling();


            double score = Fitness((MPCLSPSolution)_problem.Solution);
            return _problem.Solution.Cost;
        }

        private void HeuristicScheduling()
        {
            MPCLSPSet dataset = ((MPCLSPSolution)_problem.Solution).Dataset;

            dataset.Periods.ForEach(period =>
            {
                Schedule schedule = null;
                MPCLSPPlant plant = null;
                int capacity = 0;
                period.BestPP.ForEach(bpp =>
                {
                    if (schedule == null)
                    {
                        schedule = new Schedule();
                        schedule.Dataset = dataset;
                        schedule.Period = period.UID;
                        if (plant == null)
                        {
                            plant = dataset.Plants.Find(p=> p.UID == bpp.Item1);
                            schedule.Plants.Add(plant.UID);
                            capacity = period.Capacity[plant.UID];
                        }
                    }

                    int pdp = period.ProductDemandInPeriod(bpp.Item2);
                    if (capacity > pdp)
                    {
                        plant.InstalledProducts.Add(bpp.Item2);
                        schedule.ProductionQuantity.Add(bpp.Item2, pdp);
                        capacity -= pdp;
                    }
                    else
                    {
                        plant.FullField = true;
                        plant.LeftCapacity = capacity;
                        plant = dataset.Plants.Find(new_plant => new_plant.UID == plant.BestNeighborPlant());
                        schedule.Plants.Add(plant.UID);
                        plant.InstalledProducts.Add(bpp.Item2);
                        capacity = period.Capacity[plant.UID];
                    }

                });
                ((MPCLSPSolution)_problem.Solution).Schedules.Add(period.UID, schedule);
            });
        }


        private void HeuristicPreProcessing()
        {
            MPCLSPSet dataset = ((MPCLSPSolution)_problem.Solution).Dataset;
            //Setup Cost x Setup Time
            SetupWeight(dataset);
            //(Production Cost x Production Time) / Product Demand)
            ProductionWeight(dataset);
            //setup weight + production weight / product demand on plant
            DemandWeight(dataset);
            //Finding the best plant to setup product
            BestPlantToSetupProduct(dataset);
        }

        /// <summary>
        /// Finding the best plant to setup product based on ratio between setup cost and setup time 
        /// </summary>
        /// <param name="dataset"></param>
        private static void BestPlantToSetupProduct(MPCLSPSet dataset)
        {
            dataset.Periods.ForEach(period =>
            {
                period.DemandWeghit = period.DemandWeghit.OrderBy(d => d.Item1).ToList();
                int best_plant = 0;
                dataset.Products.ForEach(product =>
                {
                    best_plant = period.BestPlantToSetup(product.UID);
                    period.BestPP.Add(new Tuple<int, int>(best_plant, product.UID));
                });

            });
        }

        /// <summary>
        /// Demand weight based on each period and plant and product (setup weight, production weight and demand on plant)
        /// </summary>
        /// <param name="dataset"></param>
        private static void DemandWeight(MPCLSPSet dataset)
        {
            
            dataset.Periods.ForEach(period =>
            {
                period.Demands.ToList().ForEach(demand =>
                {
                    MPCLSPProduct product = demand.Key.Product;
                    MPCLSPPlant plant = demand.Key.Plant;
                    int dv = demand.Value;
                    double weight = (plant.SetupWeghit.Find(p => p.Item2 == product.UID).Item1 + 
                    plant.ProductionWeghit.Find(p => p.Item2 == product.UID).Item1 ) / dv;
                    Tuple<double, PP> dw = new Tuple<double, PP>(weight, new PP()
                    { Plant = demand.Key.Plant, Product = demand.Key.Product });
                    period.DemandWeghit.Add(dw);
                });
            });
        }

        /// <summary>
        /// Calculating weight for each product based on production cost and processing time over different palnts
        /// </summary>
        /// <param name="dataset"></param>
        private void ProductionWeight(MPCLSPSet dataset)
        {
           
            dataset.Products.ForEach(product =>
            {
                ((MPCLSPSolution)_problem.Solution).Dataset.Plants.ForEach(plant =>
                {
                    plant.ProductionWeghit.Add(new Tuple<double, int>
                        (plant.ProductionCost[product.UID] * plant.ProcessingTimes[product.UID],
                        product.UID));
                });
            });
        }

        /// <summary>
        /// Calculating weight for each product based on setup cost and setup time over different palnts
        /// </summary>
        /// <param name="dataset"></param>
        private void SetupWeight(MPCLSPSet dataset)
        {
            dataset.Products.ForEach(product =>
            {
                ((MPCLSPSolution)_problem.Solution).Dataset.Plants.ForEach(plant =>
                {
                    plant.SetupWeghit.Add(new Tuple<double, int>
                        (plant.SetupCost[product.UID] * plant.SetupTime[product.UID],
                        product.UID));
                });
            });
        }

        /// <summary>
        /// Objective function to calculate fitness for each solution
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public double Fitness(MPCLSPSolution solution)
        {
            return ObjectiveI(solution);
        }

        private double ObjectiveI(MPCLSPSolution solution)
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
                        Tstock_cost += Ttransfer_cost + (h_ijt * I_ijt);      //Stock cost of product i * Number of stoced + (Transfer cost in all periods * Number of transfers in all periods)
                    }
                }
            }

            double Tsetup_cost = 0.0;    //Total setup cost for all families that are in schedule
            foreach (var schedule in solution.Schedules)
            {
                Tsetup_cost += schedule.Value.TotalCost;
            }
            score = Tsetup_cost + Tstock_cost; //Total setup cost + Total stock cost
            return score;
        }

        private double ObjectiveII(MPCLSPSolution solution)
        {
            MPCLSPSet dataset = solution.Dataset;
            double cost = 0.0;
            //foreach (var schadule in solution.Schedules)
            //{
            //    cost += dataset.Plants[schadule.Value.Plant].ProductionCost[schadule.Value.Product] * dataset.Periods[schadule.Value.Period].ProductionQuantity.ToList().Find
            //        (p => p.Key.Plant.UID == schadule.Value.Plant && p.Key.Product.UID == schadule.Value.Product).Value;
            //}
            return cost;
        }

        private double SubObjectiveI(MPCLSPSolution solution)
        {
            //MPCLSPSet dataset = solution.Dataset;
            double cost = 0.0;
            //foreach (var schadule in solution.Schedules)
            //{
            //    cost += schadule.Plant.SetupCost[schadule.Product.UID] * schadule.Period.ProductionQuantity.ToList().Find
            //        (p => p.Key.Plant.UID == schadule.Plant.UID && p.Key.Product.UID == schadule.Product.UID).Value;
            //}
            return cost;
        }

    }
}
