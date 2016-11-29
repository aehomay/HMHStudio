using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.DataStructure
{
    public class SCPAttribute:ICloneable
    {
        List<SCPSet> _usedIn = new List<SCPSet>();
        public double Cost { get; set; }
        public double Weight { get; set; }
        public int Tag { get; set; }
        public bool Visit { get; set; }
        public int Redundancy { get; set; }
        public double Probibility { get; set; }
        public int Frequency
        {
            set; get;
        }
        public List<SCPSet> UsedIn
        {
            get
            {
                return _usedIn;
            }

            set
            {
                _usedIn = value;
            }
        }

       
        public int GetCheapestSetTag()
        {
            return UsedIn.OrderBy(s => s.Cost).First().Tag;
        }

        public SCPSet GetCheapestSet()
        {
            return UsedIn.OrderBy(s => s.Cost).First();
        }

        public double GetCheapestCost()
        {
            return UsedIn.OrderBy(s => s.Cost).First().Cost;
        }

        public SCPAttribute Copy()
        {
            SCPAttribute copy = new SCPAttribute();
            copy.UsedIn = new List<SCPSet>();
            copy.Cost = Cost;
            copy.Tag = Tag;
            copy.Weight = Weight;
            copy.Frequency = Frequency;
            copy.Visit = Visit;
            copy.Probibility = 0;
            return copy;
        }

        public object Clone()
        {
            SCPAttribute clone = new SCPAttribute();
            clone.UsedIn = new List<SCPSet>();
            UsedIn.ForEach(s => clone.UsedIn.Add((SCPSet)s.Clone()));
            clone.Cost = Cost;
            clone.Tag = Tag;
            clone.Weight = Weight;
            clone.Frequency = Frequency;
            clone.Visit = Visit;
            clone.Probibility = Probibility;
            return clone;
        }
    }
}
