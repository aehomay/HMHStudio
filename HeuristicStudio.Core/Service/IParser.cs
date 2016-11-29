using HeuristicStudio.Core.Model;
using HeuristicStudio.Core.Model.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Infrastructure.IO
{
    public interface IParser<T>
    {
        IProblem Problem { get; }
        void Read(string path);
    }
}
