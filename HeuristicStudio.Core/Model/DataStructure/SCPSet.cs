using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure
{
    public class SCPSet:ICloneable
    {
        public int Tag { get; set; }
        public bool Visit { get; set; }
        public double Cost { get; set; }
        public double Weight { get; set; }
        public int Frequency { set; get; }
        public int Confilict { get; set; }
        public double Overhead { get; set; }
        public List<int> Parents { get; set; }
        public double Probibility { get; set; }
        public bool Prime { get; set; }
        public int Freshness
        {
            get
            {
                int count = 0;
                if (Attributes == null) return 0;
                Attributes.ForEach(a => 
                {
                    if (a.Visit == false)
                        count++;
                    });
                return count;
            }
                }
        public List<SCPAttribute> Attributes { get; set; }

        public SCPSet()
        {
            Attributes = new List<DataStructure.SCPAttribute>();
        }

        public bool IsSubset(SCPSet set)
        {
            HashSet<int> me = new HashSet<int>();
            HashSet<int> sub = new HashSet<int>();
            Attributes.ForEach(a => me.Add(a.Tag));
            set.Attributes.ForEach(a => sub.Add(a.Tag));
            return me.IsSubsetOf(sub);
        }

        public bool IsSubset(List<int> set)
        {
            HashSet<int> main = new HashSet<int>();
            HashSet<int> sub = new HashSet<int>(set);
            Attributes.ForEach(a => main.Add(a.Tag));
            return sub.IsSubsetOf(main);
        }

        public bool IsCovering(List<int> set)
        {
            foreach(var item in set)
            {
                if (!Attributes.Exists(a => a.Tag.ToString() == item.ToString()))
                    return false;
            }
            return true;
        }

        public bool IsCovering(int[] set)
        {
            foreach (var item in set)
            {
                if (!Attributes.Exists(a => a.Tag.ToString() == item.ToString()))
                    return false;
            }
            return true;
        }

        public SCPSet Copy()
        {
            SCPSet copy = new SCPSet();
            copy.Attributes = new List<DataStructure.SCPAttribute>();
            copy.Tag = Tag;
            copy.Overhead = Overhead;
            copy.Confilict = Confilict;
            copy.Cost = Cost;
            copy.Frequency = Frequency;
            copy.Weight = Weight;
            copy.Probibility = Probibility;
            return copy;
        }

        public object Clone()
        {
            SCPSet clone = new SCPSet();
            Attributes.ForEach(a => clone.Attributes.Add((SCPAttribute)a.Copy()));
            clone.Tag = Tag;
            clone.Cost = Cost;
            clone.Overhead = Overhead;
            clone.Frequency = Frequency;
            clone.Confilict = Confilict;
            clone.Weight = Weight;
            clone.Probibility = Probibility;
            return clone;
        }
    }
}
