using System;
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

        private Dictionary<int, int> _Capacity = null;
        private Dictionary<Tuple<int, int, int>,int> _productionUpperBound = null;
        private Dictionary<PP, int> _demand = null;
        private Dictionary<PPP, int> _transferQuantity = null;
        private Dictionary<PP, int> _productionQuantity = null;
        private Dictionary<PP, int> _stock = null;
        private Dictionary<PP, double> _stockCost = null;
        private List<Tuple<double, PP>> _demandWeghit = new List<Tuple<double, PP>>();
        private List<Tuple<double, int>> _productionWeghit = new List<Tuple<double, int>>();
        private List<Tuple<int,int>> _bestPP = new List<Tuple<int, int>>();
        private List<MPCLSPPlant> _plants = null;

        /// <summary>
        /// C_jt is the available capacity of production at plant j in period t
        /// </summary>
        public Dictionary<int, int> Capacity
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
        /// b_imt Upper bound on production quantity of product i on filling line m in period t
        /// </summary>
        public Dictionary<Tuple<int, int, int>,int> ProductionUpperBound
        {
            get
            {
                return _productionUpperBound;
            }

            set
            {
                _productionUpperBound = value;
            }
        }

        /// <summary>
        /// This property will satisfy links between setup and productions which has been introduced by constrant number 4 in paper
        /// </summary>
        public bool CheckProductionUpperBound
        {
            get
            {
                bool r = true;
                foreach (var production in ProductionQuantity)
                {
                   var q =  ProductionUpperBound.ToList().Find(p => p.Key.Item1 == production.Key.Product.UID && p.Key.Item2 == production.Key.Plant.UID);
                    r = (production.Value - q.Value) <= 0;
                    if (r == false) return false;
                }
                return r;
            }
        }

        public List<Tuple<double, PP>> DemandWeghit
        {
            get
            {
                return _demandWeghit;
            }

            set
            {
                _demandWeghit = value;
            }
        }

        public List<MPCLSPPlant> Plants
        {
            get
            {
                return _plants;
            }

            set
            {
                _plants = value;
            }
        }

        public List<Tuple<int, int>> BestPP
        {
            get
            {
                return _bestPP;
            }

            set
            {
                _bestPP = value;
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
            ProductionUpperBound = instance.ProductionUpperBound;

            BestPP = instance.BestPP;

            Plants = new List<MPCLSPPlant>(); instance.Plants.ToList().ForEach(p =>
            Plants.Add(p.Copy()));

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

        public MPCLSPPeriod(int uid,Dictionary<int, int> capacity)
        {
            UID = uid;
            Capacity = capacity;
            //Capacity.ToList().ForEach(c => c.Key.Lines.ForEach(l => l.Capacity = c.Value));
            Demands = new Dictionary<PP, int>();
            Stock = new Dictionary<PP, int>();
            StockCost = new Dictionary<PP, double>();
            TransferQuantity = new Dictionary<PPP, int>();
            ProductionQuantity = new Dictionary<PP, int>();
            Plants = new List<MPCLSPPlant>();
         

        }

        /// <summary>
        /// Find the best plant to setup product
        /// </summary>
        /// <param name="product_uid"></param>
        /// <returns>Plant Id</returns>
        public int BestPlantToSetup(int product_uid)
        {
            int id = Plants[0].UID;
            for (int i = 1; i < Plants.Count; i++)
            {
                if (Plants[i].SetupWeghit.Find(p => p.Item2 == product_uid).Item1 < Plants[id].SetupWeghit.Find(p => p.Item2 == product_uid).Item1)
                    id = i;
            }
            
            return id;
        }

        public int ProductDemandInPeriod(int product_uid)
        {
            int pdp = 0;

            Demands.ToList().Where(d => d.Key.Product.UID == product_uid).ToList().ForEach(pd => pdp += pd.Value);

            return pdp;
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
