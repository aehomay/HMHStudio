using HeuristicStudio.Core.Model;
using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.SCPData;
using HeuristicStudio.Core.Model.Problems;

namespace HeuristicStudio.Infrastructure.IO.Parsers
{
    public class MPCLSPParser : IParser<SCPDataSet>
    {
        public IProblem Problem
        {
            get
            {
                return _problem;
            }
        }

        protected MPCLSP _problem = null;

        public void Read(string path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string str = file.ReadLine();
           
        }
       
    }
}
