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
                _problem.DataSet.Products.Add(product);
            }

            //Setup Cost
            for (int i = 1; i <= plants; i++)
            {
                str = file.ReadLine();
                if (str == "") { i--; continue; }
                string[] costs = str.Trim().Split('\t');
                MPCLSPLine line = new MPCLSPLine(i) { Families = new List<MPCLSPFamily>() { new MPCLSPFamily(i) } };
                MPCLSPPlant plant = new MPCLSPPlant(i, new List<MPCLSPLine>() { line });
                
                _problem.DataSet.Plants.Add(plant);
                for (int j = 1; j <= products; j++)
                {
                    MPCLSPProduct product = _problem.DataSet.Products.Find(p=>p.UID == j);
                    product.Family = line.Families.FirstOrDefault();
                    line.Families.FirstOrDefault().Products.Add(product);
                    line.Products.Add(product);
                    plant.Products.Add(product);
                    plant.SetupCost.Add(product, double.Parse(costs[j-1]));
                }
            }

            //Setup Time
            for (int i = 1; i <= plants; i++)
            {
                str = file.ReadLine();
                if (str == "") { i--; continue; }
                string[] costs = str.Trim().Split('\t');
                MPCLSPPlant plant = _problem.DataSet.Plants.Find(p => p.UID == i);
                for (int j = 1; j <= products; j++)
                {
                    plant.SetupTime.Add(plant.Products.Find(p => p.UID == j), double.Parse(costs[j - 1]));
                }
            }

            //Production Cost
            for (int i = 1; i <= plants; i++)
            {
                str = file.ReadLine();
                if (str == "") { i--; continue; }
                string[] costs = str.Trim().Split('\t');
                MPCLSPPlant plant = _problem.DataSet.Plants.Find(p => p.UID == i);
                for (int j = 1; j <= products; j++)
                {
                    plant.ProductionCost.Add(plant.Products.Find(p => p.UID == j), double.Parse(costs[j - 1]));
                }
            }

            //Processing Time
            for (int i = 1; i <= plants; i++)
            {
                str = file.ReadLine();
                if (str == "") { i--; continue; }
                string[] costs = str.Trim().Split('\t');
                MPCLSPPlant plant = _problem.DataSet.Plants.Find(p => p.UID == i);
                for (int j = 1; j <= products; j++)
                {
                    plant.ProcessingTimes.Add(plant.Products.Find(p => p.UID == j), double.Parse(costs[j - 1]));
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
                    plant.TransferCost.Add(_problem.DataSet.Plants.Find(p => p.UID == j), double.Parse(costs[j - 1]));
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

            for (int k = 1; k <= periods; k++)
            {
                str = file.ReadLine();
                if (str == "") { k--; continue; }

                MPCLSPPeriod period = new MPCLSPPeriod(k,new Dictionary<MPCLSPPlant, int>());
                _problem.DataSet.Periods.Add(period);
                String[] capacities = str.Trim().Split('\t');

                for (int i = 1; i <= plants; i++)
                {
                    MPCLSPPlant plant = _problem.DataSet.Plants.Find(p => p.UID == i);
                    plant.Lines.FirstOrDefault().Capacity = int.Parse(capacities[i - 1]);
                    period.Capacity.Add(plant, plant.Capacity);

                    str = file.ReadLine();
                    string[] costs = str.Trim().Split('\t');

                    for (int j = 1; j <= products; j++)
                    {
                        MPCLSPProduct product = _problem.DataSet.Products.Find(p => p.UID == j);
                        product.TotalDemand += int.Parse(costs[j - 1]);
                        period.Demands.Add(new PP() { Product = product,Plant= plant },int.Parse(costs[j-1]));
                        period.Stock.Add(new PP() { Product = product, Plant = plant }, 0);
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
