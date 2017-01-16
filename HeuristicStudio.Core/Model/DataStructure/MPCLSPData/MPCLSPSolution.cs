using HeuristicStudio.Core.Model.MPCLSPData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure.MPCLSPData
{
    public class MPCLSPSolution : ISolution
    {
        public double Cost
        {
            get
            {
                return 0;
            }
        }

        MPCLSPSet _dataset = null;
        MPCLSPSet Dataset { get { return _dataset; } }

        private MPCLSPSolution(MPCLSPSolution instance)
        {
            _dataset = instance.Dataset.Copy();
            

        }

        public MPCLSPSolution()
        {
        }

        public MPCLSPSolution Copy()
        {
            return new MPCLSPSolution(this);
        }


    }
}
