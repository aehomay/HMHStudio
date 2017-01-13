using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure.MPCLSPData
{
    public class MPCLSPLine
    {
        private int _uID = 0;
        private List<MPCLSPProduct> _products = null;
        private List<MPCLSPFamily> _families = null;
        private int _capacity = 0;
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

        public List<MPCLSPFamily> Families
        {
            get
            {
                return _families;
            }

            set
            {
                _families = value;
            }
        }

        public int Capacity
        {
            get
            {
                return _capacity;
            }

            set
            {
                _capacity = value;
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

        public MPCLSPLine()
        {
        }

        private MPCLSPLine(MPCLSPLine instance)
        {
            Capacity = instance.Capacity;
            Families = new List<MPCLSPFamily>(); instance.Families.ForEach(f => _families.Add(f.Copy()));
            Products = new List<MPCLSPProduct>(); instance.Products.ForEach(p => _products.Add(p.Copy()));
            UID = instance.UID;

        }

        public MPCLSPLine Copy()
        {
            return new MPCLSPLine(this);
        }
    }
}
