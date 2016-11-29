using HeuristicStudio.Infrastructure.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Interfaces
{
    interface IDataBinder
    {
        void Bind<T>(IParser<T> parser);
    }
}
