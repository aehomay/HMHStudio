using HeuristicStudio.Core.Model.DataStructure.MPCLSPData;
using HeuristicStudio.Core.Model.MPCLSPData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.Problems
{
    public class MPCLSP : IProblem
    {

        public MPCLSP()
        {
            Solution = new MPCLSPSolution();
            DataSet = new MPCLSPSet();
        }

        /// <summary>
        /// Copy constructor for current object
        /// </summary>
        /// <param name="instance">Current object</param>
        private MPCLSP(MPCLSP instance)
        {
            Solution = instance.Solution;
            DataSet = instance.DataSet.Copy();
        }

        /// <summary>
        /// Will return the quantity of product i produced at plant j in all periods
        /// </summary>
        /// <param name="plant">Plant</param>
        /// <param name="product">Product</param>
        /// <returns>Total X_ijT</returns>
        public int DemandInPeriods(MPCLSPPlant plant, MPCLSPProduct product)
        {
            int demand = 0;
            DataSet.Periods.ForEach(p=> p.Demands.Where(item => item.Key.Plant.UID == plant.UID && item.Key.Product.UID == product.UID).ToList().ForEach(d => demand += d.Value));
            return demand;
        }

        /// <summary>
        /// Will return the quantity of product i produced at plant j in all periods
        /// </summary>
        /// <param name="plant_uid">Plant Id</param>
        /// <param name="product_uid">Product Id</param>
        /// <returns>Total X_ijT</returns>
        public int DemandInPeriods(int plant_uid, int product_uid)
        {
            int demand = 0;
            MPCLSPPlant plant = DataSet.Plants.Find(p => p.UID == plant_uid);
            MPCLSPProduct product = DataSet.Products.Find(p => p.UID == product_uid);
            DataSet.Periods.ForEach(p => p.Demands.Where(item => item.Key.Plant.UID == plant.UID && item.Key.Product.UID == product.UID).ToList().ForEach(d => demand += d.Value));
            return demand;
        }

        /// <summary>
        /// Will return the production cost for product i from period u to period t in all plants
        /// </summary>
        /// <param name="plant_uid">Plant Id</param>
        /// <param name="product_uid">Product Id</param>
        /// <returns>C_fmu</returns>
        public double ProductProductionCoseFromP2P(int product_uid, int period_u, int period_t)
        {
            double tCost = 0.0;
            int D_iut = 0;
            MPCLSPProduct product = DataSet.Products.Find(p => p.UID == product_uid);
            for (;  period_u <= period_t; period_u++)
            {
                DataSet.Periods[period_u -1].Demands.Where(d => d.Key.Product.UID == product_uid).ToList().ForEach(d =>
                {
                    double cost = 0.0;
                    cost = d.Key.Plant.ProductionCost[product_uid];
                    D_iut += d.Value;
                    tCost += cost * D_iut;
                });
            }
          
            return tCost;
        }

        /// <summary>
        /// Production quantity for all products at plant j from period u to period t
        /// </summary>
        /// <param name="plant_uid">PlantID</param>
        /// <param name="period_u">Period u</param>
        /// <param name="period_t">Period t</param>
        /// <returns>PQ_jut</returns>
        public int PlantProductionQuantityFromP2P(int plant_uid, int period_u, int period_t)
        {
            int quantity = 0;
            for (; period_u <= period_t; period_u++)
            {
                DataSet.Periods[period_u - 1].Demands.Where(d => d.Key.Plant.UID == plant_uid).ToList().ForEach(d =>
                {
                    quantity += d.Value;
                });
            }
            return quantity;
        }


        public ISolution Solution { get; set; }

        public MPCLSPSet DataSet { get; set; }
        public IProblem Clone()
        {
            throw new NotImplementedException();
        }


      

        public MPCLSP Copy()
        {
            return new MPCLSP(this);
        }
    }
}
