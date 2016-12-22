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
                        a.Redundancy = GetAttributeRedundancy(a);
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
        
        public List<int> USet
        {
            get
            {
                if (Sets.Count == 0)
                    return null;
                List<int> uSet = new List<int>();
                Sets.ForEach(s => s.Attributes.ForEach(a => uSet.Add(a.Tag)));
                return uSet.OrderBy(a=>a).ToList(); 
            }
        }

        //public Dictionary<int,double> Catalog
        //{
        //    get
        //    {
        //        if (Sets.Count == 0)
        //            return null;
        //        Dictionary<int, double> catalog = new Dictionary<int, double>();
        //        List<Tuple<int, int>> map = new List<Tuple<int, int>>();
        //        Sets.ForEach(s => s.Attributes.ForEach(a => map.Add( new Tuple<int, int>(s.Tag, a.Tag))));
        //        foreach (var item in map)
        //        {
        //            if (catalog.ContainsKey(item.Item2) == false)
        //                catalog.Add(item.Item1, item.Item2);
        //            else
        //                catalog[item] = catalog[item]
        //        }

        //        return (Dictionary<int, double>)catalog.OrderBy(c=>c.Value);
        //    }
        //}

        public int[,] Catalog
        {
            get
            {
                if (Sets.Count == 0)
                    return null;
                int length = USet.Count;
                int[,] list = new int[2, length];
                Sets.ForEach(s =>
                {
                        s.Attributes.ForEach(a =>
                    {
                        list[0, length - 1] = s.Tag;
                        list[1, length - 1] = a.Tag;
                        length--;
                    });
                });
                return list;
            }
        }

        public Dictionary<int,int> AUFrequency
        {
            get
            {
                Dictionary<int, int> auFrequency = new Dictionary<int, int>();
                foreach (var item in USet)
                {
                    if (auFrequency.ContainsKey(item) == false)
                        auFrequency.Add(item, USet.Where(u => u == item).Count());
                }
                return auFrequency;
            }
        }

        public Dictionary<int, int> SUFrequency
        {
            get
            {
                Dictionary<int, int> suFrequency = new Dictionary<int, int>();
                foreach (var item in Sets)
                    suFrequency.Add(item.Tag, item.Attributes.Count);

                return suFrequency;
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

        public void Resetset()
        {
            Sets.ForEach(s =>
            {
                s.Visit = false;
                s.Frequency = s.Attributes.Count;
                s.Attributes.ForEach(a =>
                {
                    a.Visit = false;
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
                 int frequency = USet.Count(i => i == a.Tag);
                 if (frequency - 1 == 0)
                     a.Visit = false;
             });
            if (set.Attributes.Where(a => a.Visit == false).Count() > 0)
                CriticalList.Add(set);
            Sets.Remove(Sets.Find(s => s.Tag.ToString() == set.Tag.ToString()));
        }

        public int GetAttributeRedundancy(SCPAttribute attribute)
        {
            if (USet == null) return 0;
            return USet.Where(u => u == attribute.Tag).Count();
        }

        public void ComputeAttributeRedundancies()
        {
            Sets.ForEach(s =>
            {
                s.Attributes.ForEach(a1 =>
                {
                    a1.Redundancy = GetAttributeRedundancy(a1);
                });
            });
        }

        public void ComputeAttributeRedundancyPrice()
        {
            Sets.ForEach(set =>
            {
                set.Attributes.ForEach(a =>
                {
                    if (USet.Exists(u => u == a.Tag))
                    {
                        a.Cost = 0;
                        USet.Where(u => u == a.Tag).ToList().ForEach(i =>
                        {
                            double price = 0;
                            SCPSet ts = Sets.Find(s => s.Tag == i);
                            if (ts != null)
                                price = ts.Cost / ts.Attributes.Count;
                            a.Cost += price;
                        });

                    }
                });
            });
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
            USet.ForEach(u => universal.Add(u));
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

        internal double Overhead()
        {
            double overhead = 0;
            ComputeAttributeRedundancies();

            Sets = Sets.OrderByDescending(s => s.Cost).ToList();

            foreach (var set in Sets)
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
                    overhead += set.Cost;
            }

            return overhead;
        }

        public void Adapt(SCPSet set)
        {
            double gain = 0;
            double price = 0;

            Dictionary<int, int> suFreq = SUFrequency;
            int[,] catalog = Catalog;
            List<int> targets = new List<int>();

            price += set.Cost;
            foreach (var attribute in set.Attributes)
            {
                for (int i = 0; i < catalog.Length / 2; i++)
                {
                    if (catalog[1, i] == attribute.Tag)
                        targets.Add(catalog[0, i]);
                }
            }

            foreach (var t in targets)
            {
                suFreq[t]--;
            }

            foreach (var item in suFreq)
            {
                if (item.Value <= 0)
                    gain += Sets.Find(s => s.Tag == item.Key).Cost;
            }

            if (set.Cost - gain < 0)
            {
                Sets.Add(set);
                suFreq.Where(s1 => s1.Value == 0).ToList().ForEach(s => Sets.Remove(Sets.Find(s2 => s.Key == s2.Tag)));
            }
        }

        public double Delta(SCPSet set)
        {
            double gain = 0;
            double price = 0;

            Dictionary<int, int> suFreq = SUFrequency;
            int[,] catalog = Catalog;
            List<int> targets = new List<int>();

            price += set.Cost;
            foreach (var attribute in set.Attributes)
            {
                for (int i = 0; i < catalog.Length / 2; i++)
                {
                    if (catalog[1, i] == attribute.Tag)
                        targets.Add(catalog[0, i]);
                }
            }

            foreach (var t in targets)
            {
                suFreq[t]--;
            }

            foreach (var item in suFreq)
            {
                if (item.Value <= 0)
                    gain += Sets.Find(s=>s.Tag == item.Key).Cost;
            }

            return set.Cost - gain;
        }

        public double Delta(List<SCPSet> sets)
        {
            double gain= 0;
            double price = 0;
            Dictionary<int, int> suFreq = SUFrequency;
            int[,] catalog = Catalog;
            List<int> targets = new List<int>();

            foreach (var set in sets)
            {
                price += set.Cost;
                foreach (var attribute in set.Attributes)
                {
                    for (int i = 0; i < catalog.Length / 2; i++)
                    {
                        if (catalog[1, i] == attribute.Tag)
                            targets.Add(catalog[0, i]);
                    }
                }
            }

            foreach (var t in targets)
                suFreq[t]--;
           
            foreach (var item in suFreq)
            {
                if (item.Value <= 0)
                    gain += Sets.Find(s => s.Tag == item.Key).Cost;
            }

            return price - gain;
        }
    }
}
