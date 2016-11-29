using HeuristicStudio.Infrastructure.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeuristicStudio.Windows.Visualization
{
    public partial class fVisualization<T> : Form
    {
        IParser<T> _source;
        public fVisualization()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {

        }
    }
}
