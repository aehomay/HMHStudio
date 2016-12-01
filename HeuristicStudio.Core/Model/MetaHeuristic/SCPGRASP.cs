using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.DataStructure.SCP;
using HeuristicStudio.Core.Model.Heuristic;
using HeuristicStudio.Core.Model.Problems;
using HeuristicStudio.Core.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HeuristicStudio.Core.Model.MetaHeuristic
{
    public class SCPGRASP : IConstructiveHeuristic,IHeuristic
    {
        private readonly Stopwatch sp = new Stopwatch();
        protected double _alpha, _epsilon;
        protected List<SCPSet> _solution;
        protected double _totalCost = 0;
        protected SCPDataSet _scpData = null;
        protected SCP _problem = null;

        public double Alpha
        {
            get
            {
                return _alpha;
            }

            set
            {
                _alpha = value;
            }
        }
        public double Epsilon
        {
            get
            {
                return _epsilon;
            }

            set
            {
                _epsilon = value;
            }
        }
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

        public SCPGRASP(double alpha, double epsilon)
        {
            _solution = new List<SCPSet>();
            _alpha = alpha;
            _epsilon = epsilon;
        }

        public IProblem CloneSolvedProblem()
        {
            IProblem problem = this.Problem.Clone();
            return problem;
        }

        public double Execute(IProblem problem)
        {
            sp.Restart();
            _problem = (SCP)problem;
            int coveredatt, setCount, attCount;
            coveredatt = 0;
            List<SCPSet> sets = _problem.Source.Sets;
            List<SCPAttribute> attributes = _problem.Source.Attributes;

            setCount = sets.Count;
            attCount = attributes.Count;

            double c_min = sets.Min(a => a.Cost);

            while (coveredatt < attCount)
            {
                double e_min = 0, e_max = 0, e_limit = 0;
                SCPSet k;
                List<SCPSet> rcl = new List<SCPSet>();

                for (int i = 0; i < setCount; i++)
                {
                    if (sets[i].Frequency > 0)
                    {
                        sets[i].Weight = sets[i].Frequency / (1 + sets[i].Cost - c_min);
                        if (i > 0)
                        {
                            if (sets[i].Weight < e_min) e_min = sets[i].Weight;
                            if (sets[i].Weight > e_max) e_max = sets[i].Weight;
                        }
                        else
                            e_min = e_max = sets[i].Weight;
                    }
                }

                e_limit = e_min + _alpha * (e_max - e_min);

                for (int i = 0; i < setCount; i++)
                {
                    if (sets[i].Frequency > 0 && sets[i].Weight + _epsilon >= e_limit)
                        rcl.Add(sets[i]);
                }

                if (rcl.Count == 0) return 0;

                k = rcl[new Random().Next(0, rcl.Count) % rcl.Count];
                _solution.Add(k);
                _totalCost += k.Cost;
                k.Attributes.ForEach(a1 =>
                {
                    SCPAttribute attribute = _problem.Source.Attributes.Find(a2 => a2.Tag == a1.Tag);
                    if (attribute != null)
                    {
                        if (attribute.Visit == false)
                        {
                            coveredatt++;
                            attribute.UsedIn.ForEach(s1 =>
                            {
                                s1.Frequency--;
                            });
                        }
                        attribute.Visit = true;
                    }
                });
            }
            sp.Stop();
            _problem.Solution = new SCPSolution() { Sets = _solution};
            return _totalCost;
        }

      
    }
}
