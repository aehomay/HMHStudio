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
        private Dictionary<int,double> _processingTimes = null;
        private List<MPCLSPLine> _lines = null;

        
        public int UID
        {
            get
            {
                return _uID;
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
        }

        /// <summary>
        /// Each product has its own setup cost in each plant
        /// ProductID,Cost
        /// </summary>
        public Dictionary<int, double> SetupCost
        {
            get
            {
                return _setupCost;
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
        }

        /// <summary>
        /// Set of lines in each plant
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

        public MPCLSPPlant(int uid,List<MPCLSPLine> lines)
        {
            _uID = uid;
            _setupCost = new Dictionary<int, double>();
            _productionCost = new Dictionary<int, double>();
            _processingTimes = new Dictionary<int, double>();
            _setupTime = new Dictionary<int, double>();
            _lines = lines;
        }

        public MPCLSPPlant Copy()
        {
            MPCLSPPlant copy = new MPCLSPPlant();

            return copy;
        }

    }
}
