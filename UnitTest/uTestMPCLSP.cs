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

            int d = Problem.Demand(1, 1);

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
