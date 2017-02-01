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
        private MPCLSPPeriod _period;
        private MPCLSPPlant _plant;
        private MPCLSPProduct _product;
        private int _productionQuantity;
        public MPCLSPPeriod Period
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

        public MPCLSPPlant Plant
        {
            get
            {
                return _plant;
            }

            set
            {
                _plant = value;
            }
        }

        public MPCLSPProduct Product
        {
            get
            {
                return _product;
            }

            set
            {
                _product = value;
            }
        }

        public int ProductionQuantity
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

        private Schedule(Schedule instance)
        {
            Period = instance.Period.Copy();
            Plant = instance.Plant.Copy();
            Product = instance.Product.Copy();
            ProductionQuantity = instance.ProductionQuantity;
        }

        public Schedule() { }

        public Schedule(MPCLSPPeriod period, MPCLSPPlant plant, MPCLSPProduct product,int productionQuantity)
        {
            Period = period;
            Plant = plant;
            Product = product;
            ProductionQuantity = productionQuantity;
        }

        public Schedule Copy()
        {
            return new Schedule(this);
        }
    }

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

        List<Schedule> _schedules;
        public List<Schedule> Schedules
        {
            get
            {
                return _schedules;
            }

            set
            {
                _schedules = value;
            }
        }

        private MPCLSPSolution(MPCLSPSolution instance)
        {
            _dataset = instance.Dataset.Copy();
            Schedules = new List<Schedule>();
            instance.Schedules.ForEach(s=> Schedules.Add(s.Copy()));

        }

        public MPCLSPSolution()
        {
            _dataset = new MPCLSPSet();
            Schedules = new List<Schedule>();
        }

        public MPCLSPSolution Copy()
        {
            return new MPCLSPSolution(this);
        }


    }
}
