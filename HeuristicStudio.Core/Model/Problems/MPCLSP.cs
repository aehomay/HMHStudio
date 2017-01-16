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
            Solution = null;
            DataSet = new MPCLSPSet();
        }

        private MPCLSP(MPCLSP mPCLSP)
        {
            Solution = mPCLSP.Solution;
            DataSet = mPCLSP.DataSet.Copy();
        }

        /// <summary>
        /// Will return the quantity of product i produced at plant j in all periods
        /// </summary>
        /// <param name="plant">Plant</param>
        /// <param name="product">Product</param>
        /// <returns>Total X_ijT</returns>
        public int Demand(MPCLSPPlant plant, MPCLSPProduct product)
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
        public int Demand(int plant_uid, int product_uid)
        {
            int demand = 0;
            MPCLSPPlant plant = DataSet.Plants.Find(p => p.UID == plant_uid);
            MPCLSPProduct product = DataSet.Products.Find(p => p.UID == product_uid);
            DataSet.Periods.ForEach(p => p.Demands.Where(item => item.Key.Plant.UID == plant.UID && item.Key.Product.UID == product.UID).ToList().ForEach(d => demand += d.Value));
            return demand;
        }

        public ISolution Solution { get; set; }

        public MPCLSPSet DataSet { get; set; }
        public IProblem Clone()
        {
            throw new NotImplementedException();
        }

        public int Demand(int product, int plant, int period)
        {
            int demand = 0;

            return demand;
        }

      

        public MPCLSP Copy()
        {
            return new MPCLSP(this);
        }
    }
}
