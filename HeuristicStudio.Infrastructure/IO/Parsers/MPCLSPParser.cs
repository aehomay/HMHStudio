using HeuristicStudio.Core.Model;
using HeuristicStudio.Core.Model.SCPData;
using HeuristicStudio.Core.Model.Problems;
using HeuristicStudio.Core.Model.MPCLSPData;
using System;
using HeuristicStudio.Core.Model.DataStructure.MPCLSPData;
using System.Collections.Generic;
using System.Linq;

namespace HeuristicStudio.Infrastructure.IO.Parsers
{
    public class MPCLSPParser : IParser<SCPDataSet>
    {
   
        protected MPCLSP _problem = null;

        public IProblem Problem
        {
            get
            {
                return _problem;
            }
        }

        public MPCLSPParser()
        {
            _problem = new MPCLSP();
            _problem.DataSet = new MPCLSPSet();
        }

        public void InventoryCost(string path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string str = file.ReadLine();
            int products = int.Parse((str.Trim()));

            for (int i = 1; i <= products; i++)
            {
                double cost = double.Parse(file.ReadLine());
                _problem.DataSet.Products[i-1].InventoryCost = cost;
            }
        }

        public void SC_ST_PC_PT(string path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string str = file.ReadLine();

            int products = int.Parse((str.Trim().Split('/')[0]));
            int plants = int.Parse(str.Trim().Split('/')[1]);

            //Products
            for (int i = 1; i <= products; i++)
            {
                MPCLSPProduct product = new MPCLSPProduct(i);
                MPCLSPFamily family = new MPCLSPFamily(i);
                family.Products.Add(product);
                _problem.DataSet.Families.Add(family);
                _problem.DataSet.Products.Add(product);
            }

            //Setup Cost
            for (int j = 1; j <= plants; j++)
            {
                str = file.ReadLine();
                if (str == "") { j--; continue; }
                string[] costs = str.Trim().Split('\t');
                MPCLSPLine line = new MPCLSPLine(j) { Families = new List<MPCLSPFamily>()};
                MPCLSPPlant plant = new MPCLSPPlant(j, new List<MPCLSPLine>() { line });
                
                _problem.DataSet.Plants.Add(plant);
                for (int i = 1; i <= products; i++)
                {
                    MPCLSPProduct product = _problem.DataSet.Products.Find(p=>p.UID == i);
                    line.Products.Add(product);
                    plant.Products.Add(product);
                    plant.SetupCost.Add(product.UID, double.Parse(costs[i-1]));
                }
            }

            //Setup Time
            for (int j = 1; j <= plants; j++)
            {
                str = file.ReadLine();
                if (str == "") { j--; continue; }
                string[] costs = str.Trim().Split('\t');
                MPCLSPPlant plant = _problem.DataSet.Plants.Find(p => p.UID == j);
                for (int i = 1; i <= products; i++)
                {
                    plant.SetupTime.Add(i, int.Parse(costs[i - 1]));
                }
            }

            //Production Cost
            for (int j = 1; j <= plants; j++)
            {
                str = file.ReadLine();
                if (str == "") { j--; continue; }
                string[] costs = str.Trim().Split('\t');
                MPCLSPPlant plant = _problem.DataSet.Plants.Find(p => p.UID == j);
                for (int i = 1; i <= products; i++)
                {
                    plant.ProductionCost.Add(i, double.Parse(costs[i - 1]));
                }
            }

            //Processing Time
            for (int j = 1; j <= plants; j++)
            {
                str = file.ReadLine();
                if (str == "") { j--; continue; }
                string[] costs = str.Trim().Split('\t');
                MPCLSPPlant plant = _problem.DataSet.Plants.Find(p => p.UID == j);
                for (int i = 1; i <= products; i++)
                {
                    plant.ProcessingTimes.Add(i, int.Parse(costs[i - 1]));
                }
            }

        }
       
        public void TransferCost(string path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string str = file.ReadLine();
            int plants = int.Parse(str.Trim().Split('/')[0]);

            for (int i = 1; i <= plants; i++)
            {
                str = file.ReadLine();
                if (str == "") { i--; continue; }

                string[] costs = str.Trim().Split('\t');
                MPCLSPPlant plant = _problem.DataSet.Plants.Find(p => p.UID == i);
                for (int j = 1; j <= plants; j++)
                {
                    plant.TransferCost.Add(j, double.Parse(costs[j - 1]));
                }
            }

        }

        public void Period(string path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string str = file.ReadLine();

            int products = int.Parse((str.Trim().Split('/')[2]));
            int plants = int.Parse(str.Trim().Split('/')[1]);
            int periods = int.Parse(str.Trim().Split('/')[0]);

            for (int t = 1; t <= periods; t++)
            {
                str = file.ReadLine();
                if (str == "") { t--; continue; }

                MPCLSPPeriod period = new MPCLSPPeriod(t,new Dictionary<MPCLSPPlant, int>());
                _problem.DataSet.Periods.Add(period);
                String[] capacities = str.Trim().Split('\t');

                for (int j = 1; j <= plants; j++)
                {
                    MPCLSPPlant plant = _problem.DataSet.Plants.Find(p => p.UID == j);
                    plant.Lines.FirstOrDefault().Capacity = int.Parse(capacities[j - 1]);
                    period.Capacity.Add(plant, plant.Capacity);

                    str = file.ReadLine();
                    string[] costs = str.Trim().Split('\t');

                    for (int i = 1; i <= products; i++)
                    {
                        MPCLSPProduct product = _problem.DataSet.Products.Find(p => p.UID == i);
                        product.TotalDemand += int.Parse(costs[i - 1]);
                        period.Demands.Add(new PP() { Product = product,Plant= plant },int.Parse(costs[i-1]));
                        period.Stock.Add(new PP() { Product = product, Plant = plant }, 0); 
                        period.StockCost.Add(new PP() { Product = product, Plant = plant }, 1.0);
                        //period.TransferQuantity.Add(new PPP() { Product = product, PlantJ = plant, PlantK = plant }, 0);

                    }
                }
            }
        }

       

        public void Read(string path)
        {
            throw new NotImplementedException();
        }
    }
}
