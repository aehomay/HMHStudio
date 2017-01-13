using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.SCPData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeuristicStudio.Core.Model.Problems
{
    public class SCP:IProblem
    {
        public SCPDataSet Source
        {
            get;set;
        }

        public Matrix<int> Matrix
        {
            get;set;
        }

        public SCPSolution Solution { get; set; }

        public SCP(MatrixSize size)
        {
            Matrix = new Matrix<int>(size);
            Solution = new SCPSolution();
        }

        public List<SCPSet> GetCandidateSets(SCPSet set)
        {
            List<SCPSet> candidates = new List<SCPSet>();
            foreach (var candidate in Source.Sets)
            {
                if (set.IsSubset(candidate))
                    candidates.Add(candidate);
            }
            return candidates;
        }

        public int ComputeConfilicts(SCPSet set)
        {
            int penalty = 0;
            Source.Sets.ForEach(s =>
            {
                penalty = 0;
                if (s.Tag.ToString() != set.Tag.ToString())
                {
                    s.Attributes.ForEach(a1 =>
                    {
                        set.Attributes.ForEach(a2 =>
                        {
                            if (a1.Tag.ToString() == a2.Tag.ToString())
                                penalty++;
                        });
                    });
                }
            });
            return penalty;
        }

        public void ComputeConfilicts()
        {
            Source.Sets.ForEach(s =>
            {
                s.Confilict = ComputeConfilicts(s);
            });
        }

        public bool IsPrimeSet(SCPSet set)
        {
            SCPSet scpset = Source.Sets.Where(s => s.Tag == set.Tag).First();
            foreach (var attribute in scpset.Attributes)
            {
                if (attribute.GetCheapestSet().Tag == scpset.Tag)
                    return true;
            }
            return false;
        }

        public void MarkPrimeSets()
        {
            Source.Attributes.ForEach(attribute =>
            {
                attribute.GetCheapestSet().Prime = true;
            });
        }

        public void MarkPrimeSets(SCPSolution solution)
        {
            solution.Sets.ForEach(set =>
            {
                set.Prime = IsPrimeSet(set);
            });
        }

        public IProblem Clone()
        {
            SCP scpp = new SCP(Matrix.Size);
            scpp.Matrix = (Matrix<int>)Matrix.Clone();
            scpp.Source = (SCPDataSet)Source.Clone();
            scpp.Solution = new SCPSolution() { Sets = Solution.Sets};
            return scpp;
        }

  
    }
}
