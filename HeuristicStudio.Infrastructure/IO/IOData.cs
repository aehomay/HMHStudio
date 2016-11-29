using HeuristicStudio.Core.Model.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicStudio.Infrastructure.IO
{
    public static class IOData
    {
        public static void ReadFileToMatrix<T>(IParser<T> format, string path)
        {
            format.Read(path);
        }
    }
}
