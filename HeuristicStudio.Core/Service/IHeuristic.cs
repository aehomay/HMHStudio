using HeuristicStudio.Core.Model;
using HeuristicStudio.Core.Model.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Service
{
    public interface IHeuristic
    {
        IProblem Problem { get;}
        TimeSpan Elapsed { get; }
        double Execute(IProblem problem);
    }
}
