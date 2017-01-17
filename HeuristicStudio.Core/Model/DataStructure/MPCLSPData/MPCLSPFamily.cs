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
        private bool _active = false;
        private int _uID = 0;
        

        public  MPCLSPFamily(int uid)
        {
            UID = uid;
            _products = new List<MPCLSPProduct>();
        }

        private MPCLSPFamily(MPCLSPFamily instance)
        {
            UID = instance.UID;
            Products = new List<MPCLSPProduct>(); instance.Products.ForEach(p => _products.Add(p.Copy()));
            Active = instance.Active;
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
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

        /// <summary>
        /// Y_fmt if is true a setup occurs to family f on filling line m in period t
        /// </summary>
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                _active = value;
            }
        }

        public MPCLSPFamily Copy()
        {
            return new MPCLSPFamily(this);
        }
    }
}
