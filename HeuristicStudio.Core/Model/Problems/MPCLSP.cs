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
        /// <param name="period_t">Period t</param>
        /// <param name="period_u">Period u</param>
        /// <returns>Total X_ijT</returns>
        public int ProductDemandOnPlantFromPeriod2Period(int plant_uid, int product_uid, int period_t,int period_u)
        {
            int d_iut = 0;
            for (; period_t <= period_u; period_t++)
            {
                DataSet.Periods[period_t - 1].Demands.Where(d => d.Key.Plant.UID == plant_uid && d.Key.Product.UID == product_uid).ToList().ForEach(d =>
                {
                    d_iut += d.Value;
                });
            }
            return d_iut;
        }

        /// <summary>
        /// Will return the setup cost for product i on plant
        /// </summary>
        ///<param name = "plant_uid" >Plant Id</param>
        /// <param name="product_uid">Product Id</param>
        /// <returns>SC_ij</returns>
        public double SetupCostOnPlant(int plant_uid, int product_uid)
        {
            MPCLSPPlant plant = DataSet.Plants.Find(p => p.UID == plant_uid);
            return plant.SetupCost[product_uid];
        }

        /// <summary>
        /// Will return the production cost for product i on plant
        /// </summary>
        ///<param name = "plant_uid" >Plant Id</param>
        /// <param name="product_uid">Product Id</param>
        /// <returns>PC_ij</returns>
        public double ProductionCostOnPlant(int plant_uid, int product_uid)
        {
            MPCLSPPlant plant = DataSet.Plants.Find(p => p.UID == plant_uid);
            return plant.ProductionCost[product_uid];
        }

        /// <summary>
        /// Will return the setup time for product i on plant
        /// </summary>
        ///<param name = "plant_uid" >Plant Id</param>
        /// <param name="product_uid">Product Id</param>
        /// <returns>ST_ij</returns>
        public double SetupTimeInPlant(int plant_uid, int product_uid)
        {
            MPCLSPPlant plant = DataSet.Plants.Find(p => p.UID == plant_uid);
            return plant.SetupTime[product_uid];
        }

        /// <summary>
        /// Will return the processing time for product i on plant
        /// </summary>
        ///<param name = "plant_uid" >Plant Id</param>
        /// <param name="product_uid">Product Id</param>
        /// <returns>PT_ij</returns>
        public double ProcessingTimeInPlant(int plant_uid, int product_uid)
        {
            MPCLSPPlant plant = DataSet.Plants.Find(p => p.UID == plant_uid);
            return plant.ProcessingTimes[product_uid];
        }

        /// <summary>!!!
        /// Will return the production cost for product i from period u to period t in all plants
        /// </summary>
        /// <param name="product_uid">Product Id</param>
        /// <param name="period_t">Period u</param>
        /// <param name="period_u">Period t</param>
        /// <returns>Phi_fmut</returns>
        public double ProductProductionCostFromPeriod2Period(int product_uid, int period_t, int period_u)
        {
            double tCost = 0.0;
            int D_iut = 0;
            MPCLSPProduct product = DataSet.Products.Find(p => p.UID == product_uid);
            for (;  period_t <= period_u; period_t++)
            {
                DataSet.Periods[period_t -1].Demands.Where(d => d.Key.Product.UID == product_uid).ToList().ForEach(d =>
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
        /// Product demand for product i from period u to period t
        /// </summary>
        /// <param name="product_uid">ProductID</param>
        /// <param name="period_t">Period t</param>
        /// <param name="period_u">Period u</param>
        /// <returns>D_iut</returns>
        public int ProductDemandFromPeriod2Period(int product_uid, int period_u, int period_t)
        {
            int D_iut = 0;
            for (; period_u <= period_t; period_u++)
            {
                DataSet.Periods[period_u - 1].Demands.Where(d => d.Key.Product.UID == product_uid).ToList().ForEach(d =>
                {
                    D_iut += d.Value;
                });
            }
            return D_iut;
        }

        /// <summary>
        /// Plant demand for plant j from period u to period t
        /// </summary>
        /// <param name="plant_uid">PlantID</param>
        /// <param name="period_t">Period t</param>
        /// <param name="period_u">Period u</param>
        /// <returns>PD_jut</returns>
        public int PlantDemandFromPeriod2Period(int plant_uid, int period_t, int period_u)
        {
            int PD_jut = 0;
            for (; period_t <= period_u; period_t++)
            {
                DataSet.Periods[period_t - 1].Demands.Where(d => d.Key.Plant.UID == plant_uid).ToList().ForEach(d =>
                {
                    PD_jut += d.Value;
                });
            }
            return PD_jut;
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
