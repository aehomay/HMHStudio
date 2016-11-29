using System;
using HeuristicStudio.Core.Model;
using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.DataStructure.SCP;
using HeuristicStudio.Core.Model.Problems;

namespace HeuristicStudio.Infrastructure.IO.Parsers
{
    public class SCPParser : IParser<SCPDataSet>
    {
        public IProblem Problem
        {
            get
            {
                return _problem;
            }
        }

        protected SCP _problem = null;

        public void Read(string path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string str = file.ReadLine();
            _problem = new SCP(new MatrixSize() { X = int.Parse((str.Trim().Split(' ')[0])), Y = int.Parse(str.Trim().Split(' ')[1]) });
            _problem.Source = new SCPDataSet();
            str = "";
            for (int index = 0; index < _problem.Matrix.Size.Y;)
            {
                int read = file.Read();

                if (read == 32 || read == 10)
                {
                    if (str != "")
                    {
                        SCPSet set = new SCPSet() { Tag = ++index, Cost = int.Parse(str) };
                        _problem.Source.Sets.Add(set);
                        str = "";
                    }
                    continue;
                }
                else
                    str += (read - 48);
            }

            DataBinding(true, 0, _problem.Matrix.Size.X, file);
        }
        private void DataBinding(bool initialize, int attribte, int size, System.IO.StreamReader file)
        {
            if (attribte > _problem.Matrix.Size.X) return;
            string str = "";
            SCPAttribute att = null;

            for (int index = 0; index < size;)
            {
                int read = file.Read();
                if (read == 32 || read == 10)
                {
                    if (str != "")
                    {
                        if (initialize)
                        {
                            DataBinding(false, ++attribte, int.Parse(str), file);
                        }
                        else
                        {
                            if (att == null)
                            {
                                att = new SCPAttribute();
                                _problem.Source.Attributes.Add(att);
                                att.Tag = attribte;
                            }

                            att.UsedIn.Add(_problem.Source.Sets[int.Parse(str) - 1]);

                            _problem.Matrix.Write(1, att.Tag - 1, int.Parse(str) - 1);
                            _problem.Source.Sets[int.Parse(str) - 1].Attributes.Add(att);
                            _problem.Source.Sets[int.Parse(str) - 1].Frequency++;

                    
                            index++;
                        }
                        str = "";
                    }
                    continue;
                }
                else if
                    (read == -1) break;//End of file
                else
                    str += (read - 48);
            }
            initialize = true;
        }
    }
}
