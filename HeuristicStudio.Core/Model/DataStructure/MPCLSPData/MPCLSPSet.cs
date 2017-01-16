using HeuristicStudio.Core.Model.DataStructure.MPCLSPData;
using System;
using System.Collections.Generic;


namespace HeuristicStudio.Core.Model.MPCLSPData
{
    public class MPCLSPSet
    {
        public List<MPCLSPPlant> Plants { get; set; }
        public List<MPCLSPPeriod> Periods { get; set; }
        public List<MPCLSPProduct> Products { get; set; }
        public List<MPCLSPFamily> Families { get; set; }

        private MPCLSPSet(MPCLSPSet instance)
        {
            Plants = new List<MPCLSPPlant>();instance.Plants.ForEach(p => Plants.Add(p.Copy()));
            Periods = new List<MPCLSPPeriod>(); instance.Periods.ForEach(p => Periods.Add(p.Copy()));
            Products = new List<MPCLSPProduct>(); instance.Products.ForEach(p => Products.Add(p.Copy()));
            Families = new List<MPCLSPFamily>(); instance.Families.ForEach(f => Families.Add(f.Copy()));
        }
        public MPCLSPSet()
        {
            Plants = new List<MPCLSPPlant>();
            Periods = new List<MPCLSPPeriod>();
            Products = new List<MPCLSPProduct>();
            Families = new List<MPCLSPFamily>();
        }

        public void Read(String path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string str = file.ReadLine();
        }

        public MPCLSPSet Copy()
        {
            return new MPCLSPSet(this);
        }
    }
}
