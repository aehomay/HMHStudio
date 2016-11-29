using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicStudio.Infrastructure.IO;
using System.Windows.Forms;
using HeuristicStudio.Infrastructure.IO.Parsers;

namespace HeuristicStudio.Interfaces.DataBinder
{
    public class TreeBinder : IDataBinder
    {
        TreeView _treeView = null;
        public TreeBinder(TreeView tv)
        {
            _treeView = tv;
        }
        public void Bind<T>(IParser<T> parser)
        {
            Bind(parser);
        }

        private void Bind(SCPParser scpParser)
        {
           
        }
    }
}
