using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.Heuristic;
using HeuristicStudio.Core.Model.Problems;
using HeuristicStudio.Core.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.MetaHeuristic
{
    public class LocalSearch 
    {
        SCP _problem = null;
        SCPSolution _opSolution = null;
        SCPSolution _currSolution = null;
        Stopwatch _sp = new Stopwatch();

        public TimeSpan Elapsed
        {
            get
            {
                return _sp.Elapsed;
            }
        }

        public SCPSolution OptimumSultion
        {
            get
            {
                return _opSolution;
            }
        }

        public IProblem Problem
        {
            get
            {
                return _problem;
            }
        }

        public double Execute(IProblem problem)
        {
            _sp.Restart();
            Initializer(problem);
            _currSolution.ComputeAttributeRedundancies();
            List<double> cost_history = new List<double>();

            while (true)
            {
                if (cost_history.Count > 2)
                    if (cost_history[cost_history.Count - 1] == cost_history[cost_history.Count - 2])
                        break;

                //STEP#1
                _currSolution = RemoveRedundantSet(_currSolution);
                if (_currSolution.Cost < _opSolution.Cost)
                    _opSolution = _currSolution.Clone();


                //STEP#2
                ImproveByNeighborhood();
                if (_currSolution.Cost < _opSolution.Cost)
                    _opSolution = _currSolution.Clone();

                ////STEP#3
                ImproveBySecondNeighborhood();
                if (_currSolution.Cost < _opSolution.Cost)
                    _opSolution = _currSolution.Clone();


                cost_history.Add(_opSolution.Cost);
            }
            _sp.Stop();
            return OptimumSultion.Cost;
        }

        private void ImproveByNeighborhood()
        {
            int index = 0;
            int setcount = _currSolution.Sets.Count;

            _currSolution.ComputeAttributeRedundancies();
            while (index < setcount)
            {
                SCPSet target = _currSolution.Sets[index];
                List<int> need_to_covered = new List<int>();
                foreach (var attribute in target.Attributes)
                {
                    if (attribute.Redundancy <= 1)
                        need_to_covered.Add(attribute.Tag);
                }

                List<SCPSet> neighbors = _problem.Source.GetNeighbors(need_to_covered);

                foreach (var set in neighbors)
                {
                    if ((set.Cost - target.Cost) < 0)
                    {
                        if (_currSolution.Sets.Exists(s => s.Tag.ToString() == set.Tag.ToString()) == false)
                        {
                            _currSolution.Sets.Remove(target);
                            _currSolution.Sets.Add(set);
                            _currSolution.ComputeAttributeRedundancies();
                            index = 0;
                            break;
                        }
                    }
                }
                index++;
            }
        }

        private void ImproveBySecondNeighborhood()
        {
            int index = 0;
            int setcount = _currSolution.Sets.Count;
            List<int> need_to_covered = new List<int>();

            _currSolution.ComputeAttributeRedundancies();
            while (index < setcount)
            {
                SCPSet target = _currSolution.Sets[index];
                
                foreach (var attribute in target.Attributes)
                {
                    if (attribute.Redundancy <= 1)
                        need_to_covered.Add(attribute.Tag);
                }
                if (index % 2 == 0)
                {
                    index++;
                    continue;
                }

                List<SCPSet> neighbors = _problem.Source.GetNeighbors(need_to_covered);

                foreach (var set in neighbors)
                {
                    if ((set.Cost - target.Cost) < 0)
                    {
                        if (_currSolution.Sets.Exists(s => s.Tag.ToString() == set.Tag.ToString()) == false)
                        {
                            _currSolution.Sets.Remove(target);
                            _currSolution.Sets.Add(set);
                            _currSolution.ComputeAttributeRedundancies();
                            index = 0;
                            break;
                        }
                    }
                }

                need_to_covered.Clear();
                index++;
            }
        }

        private SCPSolution RemoveRedundantSet(SCPSolution solution)
        {
            SCPSolution improved = solution.Clone();
            List<int> blacklist = new List<int>();

            solution.ComputeAttributeRedundancies();

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

            blacklist.ForEach(b =>
            {
                SCPSet remove = improved.Sets.Find(s => s.Tag.ToString() == b.ToString());
                improved.Sets.Remove(remove);
                });
            if (improved.Cost < solution.Cost)
                return improved;
            else
                return solution;
        }

        private void Initializer(IProblem problem)
        {
            _problem = (SCP)problem;
            _currSolution = _problem.Solution.Clone();
            _opSolution = _currSolution.Clone();
        }




    }
}
