﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure.MPCLSPData
{
    /// <summary>
    /// PP: Plant j and Product i
    /// </summary>
    public struct PP
    {
        /// <summary>
        /// Plant j
        /// </summary>
        public MPCLSPPlant Plant;
        /// <summary>
        /// Product i
        /// </summary>
        public MPCLSPProduct Product;
    }

    /// <summary>
    /// PPP: Plant j, Plant k and Product i
    /// </summary>
    public struct PPP
    {
        /// <summary>
        /// Plant j 
        /// </summary>
        public MPCLSPPlant PlantJ;
        /// <summary>
        /// Plant k
        /// </summary>
        public MPCLSPPlant PlantK;
        /// <summary>
        /// Product i
        /// </summary>
        public MPCLSPProduct Product;
    }

    public class MPCLSPPeriod
    {
       
        public int UID { get; set; }

        private Dictionary<MPCLSPPlant, int> _Capacity = null;
        private Dictionary<PP, int> _demand = null;
        private Dictionary<PPP, int> _transferQuantity = null;
        private Dictionary<PP, int> _productionQuantity = null;
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
        /// I_ijt is the quantity (stock) of item i storage at plant j at the end of the period t
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

        /// <summary>
        /// W_ijkt transfer quantity of product i from lant j to plant k in period t
        /// Note: The value of this parameter will be calculated during execution of the algorithm
        /// Note: The max(W_ijkt) less than equal of d_ijt - (X_imt + I_ijt)
        /// </summary>
        public Dictionary<PPP, int> TransferQuantity
        {
            get
            {
                return _transferQuantity;
            }

            set
            {
                _transferQuantity = value;
            }
        }

        /// <summary>
        /// X_imt production quantity of product i on filling line m (plant j) in in period t
        /// </summary>
        public Dictionary<PP, int> ProductionQuantity
        {
            get
            {
                return _productionQuantity;
            }

            set
            {
                _productionQuantity = value;
            }
        }


        /// <summary>
        /// Copy constructor for current object
        /// </summary>
        /// <param name="instance">Current object</param>
        private MPCLSPPeriod(MPCLSPPeriod instance)
        {
            UID = instance.UID;
            Capacity = instance.Capacity;

            Demands = new Dictionary<PP, int>(); instance.Demands.ToList().ForEach(p =>
            Demands.Add(new PP() { Product = p.Key.Product.Copy(), Plant = p.Key.Plant.Copy() }, p.Value));

            Stock = new Dictionary<PP, int>(); instance.Stock.ToList().ForEach(p =>
            Stock.Add(new PP() { Product = p.Key.Product.Copy(), Plant = p.Key.Plant.Copy() }, p.Value));

            ProductionQuantity = new Dictionary<PP, int>(); instance.ProductionQuantity.ToList().ForEach(p =>
            ProductionQuantity.Add(new PP() { Product = p.Key.Product.Copy(), Plant = p.Key.Plant.Copy() }, p.Value));

            StockCost = new Dictionary<PP, double>(); instance.StockCost.ToList().ForEach(p =>
            StockCost.Add(new PP() { Product = p.Key.Product.Copy(), Plant = p.Key.Plant.Copy() }, p.Value));

            TransferQuantity = new Dictionary<PPP, int>(); instance.TransferQuantity.ToList().ForEach(p =>
            TransferQuantity.Add(new PPP() { PlantJ = p.Key.PlantJ.Copy(), PlantK = p.Key.PlantK.Copy(),Product = p.Key.Product.Copy() }, p.Value));
           
        }

        public MPCLSPPeriod(int uid,Dictionary<MPCLSPPlant, int> capacity)
        {
            UID = uid;
            Capacity = capacity;
            Capacity.ToList().ForEach(c => c.Key.Lines.ForEach(l => l.Capacity = c.Value));
            Demands = new Dictionary<PP, int>();
            Stock = new Dictionary<PP, int>();
            StockCost = new Dictionary<PP, double>();
            TransferQuantity = new Dictionary<PPP, int>();
            ProductionQuantity = new Dictionary<PP, int>();
        }

  
        public int ProductProductionQuantity(int product_uid)
        {
            int pc = 0;
            ProductionQuantity.ToList().Where(key => key.Key.Product.UID == product_uid).ToList().ForEach(p => pc += p.Value);
            return pc;
        }


        public MPCLSPPeriod Copy()
        {
            return new MPCLSPPeriod(this);
        }
        
    }
}
