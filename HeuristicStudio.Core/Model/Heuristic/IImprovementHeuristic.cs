using HeuristicStudio.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Core.Model.Heuristic
{
    public interface IImprovementHeuristic<T>: IHeuristic
    {
        T OptimumSultion { get; }
    }
}
