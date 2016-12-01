using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.DataStructure.SCP;
using HeuristicStudio.Core.Model.Heuristic;
using HeuristicStudio.Core.Model.Heuristic.Constructive;
using HeuristicStudio.Core.Model.MetaHeuristic;
using HeuristicStudio.Core.Model.Problems;
using HeuristicStudio.Infrastructure.IO;
using HeuristicStudio.Infrastructure.IO.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using HeuristicStudio.Infrastructure.IO.Monitor;
using HeuristicStudio.Core.Service;
using System.Threading;

namespace UnitTest
{
    struct Result
    {
        public double Cost;
        public int Time;
    }
    /// <summary>
    /// Summary description for utSCP
    /// </summary>
    [TestClass]
    public class utSCP
    {
        SCPParser scpParser = new SCPParser();
        List<String> files = new List<string>();
        public utSCP()
        {
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scp42.txt");//0-512-(200x1000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scp43.txt");//1-516-(200x1000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scp44.txt");//2-494-(200x1000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scp56.txt");//3-213-(200x2000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scp57.txt");//4-293-(200x2000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scpa4.txt");//5-234-(300x3000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scpa2.txt");//6-252-(300x3000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scpa3.txt");//7-232-(300x3000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scpa1.txt");//8-253-(300x3000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scpb3.txt");//9-80-(300x3000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scpb2.txt");//10-76-(300x3000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scpb1.txt");//11-69-(300x3000)
            files.Add("C:\\Users\\aehom\\Desktop\\Assignment 1\\SCP-Instances\\scpb4.txt");//12-79-(300x3000)
            IOData.ReadFileToMatrix(scpParser, files[0]);
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private bool SolutionValidity()
        {
            SCP problem = (SCP)scpParser.Problem;
            HashSet<int> problem_att = new HashSet<int>();
            problem.Source.Attributes.ForEach(a => problem_att.Add(a.Tag));

            HashSet<int> solution_att = new HashSet<int>();
            problem.Solution.USet.ForEach(a => solution_att.Add(a.Item2));

            return (problem_att.IsSubsetOf(solution_att));
        }

        private bool IsFeasible(SCPSolution solution, SCP problem)
        {
            if (solution.Sets.Count <= 0) return false;
            #region Step#1 Exhustive test
            List<int> attributes = new List<int>();
            solution.Sets.ForEach(s => s.Attributes.ForEach(a => attributes.Add(a.Tag)));
            attributes = attributes.OrderBy(a => a).ToList();
            for (int i = 1; i < attributes.Count; i++)
            {
                if (attributes[i] - attributes[i - 1] > 1)
                    return false;
            }
            #endregion

            #region Step#2 Formulation test nx(n-1)/2
            int n = problem.Matrix.Size.X;
            int sum1 = n * (n + 1) / 2;

            int seed = attributes[0];
            int sum2 = seed;
            for (int i = 1; i < attributes.Count; i++)
            {
                if (attributes[i] != seed)
                {
                    seed = attributes[i];
                    sum2 += seed;
                }
            }
            if (sum2 != sum1) return false;
            #endregion

            #region Step#3 Hashing test
            int[] list = new int[n];
            foreach (int item in attributes)
                list[item - 1] = item;
            #endregion

            return true;
        }

        private void RemoveRedundantSet(SCPSolution solution)
        {
            SCPSolution improved = solution.Clone();
            List<int> blacklist = new List<int>();

            solution.ComputeAttributeRedundancy();

            foreach (var set in solution.Sets)
            {
                bool useless = true;

                foreach (var a in set.Attributes)
                {
                    if (a.Redundancy <= 1)
                    {
                        useless = false;
                        break;
                    }
                }
                if (useless)
                {
                    set.Attributes.ForEach(a => a.Redundancy--);
                    blacklist.Add(set.Tag);
                }
            }

            blacklist.ForEach(b => improved.Sets.Remove(improved.Sets.Find(s => s.Tag.ToString() == b.ToString())));
            if (improved.Cost < solution.Cost)
                solution = improved;
        }

        [TestMethod]
        [TestCategory("Constructive")]
        public void Test()
        {
            SCP problem = (SCP)scpParser.Problem;
            SCPDataSet source = (SCPDataSet)problem.Source.Clone();
            List<Tuple<SCPAttribute, SCPSet>> panel = new List<Tuple<SCPAttribute, SCPSet>>();
            source.Attributes.ForEach(a =>
            {
                a.Cost = a.GetCheapestCost();
                panel.Add(new Tuple<SCPAttribute, SCPSet>(a, a.GetCheapestSet()));
            });



            int covered = 0;
            SCPSolution solution2 = new SCPSolution();
            while (covered < source.Attributes.Count)
            {
                source.Attributes = source.Attributes.OrderByDescending(a => a.Cost).ToList();
                source.Attributes.ForEach(a1 =>
                {
                    a1.Visit = false;
                    a1.UsedIn.ForEach(s =>
                    {
                        s.Weight = s.Cost / s.Frequency;
                    });
                    a1.UsedIn = a1.UsedIn.OrderBy(ui => ui.Weight).ToList();
                });

                SCPSet set = source.Attributes.Where(at => at.Visit == false).First().UsedIn.First();

                set.Attributes.ForEach(a =>
                {
                    if (a.Visit == false)
                    {
                        a.Visit = true;
                        covered++;
                        a.UsedIn.ForEach(s => s.Frequency--);
                    }
                });
                if (set.Visit == false)
                    solution2.Sets.Add(set);
                set.Visit = true;
            }


            RemoveRedundantSet(solution2);

            bool result = IsFeasible(solution2, problem);

        }

        private SCPSet GetTheMostExSet(List<Tuple<SCPAttribute, SCPSet>> panel)
        {
            SCPSet set = new SCPSet();
            panel.ForEach(s =>
            {
                if (s.Item1.Visit == false)
                {
                    if (s.Item2.Cost > set.Cost)
                        set = s.Item2;
                }
            });
            return set;
        }



        [TestMethod]
        [TestCategory("Constructive")]
        public void GRASP()
        {
            IConstructiveHeuristic grasp = new SCPGRASP(0.9, 1e-9);
            double cost = grasp.Execute(scpParser.Problem);
            TimeSpan elapsed = grasp.Elapsed;
            bool validity = SolutionValidity();
            Assert.AreEqual(validity, true);
        }

        [TestMethod]
        [TestCategory("Constructive")]
        public void VerticalFirstOrderGreedy()
        {
            SCPFirstOrderGreedy scpFOG = new SCPFirstOrderGreedy();
            double cost = scpFOG.Execute(scpParser.Problem, true);
            TimeSpan elapsed = scpFOG.Elapsed;
            bool validity = SolutionValidity();
            Assert.AreEqual(validity, true);
            Debug.Print("Cost:" + cost);
        }

        [TestMethod]
        [TestCategory("Constructive")]
        public void HorizintalFirstOrderGreedy()
        {
            SCPFirstOrderGreedy scpG = new SCPFirstOrderGreedy();
            double cost = scpG.Execute(scpParser.Problem, false);
            TimeSpan elapsed = scpG.Elapsed;
            bool validity = SolutionValidity();
            Assert.AreEqual(validity, true);
        }


        [TestMethod]
        [Description("SmartLocalSearch + SCPFirstOrderGreedy")]
        [TestCategory("Improvement")]
        public void BGreedySmartLocalSearch()
        {
            SCPFirstOrderGreedy scpFOG = new SCPFirstOrderGreedy();
            double cost = scpFOG.Execute(scpParser.Problem);

            SmartLocalSearch scpBS = new SmartLocalSearch();
            cost = scpBS.Execute(scpFOG.Problem);
            ((SCP)scpParser.Problem).Solution = scpBS.OptimumSultion;
            TimeSpan elapsed = scpBS.Elapsed;
            bool validity = SolutionValidity();
            Assert.AreEqual(validity, true);
        }

        [TestMethod]
        [Description("SmartLocalSearch + GRASP")]
        [TestCategory("Improvement")]
        public void FOGSmartLocalSearch()
        {
            IConstructiveHeuristic grasp = new SCPFirstOrderGreedy();
            double cost = grasp.Execute(scpParser.Problem);

            SmartLocalSearch scpLS = new SmartLocalSearch();
            cost = scpLS.Execute(grasp.Problem);
            ((SCP)scpParser.Problem).Solution = scpLS.OptimumSultion;
            TimeSpan elapsed = scpLS.Elapsed;
            bool validity = SolutionValidity();
            Assert.AreEqual(validity, true);
        }

        [TestMethod]
        [Description("DestructiveConstructive + FOG")]
        [TestCategory("Improvement")]
        public void DCFOG()
        {
            IConstructiveHeuristic fog = new SCPFirstOrderGreedy();
            double cost = fog.Execute(scpParser.Problem);

            DestructiveConstructive dc = new DestructiveConstructive();
            cost = dc.Execute(fog.Problem);
            ((SCP)scpParser.Problem).Solution = dc.OptimumSultion;
            TimeSpan elapsed = dc.Elapsed;
            bool validity = SolutionValidity();
            Assert.AreEqual(validity, true);
        }

        [TestMethod]
        [Description("DestructiveConstructive + GRASP")]
        [TestCategory("Improvement")]
        public void DCGRASP()
        {
            IConstructiveHeuristic grasp = new SCPGRASP(0.9, 1e-9);
            double cost = grasp.Execute(scpParser.Problem);

            DestructiveConstructive dc = new DestructiveConstructive();
            cost = dc.Execute(grasp.Problem);
            ((SCP)scpParser.Problem).Solution = dc.OptimumSultion;
            TimeSpan elapsed = dc.Elapsed;
            bool validity = SolutionValidity();
            Assert.AreEqual(validity, true);
        }

        [TestMethod]
        public void GenSubSet()
        {
            Subset<int>(new HashSet<int>() { 1, 2, 3 });
        }

        public void Subset<T>(HashSet<T> set)
        {
            List<HashSet<T>> allsubsets = new List<HashSet<T>>();

            int length = set.Count;
            int index = 0;

           
            while (index < length)
            {
                allsubsets.Add(new HashSet<T>() { set.ElementAt(index)});

                HashSet<T> newsubset = new HashSet<T>();
                for (int k = 0; k < length; k++)
                {
                    if (k == index) continue;
                    newsubset.Add(set.ElementAt(k));
                }

                for (int i = 0; i < length - 1; i++)
                {
                    HashSet<T> subset = new HashSet<T>();
                    for (int j = i; j < length - 1; j++)
                    {
                        subset.Add(newsubset.ElementAt(j));
                    }
                    if (subset.Count > 0)
                    {
                        bool add = true;
                        foreach (var s in allsubsets)
                        {
                            if (newsubset.IsSubsetOf(s))
                            { add = false; break; }
                        }
                        if(add)
                            allsubsets.Add(subset);
                    }
                }
                index++;
            }
            allsubsets.Add(set);
        }


    [TestMethod]
        [Description("SmartLocalSearch + GRASP")]
        [TestCategory("Improvement")]
        public void GSmartLocalSearch()
        {
            IConstructiveHeuristic grasp = new SCPGRASP(0.9, 1e-9);
            double cost = grasp.Execute(scpParser.Problem);

            SmartLocalSearch scpLS = new SmartLocalSearch();
            cost = scpLS.Execute(grasp.Problem);
            ((SCP)scpParser.Problem).Solution = scpLS.OptimumSultion;
            TimeSpan elapsed = scpLS.Elapsed;
            bool validity = SolutionValidity();
            Assert.AreEqual(validity, true);
        }

        [TestMethod]
        [Description("LocalSearch + GRASP")]
        [TestCategory("Improvement")]
        public void GLocalSearch()
        {
            IConstructiveHeuristic grasp = new SCPGRASP(0.9, 1e-9);
            double cost = grasp.Execute(scpParser.Problem);

            LocalSearch scpLS = new LocalSearch();
            cost = scpLS.Execute(grasp.Problem.Clone());
            ((SCP)scpParser.Problem).Solution = scpLS.OptimumSultion;
            TimeSpan elapsed = scpLS.Elapsed;
            bool validity = SolutionValidity();
            Assert.AreEqual(validity, true);
        }

        [TestMethod]
        [Description("IterativeLocalSearch + GRASP")]
        [TestCategory("Improvement")]
        public void IterativeLocalSearch()
        {
            List<Tuple<double, SCPSolution>> history = new List<Tuple<double, SCPSolution>>();
            bool validity = false;
            int iteratin = 0;
            while (iteratin++ < 5)
            {
                IConstructiveHeuristic grasp = new SCPGRASP(0.9, 1e-9);
                double cost = grasp.Execute((SCP)((SCP)scpParser.Problem).Clone());
                LocalSearch scpLS = new LocalSearch();
                cost = scpLS.Execute(grasp.Problem);
                TimeSpan elapsed = scpLS.Elapsed;
                history.Add(new Tuple<double, SCPSolution>(cost, scpLS.OptimumSultion.Clone()));
            }

             ((SCP)scpParser.Problem).Solution = history.OrderBy(h => h.Item1).ToList().FirstOrDefault().Item2;
            validity = SolutionValidity();

            Assert.AreEqual(validity, true);
        }

        [TestMethod]
        [Description("IterativeLocalSearch + GRASP")]
        [TestCategory("Improvement")]
        public void IterativeSmartLocalSearch()
        {
            SmartLocalSearch scpLS = null;
            List<Tuple<double, SCPSolution>> history = new List<Tuple<double, SCPSolution>>();
            bool validity = false;
            int iteratin = 0;
            while (iteratin++ < 5)
            {
                IConstructiveHeuristic grasp = new SCPGRASP(0.9, 1e-9);
                double cost = grasp.Execute((SCP)((SCP)scpParser.Problem).Clone());
                scpLS = new SmartLocalSearch();
                cost = scpLS.Execute(grasp.Problem);
                TimeSpan elapsed = scpLS.Elapsed;
                history.Add(new Tuple<double, SCPSolution>(cost, scpLS.OptimumSultion.Clone()));
            }

          ((SCP)scpParser.Problem).Solution = history.OrderBy(h => h.Item1).ToList().FirstOrDefault().Item2;
            validity = SolutionValidity();

            Assert.AreEqual(validity, true);

        }

        [TestMethod]
        [TestCategory("Benchmark")]
        public void Benchmark1()
        {
            SCPParser parser = new SCPParser();
            files.ForEach(file =>
            {
                IOData.ReadFileToMatrix(parser, file);

                IHeuristic h1 = new SCPGRASP(0.9, 1e-9);
                IHeuristic h2 = new SCPFirstOrderGreedy();
                IHeuristic h3 = new GreedyInsecticideSpray();
                IHeuristic h4 = new LocalSearch();
                IHeuristic h5 = new SmartLocalSearch();






                double cost1 = h1.Execute((SCP)((SCP)parser.Problem).Clone());
                double cost2 = h2.Execute((SCP)((SCP)parser.Problem).Clone());
                double cost3 = h3.Execute((SCP)((SCP)parser.Problem).Clone());
                double cost4 = h4.Execute((SCP)(((SCP)h1.Problem).Clone()));
                double cost5 = h5.Execute((SCP)(((SCP)h1.Problem).Clone()));



                int ms1 = h1.Elapsed.Milliseconds;
                int ms2 = h2.Elapsed.Milliseconds;
                int ms3 = h3.Elapsed.Milliseconds;
                int ms4 = h4.Elapsed.Milliseconds;
                int ms5 = h5.Elapsed.Milliseconds;

                Monitoring.Instance.Write("GRASP:" + file.Split('\\')[file.Split('\\').Count() - 1] + ":" + cost1.ToString() + ":" + ms1.ToString());
                Monitoring.Instance.Write("FOG:" + file.Split('\\')[file.Split('\\').Count() - 1] + ":" + cost2.ToString() + ":" + ms2.ToString());
                Monitoring.Instance.Write("LS:" + file.Split('\\')[file.Split('\\').Count() - 1] + ":" + cost3.ToString() + ":" + ms3.ToString());
                Monitoring.Instance.Write("GIS:" + file.Split('\\')[file.Split('\\').Count() - 1] + ":" + cost4.ToString() + ":" + ms4.ToString());
                Monitoring.Instance.Write("BIS:" + file.Split('\\')[file.Split('\\').Count() - 1] + ":" + cost5.ToString() + ":" + ms5.ToString());




            });
        }

        [TestMethod]
        [TestCategory("Benchmark")]
        public void Benchmark2()
        {
            SCPParser parser = new SCPParser();
            files.ForEach(file =>
            {
                IOData.ReadFileToMatrix(parser, file);

                //IHeuristic h1 = new SCPGRASP(0.9, 1e-9);
                IHeuristic h1 = new SCPFirstOrderGreedy();
                double cost = h1.Execute(parser.Problem);

                DestructiveConstructive dc = new DestructiveConstructive();
                cost = dc.Execute(h1.Problem);
                ((SCP)scpParser.Problem).Solution = dc.OptimumSultion;
                TimeSpan elapsed = dc.Elapsed;

                Monitoring.Instance.Write("DCFOG    " + file.Split('\\')[file.Split('\\').Count() - 1] + " " + cost.ToString() + " " + dc.Elapsed.ToString());
            });
        }

        private Result ISLS(SCP problem)
        {
            List<Tuple<double, SCPSolution>> history = new List<Tuple<double, SCPSolution>>();
            bool validity = false;
            int iteratin = 0;
            TimeSpan elapsed = new TimeSpan();
            double cost = 0;
            while (iteratin++ < 5)
            {
                IConstructiveHeuristic grasp = new SCPGRASP(0.9, 1e-9);
                cost = grasp.Execute(problem.Clone());
                SmartLocalSearch scpLS = new SmartLocalSearch();
                cost = scpLS.Execute(grasp.Problem);
                elapsed = scpLS.Elapsed;
                history.Add(new Tuple<double, SCPSolution>(cost, scpLS.OptimumSultion.Clone()));
            }

          ((SCP)scpParser.Problem).Solution = history.OrderBy(h => h.Item1).ToList().FirstOrDefault().Item2;
            validity = SolutionValidity();

            Assert.AreEqual(validity, true);

            Result r = new UnitTest.Result();
            r.Time = elapsed.Milliseconds;
            r.Cost = cost;
            return r;
        }

        private Result ILS(SCP problem)
        {
            List<Tuple<double, SCPSolution>> history = new List<Tuple<double, SCPSolution>>();
            bool validity = false;
            int iteratin = 0;
            TimeSpan elapsed = new TimeSpan();
            double cost = 0;
            while (iteratin++ < 5)
            {
                IConstructiveHeuristic grasp = new SCPGRASP(0.9, 1e-9);
                cost = grasp.Execute(problem.Clone());
                SmartLocalSearch scpLS = new SmartLocalSearch();
                cost = scpLS.Execute(grasp.Problem);
                elapsed = scpLS.Elapsed;
                history.Add(new Tuple<double, SCPSolution>(cost, scpLS.OptimumSultion.Clone()));
            }

          ((SCP)scpParser.Problem).Solution = history.OrderBy(h => h.Item1).ToList().FirstOrDefault().Item2;
            validity = SolutionValidity();

            Assert.AreEqual(validity, true);

            Result r = new UnitTest.Result();
            r.Time = elapsed.Milliseconds;
            r.Cost = cost;
            return r;
        }
    }
}
