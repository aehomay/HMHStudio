using System;
using System.Collections.Generic;
using System.Linq;

namespace HeuristicStudio.Core.Model.DataStructure.SCP
{

    public class SCPDataSet : IDataSource,ICloneable
    {
        private bool _weighting = false;
        //public Dictionary<set index, set>
        public List<SCPAttribute> Attributes { get; set; }

        //Dictionary<attribute index, {attribute cost,frequency}>
        public List<SCPSet> Sets { get; set; }

        public int Size
        {
            get;

            set;
        }

        public SCPDataSet()
        {
            Attributes = new List<SCPAttribute>();
            Sets = new List<SCPSet>();
        }

        public object Clone()
        {
            SCPDataSet copy = new SCPDataSet();
            copy.Size = Size;
            Sets.ForEach(s => copy.Sets.Add((SCPSet)s.Copy()));
            Attributes.ForEach(a => copy.Attributes.Add(a.Copy()));

            Sets.ForEach(s1 => 
            {
                SCPSet set = copy.Sets.Find(s2 => s2.Tag == s1.Tag);
                s1.Attributes.ForEach(a1 => 
                {
                    SCPAttribute attribute = copy.Attributes.Find(a2 => a2.Tag == a1.Tag);
                    set.Attributes.Add(attribute);
                });
            });

            Attributes.ForEach(a1 =>
            {
                SCPAttribute attribute = copy.Attributes.Find(a2 => a2.Tag == a1.Tag);
                a1.UsedIn.ForEach(s1 => attribute.UsedIn.Add( copy.Sets.Find(s2=>s2.Tag == s1.Tag)));
            });

            return copy;
        }

        public SCPSet GetLightestSet()
        {
            if (_weighting == false) Weighting();
            return Sets.FirstOrDefault();
        }

        public SCPSet GetLightestNotVisitedSet()
        {
            if (_weighting == false) Weighting();
            return Sets.Where(s => s.Visit == false).FirstOrDefault();
        }

        public List<SCPSet> GetNeighbors(SCPSet set)
        {
           List<SCPSet> subsets = new List<SCPSet>();
           
           foreach(var s in Sets)
            {
                bool subest = set.IsSubset(s);
                if(subest)
                    subsets.Add(s);
            }
            return subsets;
        }

        public List<SCPSet> GetNeighbors(List<int> subset)
        {
            List<SCPSet> subsets = new List<DataStructure.SCPSet>();
            if (subset.Count > 0)
            {
                Sets.ForEach(s =>
                {
                    if (s.IsCovering(subset))
                        subsets.Add(s);
                });
            }
            return subsets;
        }

        public List<SCPSet> GetNeighbors(int[] subset)
        {
            List<SCPSet> subsets = new List<DataStructure.SCPSet>();
            if (subset.Length > 0)
            {
                Sets.ForEach(s =>
                {
                    if (s.IsCovering(subset))
                        subsets.Add(s);
                });
            }
            return subsets;
        }

        public void Weighting()
        {
            _weighting = true;
           Sets.ForEach(s =>
            {
                s.Weight = (s.Cost / s.Frequency);
            });
            Sets = Sets.OrderBy(s => s.Weight).ThenByDescending(s=>s.Frequency).ToList();
        }

        public void ResetWeighting()
        {
            _weighting = true;
            Sets.ForEach(s =>
            {
                s.Frequency = s.Attributes.Count;
                s.Weight = (s.Cost / s.Frequency);
            });
            Sets = Sets.OrderBy(s => s.Weight).ThenByDescending(s => s.Frequency).ToList();
        }

        internal void CalculateProbibilityOfAttributes()
        {
            Attributes.ForEach(a =>
            {
                a.Probibility = 1 - ((double)1 / (double)a.UsedIn.Count);
            });
        }
    }
}

