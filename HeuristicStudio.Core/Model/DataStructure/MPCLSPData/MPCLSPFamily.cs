using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure.MPCLSPData
{
    public class MPCLSPFamily
    {
        List<MPCLSPProduct> _products = null;
        
        public int UID { get; set; }

        public  MPCLSPFamily(int uid)
        {
            UID = uid;
            _products = new List<MPCLSPProduct>();
        }

        private MPCLSPFamily(MPCLSPFamily instance)
        {
            UID = instance.UID;
            Products = new List<MPCLSPProduct>(); instance.Products.ForEach(p => _products.Add(p.Copy()));
        }

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

        public MPCLSPFamily Copy()
        {
            return new MPCLSPFamily(this);
        }
    }
}
