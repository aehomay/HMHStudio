using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.Problems;
using System.Diagnostics;
using HeuristicStudio.Core.Model.DataStructure.SCP;

namespace HeuristicStudio.Core.Model.Heuristic
{
    public class SCPImprovementGreedy : IConstructiveHeuristic
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


        public SCPImprovementGreedy()
        {
            FSolution = new SCPSolution();
        }

        public double Execute(IProblem problem)
        {
            _problem = (SCP)problem;

            HorizantalSelection();

            FSolution = RemoveRedundantSet((SCPSolution)FSolution);
            if (IsFeasible((SCPSolution)FSolution) == false)
                _problem.Solution = null;
            else
                _problem.Solution = (SCPSolution)FSolution;
            
            return _problem.Solution.Cost;
        }

     
        private void HorizantalSelection()
        {
            sp.Restart();

            SCPDataSet source = (SCPDataSet)_problem.Source.Clone();
           // source.Attributes = AttributeProbability(source.Attributes,source.Sets.Count);
            //source.Sets = Weighting(source.Sets);

            //List<SCPSet> candidates = GetCandidates(source.Sets);
            Stack<SCPSet> starts = new Stack<SCPSet>(source.Sets);
            List<SCPSolution> solutions = new List<SCPSolution>();

            while (starts.Count > 0)
            {
                starts.Peek().Visit = true;
               List<SCPSet> best = Fitness(new List<SCPSet>() { starts.Pop() }, ((SCPDataSet)source.Clone()).Sets);
                SCPSolution solution = new SCPSolution();
                solution.Sets = best;
                solutions.Add(RemoveRedundantSet(solution));
            }

            FSolution = solutions.OrderBy(s => s.Cost).First();

            sp.Stop();

        }

        private List<SCPSet> Fitness(List<SCPSet> sets, List<SCPSet> union)
        {
            if (IsFeasible(sets)) return sets;

            HashSet<int> attributes = new HashSet<int>();
            double price = 0.0;
            sets.ForEach(s =>
            {
                price += s.Cost;
                s.Attributes.ForEach(a => attributes.Add(a.Tag));
            });
            union.ForEach(set => 
            {
                int frequency = set.Attributes.Select(a => a.Tag).Except(attributes).Count() + attributes.Count;
                set.Overhead = (set.Cost + price) / frequency;
            });
            union = union.OrderBy(u => u.Overhead).ToList();

            SCPSet best = union.First();
            best.Visit = true;
            HashSet<int> additional = new HashSet<int>();
            best.Attributes.ForEach(a => additional.Add(a.Tag));

            if(additional.IsSubsetOf(attributes) == false)
                sets.Add(best);
            union = union.Where(set => set.Visit == false).ToList();

           return Fitness(sets, union);
        }

        private List<SCPAttribute> AttributeProbability(List<SCPAttribute> attributes,int n)
        {
            attributes.ForEach(a=>a.Probibility = a.UsedIn.Count / (double)n);
            return attributes;
        }

        private List<SCPSet> GetCandidates(List<SCPSet> sets)
        {
            List<SCPSet> candidates = new List<SCPSet>();
            double minC = sets.Min(s => s.Weight);
            sets.ForEach(set => 
            {
                if (set.Weight == minC)
                    candidates.Add(set);
            });
            candidates.ForEach(s =>
            {
                double p = 0.0;
                s.Attributes.ForEach(a => p += a.Probibility);
                s.Probibility = p;
            });
            candidates = candidates.OrderBy(c => c.Probibility).ToList();
            return candidates;
        }

        private List<SCPSet> Weighting(List<SCPSet> sets)
        {
            sets.ForEach(s =>
            {
                s.Weight = (s.Cost / s.Frequency);
            });
            sets = sets.Where(s=> s.Visit == false).OrderBy(s => s.Weight).ThenByDescending(s=>s.Frequency).ToList();
            return sets;
        }

        private bool IsFeasible(SCPSolution solution)
        {

            HashSet<int> problem_att = new HashSet<int>();
            _problem.Source.Attributes.ForEach(a => problem_att.Add(a.Tag));

            HashSet<int> solution_att = new HashSet<int>();
            solution.USet.ForEach(a => solution_att.Add(a));

            return (problem_att.IsSubsetOf(solution_att));
        }

        private bool IsFeasible(List<SCPSet> solution)
        {

            HashSet<int> problem_att = new HashSet<int>();
            _problem.Source.Attributes.ForEach(a => problem_att.Add(a.Tag));

            HashSet<int> solution_att = new HashSet<int>();
            solution.ForEach(s => s.Attributes.ForEach(a=> solution_att.Add(a.Tag)));

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
