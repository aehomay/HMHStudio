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

        public MPCLSPProduct(int uid, MPCLSPFamily family)
        {
            Family = family;
            UID = uid;
        }

        private MPCLSPProduct(MPCLSPProduct instance)
        {
            InventoryCost = instance.InventoryCost;
            UID = instance.UID;
            Family = instance.Family;
        }

        public MPCLSPProduct Copy()
        {
            return new MPCLSPProduct(this);
        }
    }
}
