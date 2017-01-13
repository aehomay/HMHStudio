using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.Problems;
using System.Diagnostics;
using HeuristicStudio.Core.Model.SCPData;

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
                bool valuable = false;
                set.Attributes.ForEach(a1 =>
                {

                    if (a1.Visit == false)
                    {
                        if (_problem.Source.Attributes.Select(a => a.Tag).ToList().Exists(a => a == a1.Tag))
                        {
                            valuable = true;
                            a1.Visit = true;
                            coveredatt++;
                            a1.UsedIn.ForEach(s1 =>
                                 {
                                     s1.Frequency--;
                                 });
                        }
                    }
                });
                if (set.Visit == false && valuable)
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

            HashSet<int> problem_att = new HashSet<int>();
            _problem.Source.Attributes.ForEach(a => problem_att.Add(a.Tag));

            HashSet<int> solution_att = new HashSet<int>();
            solution.USet.ForEach(a => solution_att.Add(a));

            return (problem_att.IsSubsetOf(solution_att));
        }

        private SCPSolution RemoveRedundantSet(SCPSolution solution)
        {
            if (solution.Sets.Count == 1) return solution;

            SCPSolution improved = solution.Clone();
            List<int> blacklist = new List<int>();

            solution.ComputeAttributeRedundancies();
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
