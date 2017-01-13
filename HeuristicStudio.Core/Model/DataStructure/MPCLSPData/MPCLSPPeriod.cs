using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure.MPCLSPData
{
    public class MPCLSPPeriod
    {
       
        private Dictionary<MPCLSPPlant, int> _Capacity = null;

        /// <summary>
        /// Each plant has its own capacity in each period
        /// PlantID, Capacity
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

        public MPCLSPPeriod(Dictionary<MPCLSPPlant, int> capacity)
        {
            Capacity = capacity;
            Capacity.ToList().ForEach(c => c.Key.Lines.ForEach(l => l.Capacity = c.Value));
        }

        
    }
}
