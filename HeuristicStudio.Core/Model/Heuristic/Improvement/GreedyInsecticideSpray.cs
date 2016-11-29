using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.DataStructure.SCP;
using HeuristicStudio.Core.Model.Problems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.Heuristic.Constructive
{
    //Based on the first improvment strategy
    public class GreedyInsecticideSpray : IImprovementHeuristic<SCPSolution>
    {
        private readonly Stopwatch sp = new Stopwatch();
        protected SCP _problem = null;
        protected SCPSolution _opoSolution;

        public int Iteration { get; set; }

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

        public SCPSolution OptimumSultion
        {
            get
            {
                return _opoSolution;
            }
        }

        public GreedyInsecticideSpray()
        {

        }

        public double Execute(IProblem problem)
        {
            _problem = ((SCP)problem);
            _opoSolution = (SCPSolution)_problem.Solution;
            _opoSolution.ResetWeights();

            sp.Restart();
            List<double> cost_history = new List<double>();
            while (true)
            {
                if (cost_history.Count > 3)
                    if (cost_history[cost_history.Count - 1] == cost_history[cost_history.Count - 2])
                        break;

                Spraying();
                cost_history.Add(_opoSolution.Cost);
                Iteration++;
            }
            //_opoSolution.CheckCriticalList();
            sp.Stop();
            return _opoSolution.Cost;
        }

        private void Spraying()
        {

            SCPAttribute attRedundancy = null;
            List<SCPSet> setRedundancy = new List<SCPSet>();
            _opoSolution.Sets.ForEach(s =>
            {
                s.Attributes.ForEach(a =>
                {
                    if (attRedundancy == null)
                        attRedundancy = a;
                    else
                        attRedundancy = (a.Frequency > attRedundancy.Frequency) ? a : attRedundancy;
                });
            });


            setRedundancy = _opoSolution.Sets.Where(s => s.Attributes.Contains(attRedundancy)).ToList();
            foreach (var sr in setRedundancy)
            {
                foreach (var attribute in sr.Attributes)
                {
                    if (attribute.Frequency == 1)
                    {
                        SCPSet set = _problem.Source.Sets.Find(s => s.Tag == attribute.GetCheapestSetTag());
                        _opoSolution.Sets.Add(set);
                        break;
                    }
                }
                _opoSolution.RemoveSet(sr);
                _opoSolution.ResetWeights();
            }
        }






    }

}

