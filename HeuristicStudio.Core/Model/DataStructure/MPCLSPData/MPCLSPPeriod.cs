using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure.MPCLSPData
{
    public struct PP
    {
        public MPCLSPPlant Plant;
        public MPCLSPProduct Product;
    }
    public class MPCLSPPeriod
    {
       
        public int UID { get; set; }

        private Dictionary<MPCLSPPlant, int> _Capacity = null;
        private Dictionary<PP, int> _demand = null;
        private Dictionary<PP, int> _stock = null;
        private Dictionary<PP, double> _stockCost = null;

        /// <summary>
        /// C_jt is the available capacity of production at plant j in period t
        /// </summary>
        public Dictionary<MPCLSPPlant, int> Capacity
        {
            get
            {
                return _Capacity;
            }

            set
            {
                _Capacity = value;
            }
        }

        /// <summary>
        /// d_ijt is the demand of item i at plant j in period t
        /// </summary>
        public Dictionary<PP, int> Demands
        {
            get
            {
                return _demand;
            }

            set
            {
                _demand = value;
            }
        }

        /// <summary>
        /// I_ijt is the quantity of item i storage at plant j at the end of the period t
        /// </summary>
        public Dictionary<PP, int> Stock
        {
            get
            {
                return _stock;
            }

            set
            {
                _stock = value;
            }
        }

        /// <summary>
        ///h_ijt unitary holding cost of product i at plant j at the end of period t 
        ///Note: The stock cost parameter is the unitary holding cost (represented by h_ijt). It is a constant value per unit of product per period.
        /// </summary>
        public Dictionary<PP, double> StockCost
        {
            get
            {
                return _stockCost;
            }

            set
            {
                _stockCost = value;
            }
        }

        private MPCLSPPeriod(MPCLSPPeriod instance)
        {
            UID = instance.UID;
            Capacity = instance.Capacity;
            Demands = new Dictionary<PP, int>(); instance.Demands.ToList().ForEach(p => 
            Demands.Add(new PP() { Product = p.Key.Product.Copy(), Plant = p.Key.Plant.Copy() }, p.Value));
            Stock = new Dictionary<PP, int>(); instance.Stock.ToList().ForEach(p =>
            Stock.Add(new PP() { Product = p.Key.Product.Copy(), Plant = p.Key.Plant.Copy() }, p.Value));
            StockCost = new Dictionary<PP, double>(); instance.StockCost.ToList().ForEach(p =>
            StockCost.Add(new PP() { Product = p.Key.Product.Copy(), Plant = p.Key.Plant.Copy() }, p.Value));
        }

        public MPCLSPPeriod(int uid,Dictionary<MPCLSPPlant, int> capacity)
        {
            UID = uid;
            Capacity = capacity;
            Capacity.ToList().ForEach(c => c.Key.Lines.ForEach(l => l.Capacity = c.Value));
            Demands = new Dictionary<PP, int>();
            Stock = new Dictionary<PP, int>();
            StockCost = new Dictionary<PP, double>();
        }

  

        public MPCLSPPeriod Copy()
        {
            return new MPCLSPPeriod(this);
        }
        
    }
}
