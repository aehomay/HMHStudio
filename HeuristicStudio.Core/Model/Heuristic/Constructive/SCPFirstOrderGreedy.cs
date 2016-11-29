using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.Problems;
using System.Diagnostics;

namespace HeuristicStudio.Core.Model.Heuristic
{
    public class SCPFirstOrderGreedy : IConstructiveHeuristic
    {
        private readonly Stopwatch sp = new Stopwatch();
        public ISolution FSolution { get; set; }
        protected SCP _problem = null;
        protected double _totalCost = 0;


        public double TotalCost
        {
            get
            {
                return _totalCost;
            }

            set
            {
                _totalCost = value;
            }
        }
        public IProblem Problem
        {
            get
            {
                return _problem;
            }
        }
        public TimeSpan Elapsed
        {
            get
            {
                return sp.Elapsed;
            }
        }


        public SCPFirstOrderGreedy()
        {
            FSolution = new SCPSolution();
        }

        public double Execute(IProblem problem)
        {
            return Execute(problem,false);
        }
        public double Execute(IProblem problem,bool vertical)
        {
            _problem = (SCP)problem;
            if (vertical)
                VerticalSelection();
            else
                HorizantalSelection();

            FSolution = RemoveRedundantSet((SCPSolution)FSolution);
            if (IsFeasible((SCPSolution)FSolution) == false)
                _problem.Solution = null;
            else
                _problem.Solution = (SCPSolution)FSolution;

            return _problem.Solution.Cost;
        }
        private void VerticalSelection()
        {
            sp.Restart();
            AttributeWeighting();

            foreach (var att in _problem.Source.Attributes)
            {
                SCPSet set = _problem.Source.Sets.Find(s => s.Tag == att.GetCheapestSetTag()); 
                bool flag = false;
                set.Attributes.ForEach(a1 =>
                {
                    if (a1.Visit == false)
                    {
                        a1.Visit = true;
                        flag = true;
                    }
                });
                if (flag)
                    ((SCPSolution)FSolution).Sets.Add(set);
            }
            sp.Stop();
            _problem.Solution = (SCPSolution)FSolution;
        }
        private void HorizantalSelection()
        {
            FSolution = new SCPSolution();
            sp.Restart();
            int coveredatt = 0;
            int att_count = _problem.Matrix.Size.X;
            while (coveredatt < att_count)
            {

                SCPSet set = _problem.Source.GetLightestNotVisitedSet();

                set.Attributes.ForEach(a1 =>
                {
        
                if (a1.Visit == false)
                {
                        a1.Visit = true;
                    coveredatt++;
                        a1.UsedIn.ForEach(s1 =>
                    {
                        s1.Frequency--;
                    });
                    }
                });
                if (set.Visit == false)
                    ((SCPSolution)FSolution).Sets.Add(set);
                _problem.Source.Sets.Find(s => s.Tag == set.Tag).Visit = true;
                _problem.Source.Weighting();
            }
            sp.Stop();

           
        }

        private void AttributeWeighting()
        {
            foreach (var att in _problem.Source.Attributes)
            {
                att.Weight = 1.0 / att.Frequency;
            }
            //Sort by less probability 
            _problem.Source.Attributes = _problem.Source.Attributes.OrderByDescending(a => a.Weight).ToList();
        }

        private bool IsFeasible(SCPSolution solution)
        {

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
            int n = _problem.Matrix.Size.X;
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


            return true;
        }

        private SCPSolution RemoveRedundantSet(SCPSolution solution)
        {
            SCPSolution improved = solution.Clone();
            List<int> blacklist = new List<int>();

            solution.ComputeAttributeRedundancy();
            solution.Sets = solution.Sets.OrderByDescending(s => s.Cost).ToList();
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
                    set.Attributes.ForEach(a =>
                    {
                        a.Redundancy--;
                    });
                    blacklist.Add(set.Tag);
                }
            }

            blacklist.ForEach(b => improved.Sets.Remove(improved.Sets.Find(s => s.Tag.ToString() == b.ToString())));
            if (improved.Cost < solution.Cost)
                return improved;
            else
                return solution;
        }

    }
}
