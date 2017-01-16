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
        private MPCLSPFamily _family = null;
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


        public MPCLSPFamily Family
        {
            get
            {
                return _family;
            }

            set
            {
                _family = value;
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

        public MPCLSPProduct(int uid, MPCLSPFamily family)
        {
            Family = family;
            UID = uid;
        }

        private MPCLSPProduct(MPCLSPProduct instance)
        {
            UID = instance.UID;
            Family = instance.Family;
            TotalDemand = instance.TotalDemand;
            InventoryCost = instance.InventoryCost;
        }

        public MPCLSPProduct Copy()
        {
            return new MPCLSPProduct(this);
        }
    }
}
