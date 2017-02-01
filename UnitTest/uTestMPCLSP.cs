using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeuristicStudio.Core.Model.Problems;
using HeuristicStudio.Core.Model.MPCLSPData;
using HeuristicStudio.Infrastructure.IO.Parsers;
using HeuristicStudio.Core.Model.DataStructure.MPCLSPData;
using HeuristicStudio.Core.Model.MetaHeuristic.MPCLSPHeuristic;

namespace UnitTest
{
    [TestClass]
    public class uTestMPCLSP
    {

        private MPCLSP Problem = null;

        private void LoadData()
        {
            MPCLSPParser parser = new MPCLSPParser();
            Problem = new MPCLSP();
            MPCLSPSet dataset = new MPCLSPSet();

            string Path = Environment.CurrentDirectory + "\\Dataset";
            string SC_ST_PC_PT = Path + "\\SC-ST-PC-PT.txt";
            string TransferCostPath = Path + "\\TransferCost.txt";
            string InventoryCostPath = Path + "\\InventoryCost.txt";
            string PeriodPath = Path + "\\Period-Capacity.txt";
           

            parser.SC_ST_PC_PT(SC_ST_PC_PT);
            parser.TransferCost(TransferCostPath);
            parser.InventoryCost(InventoryCostPath);
            parser.Period(PeriodPath);

            Problem = ((MPCLSP)parser.Problem).Copy();


            double cost = Problem.ProductProductionCostFromPeriod2Period(1, 1, 1);

            double sc_pp = Problem.SetupCostOnPlant(1, 1);
            double st_pp = Problem.SetupTimeInPlant(1, 1);

            double pc_pp = Problem.ProductionCostOnPlant(1, 1);
            double pt_pp = Problem.ProcessingTimeInPlant(1, 1);

            Problem.DataSet.Periods[1].ProductionQuantity.Add(new PP() { Plant = Problem.DataSet.Plants[0], Product = Problem.DataSet.Products[0] }, 20);
            Problem.DataSet.Periods[1].ProductionQuantity.Add(new PP() { Plant = Problem.DataSet.Plants[1], Product = Problem.DataSet.Products[0] }, 20);
            Problem.DataSet.Periods[1].ProductionQuantity.Add(new PP() { Plant = Problem.DataSet.Plants[2], Product = Problem.DataSet.Products[0] }, 20);

            int pc = Problem.DataSet.Periods[1].ProductProductionQuantity(1);
            

            int d_iut = Problem.ProductDemandOnPlantFromPeriod2Period(1, 1, 1, 6);
            int PD_jut = Problem.PlantDemandFromPeriod2Period(1, 1, 1);
            int D_iut = Problem.ProductDemandFromPeriod2Period(1, 1, 6);
            MPCLSPSolution solution = new MPCLSPSolution() { Dataset = Problem.DataSet };
            MyILS_MPCLSP heuristic = new MyILS_MPCLSP();
            
            heuristic.Execute(Problem);
        }

        [TestMethod]
        public void TestMethod1()
        {
            LoadData();
        }
    }
}
