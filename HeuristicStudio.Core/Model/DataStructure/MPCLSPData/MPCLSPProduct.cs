using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure.MPCLSPData
{
    
    public class MPCLSPProduct
    {
        private int _uID;
        private double _inventoryCost = 0.0;
        private int _totalDemand = 0;
        
        public double InventoryCost
        {
            get
            {
                return _inventoryCost;
            }

            set
            {
                _inventoryCost = value;
            }
        }

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

        public int TotalDemand
        {
            get
            {
                return _totalDemand;
            }

            set
            {
                _totalDemand = value;
            }
        }

        public MPCLSPProduct(int uid)
        {
            UID = uid;
        }

        /// <summary>
        /// Copy constructor for current object
        /// </summary>
        /// <param name="instance">Current object</param>
        private MPCLSPProduct(MPCLSPProduct instance)
        {
            UID = instance.UID;
            TotalDemand = instance.TotalDemand;
            InventoryCost = instance.InventoryCost;
        }

        public MPCLSPProduct Copy()
        {
            return new MPCLSPProduct(this);
        }
    }
}
