using HeuristicStudio.Core.Model.MPCLSPData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure.MPCLSPData
{
    public class Schedule
    {
        private MPCLSPSet _dataset = null;
        private int _period;
        private List<int> _plants;
        private Dictionary<int, int> _productionQuantity = null;
        public int Period
        {
            get
            {
                return _period;
            }

            set
            {
                _period = value;
            }
        }


        public Dictionary<int, int> ProductionQuantity
        {
            get
            {
                return _productionQuantity;
            }

            set
            {
                _productionQuantity = value;
            }
        }

        public double TotalCost
        {
            get
            {
                double cost = 0;
                Dataset.Plants.ForEach(plant =>
                {
                    plant.InstalledProducts.ForEach(product =>
                    {
                        cost += plant.SetupCost[product] + plant.ProductionCost[product];
                    });
                });
                return cost;
            }
        }

        public double ProductionCost
        {
            get
            {
                double cost = 0;
                Dataset.Plants.ForEach(plant =>
                {
                    plant.InstalledProducts.ForEach(product =>
                    {
                        cost += plant.ProductionCost[product];
                    });
                });
                return cost;
            }
        }

        public double SetupCost
        {
            get
            {
                double cost = 0;
                Dataset.Plants.ForEach(plant =>
                {
                    plant.InstalledProducts.ForEach(product=>
                    {
                        cost += plant.SetupCost[product];
                    });
                });
                return cost;
            }
        }

        public MPCLSPSet Dataset
        {
            get
            {
                return _dataset;
            }

            set
            {
                _dataset = value;
            }
        }

        public List<int> Plants
        {
            get
            {
                return _plants;
            }

            set
            {
                _plants = value;
            }
        }

        private Schedule(Schedule instance)
        {
            Period = instance.Period;
            Plants = new List<int>();instance.Plants.ForEach(plant=>Plants.Add(plant));
            ProductionQuantity = instance.ProductionQuantity;
        }

        public Schedule()
        {
            ProductionQuantity = new Dictionary<int, int>();
            Plants = new List<int>();
        }


        public Schedule(int period, int plant, List<int> product)
        {
            Period = period;
        }

        public Schedule Copy()
        {
            return new Schedule(this);
        }
    }
}
