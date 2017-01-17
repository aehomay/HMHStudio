using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure.MPCLSPData
{
    public class MPCLSPPlant
    {
        private int _uID;
        private Dictionary<int, double> _setupCost = null;
        private Dictionary<int, double> _setupTime = null;
        private Dictionary<int, double> _productionCost = null;
        private Dictionary<int, double> _processingTimes = null;
        private List<MPCLSPLine> _lines = null;
        private List<MPCLSPProduct> _products = null;
        private Dictionary<int, double> _transferCost = null;
        
        public int UID
        {
            get
            {
                return _uID;
            }
            set
            {
                _uID = value;
            }
        }

        public int Capacity
        {
            get
            {
                int capacity = 0;
                Lines.ForEach(l => capacity += l.Capacity);
                return capacity;
            }
        }

        /// <summary>
        /// Each product has its own production cost in each plant
        /// ProductID,Cost
        /// </summary>
        public Dictionary<int, double> ProductionCost
        {
            get
            {
                return _productionCost;
            }
            set
            {
                _processingTimes = value;
            }
        }

        /// <summary>
        /// c_fmt setup cost of family f on filing line m in period t
        /// Each family is considered as a product alone (or, in other words, each family has only one product)
        /// </summary>
        public Dictionary<int, double> SetupCost
        {
            get
            {
                return _setupCost;
            }
            set
            {
                _setupCost = value;
            }
        }

        /// <summary>
        /// Each product has its own processing time in each plant
        /// ProductID,Time
        /// </summary>
        public Dictionary<int, double> ProcessingTimes
        {
            get
            {
                return _processingTimes;
            }
            set
            {
                _processingTimes = value;
            }
        }

        /// <summary>
        /// Each product has its own setup time in each plant
        /// ProductID,Time
        /// </summary>
        public Dictionary<int, double> SetupTime
        {
            get
            {
                return _setupTime;
            }
            set
            {
                _setupTime = value;
            }
        }

        /// <summary>
        /// Set of lines in each plant
        ///Note:There is just one filling line per plant
        /// </summary>
        public List<MPCLSPLine> Lines
        {
            get
            {
                return _lines;
            }

            set
            {
                _lines = value;
            }
        }

        /// <summary>
        /// List of products can be producted in each plant
        /// </summary>
        public List<MPCLSPProduct> Products
        {
            get
            {
                return _products;
            }

            set
            {
                _products = value;
            }
        }

        /// <summary>
        /// r_ijkt unitary transfer cost of prduct i from plant j to plant k in period t
        /// </summary>
        public Dictionary<int, double> TransferCost
        {
            get
            {
                return _transferCost;
            }

            set
            {
                _transferCost = value;
            }
        }

        private MPCLSPPlant(MPCLSPPlant instance)
        {
            UID = instance.UID;
            SetupCost = instance.SetupCost;
            ProductionCost = instance.ProductionCost;
            ProcessingTimes = instance.ProcessingTimes;
            TransferCost = instance.TransferCost;
            SetupTime = instance.SetupTime;
            Lines = new List<MPCLSPLine>();instance.Lines.ForEach(l => Lines.Add(l.Copy()));
            Products = new List<MPCLSPProduct>();instance.Products.ForEach(p => Products.Add(p.Copy()));
        }


        public MPCLSPPlant(int uid,List<MPCLSPLine> lines)
        {
            _uID = uid;
            _setupCost = new Dictionary<int, double>();
            _productionCost = new Dictionary<int, double>();
            _transferCost = new Dictionary<int, double>();
            _processingTimes = new Dictionary<int, double>();
            _setupTime = new Dictionary<int, double>();
            Products = new List<MPCLSPProduct>();
            _lines = lines;
        }

        public MPCLSPPlant Copy()
        {
            return new MPCLSPPlant(this);
        }

    }
}
