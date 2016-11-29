using System;
using System.Collections.Generic;
using System.Linq;

namespace HeuristicStudio.Core.Model.DataStructure
{
    public class SCPSolution:ISolution
    {

        
        public List<SCPSet> CriticalList { get; set; }

        public List<Tuple<double,int>> LostedAttributes {
            get
            {
                List<Tuple<double, int>> died = new List<Tuple<double, int>>();
                CriticalList.ForEach(s =>
                {
                    s.Attributes.ForEach(a =>
                    {
                        a.Redundancy = ComputeAttributeRedundancy(a);
                        if (a.Redundancy <= 0)
                        {
                            if (died.Exists(d => d.Item2 == a.Tag) == false)
                                died.Add(new Tuple<double, int>(a.Cost, a.Tag));
                        }
                    });
                });
                return died.Distinct().ToList();
            }
        }
        public List<SCPSet> Sets { get; set; }
        public double Cost
        {

            get
            {
                double cost = 0;
                if (Sets != null)
                    Sets.ForEach(s => cost += s.Cost);
                return cost;
            }
        }
        

        public List<Tuple<int,int>> USet
        {
            get
            {
                if (Sets.Count == 0)
                    return null;
                List<Tuple<int, int>> uSet = new List<Tuple<int, int>>();
                Sets.ForEach(s => s.Attributes.ForEach(a => uSet.Add( new Tuple<int, int>(s.Tag, a.Tag))));
                return uSet.OrderBy(a => a).ToList(); 
            }
        }

        public SCPSolution()
        {
            CriticalList = new List<SCPSet>();
            Sets = new List<SCPSet>();
        }

        public void ResetWeights()
        {
            Sets.ForEach(s =>
            {
                s.Frequency = s.Attributes.Count;
                s.Attributes.ForEach(a =>
                {
                    a.Frequency = 0;
                    a.Weight = 0;
                });
            });
            Sets.ForEach(s =>
            {
                s.Weight = s.Cost / s.Frequency;
                s.Attributes.ForEach(a =>
                {
                    a.Frequency++;
                });
            });
        }

        public void RemoveSet(SCPSet set)
        {
            set.Attributes.ForEach(a =>
             {
                 int frequency = USet.Count(i => i.Item2 == a.Tag);
                 if (frequency - 1 == 0)
                     a.Visit = false;
             });
            if (set.Attributes.Where(a => a.Visit == false).Count() > 0)
                CriticalList.Add(set);
            Sets.Remove(Sets.Find(s => s.Tag.ToString() == set.Tag.ToString()));
        }

        public int ComputeConfilict(SCPSet set)
        {
            int penalty = 0;
            if (USet == null) return 0;
            set.Attributes.ForEach(a => 
            {
                if (USet.Exists(u=>u.Item2 ==  a.Tag))
                    penalty++;
            });
            return penalty;
        }

        public int ComputeAttributeRedundancy(SCPAttribute attribute)
        {
            int redundancy = 0;
            if (USet == null) return 0;
            USet.ForEach(a => 
            {       
                if (attribute.Tag == a.Item2)
                    redundancy++;
            });
            return redundancy;
        }

        public void ComputeAttributeRedundancy()
        {
            Sets.ForEach(s =>
            {
                s.Attributes.ForEach(a1 =>
                {
                    a1.Redundancy = ComputeAttributeRedundancy(a1);
                });
            });
        }

    

        public void ComputeAttributeRedundancyPrice()
        {
            Sets.ForEach(set =>
            {
                set.Attributes.ForEach(a =>
                {
                    if (USet.Exists(u => u.Item2 == a.Tag))
                    {
                        a.Cost = 0;
                        USet.Where(u => u.Item2 == a.Tag).ToList().ForEach(i =>
                        {
                            double price = 0;
                            SCPSet ts = Sets.Find(s => s.Tag == i.Item1);
                            if (ts != null)
                                price = ts.Cost / ts.Attributes.Count;
                            a.Cost += price;
                        });

                    }
                });
            });
        }

        public void ComputeSetConfilict()
        {
            Sets.ForEach(s => 
            {
                s.Attributes.ForEach(a => 
                {
                    s.Confilict += USet.Count(i => i.Item2 == a.Tag);
                });
                s.Confilict -= s.Frequency;
            });
        }

        public double ComputeSetRedundancyPrice(SCPSet set)
        {
            double total_price = 0;
            if (USet != null)
            {
                set.Attributes.ForEach(a =>
                {
                    if (USet.Exists(u => u.Item2 == a.Tag))
                    {
                        USet.Where(u => u.Item2 == a.Tag).ToList().ForEach(i =>
                        {
                            double price = 0;
                            SCPSet ts = Sets.Find(s => s.Tag == i.Item1);
                            if (ts != null)
                                price = ts.Cost / ts.Attributes.Count;
                            total_price += price;
                        });

                    }
                });
            } 
            return total_price;
        }

        public SCPSolution Clone()
        {
            List<SCPAttribute> attributes = new List<DataStructure.SCPAttribute>();
            Sets.ForEach(s => s.Attributes.ForEach(a1 => 
            {
                if (attributes.Exists(a2 => a2.Tag == a1.Tag) == false)
                    attributes.Add(a1.Copy());
            }));

            SCPSolution clone = new SCPSolution();
            Sets.ForEach(s =>
            {
                SCPSet set = s.Copy();
                clone.Sets.Add(set);
                s.Attributes.ForEach(a1 =>
                {
                    SCPAttribute attribute = attributes.Find(a2 => a2.Tag == a1.Tag);
                    attribute.UsedIn.Add(set);
                    set.Attributes.Add(attribute);
                });
            });

            clone.CriticalList = new List<DataStructure.SCPSet>(CriticalList);
            
            return clone;
        }

        public SCPSet GetMostExpensiveSet()
        {
            return Sets.OrderByDescending(s => s.Cost).FirstOrDefault();
        }

        public bool IsSubset(SCPSet set)
        {
            if (USet == null)
                return false;

            HashSet<int> universal = new HashSet<int>();
            USet.ForEach(u => universal.Add(u.Item2));
            HashSet<int> subset = new HashSet<int>();
            set.Attributes.ForEach(a => subset.Add(a.Tag));
            bool r = subset.IsSubsetOf(universal);
            return r;
        }

        public List<Tuple<int, int>> GetFrequencyList()
        {
            List<Tuple<int, int>> frequencyList = new List<Tuple<int, int>>();
            Sets.ForEach(s =>
            {
                s.Attributes.ForEach(a =>
                {
                    frequencyList.Add(new Tuple<int, int>(s.Tag, a.Tag));
                });
            });
            return frequencyList;
        }
    }
}
