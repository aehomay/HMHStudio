using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.SCPData;
using HeuristicStudio.Core.Model.Problems;
using HeuristicStudio.Core.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


/*
 *               
 *              n!
  C(n,m) = -------------
           m! (n - m)! 
 */


namespace HeuristicStudio.Core.Model.Heuristic.Constructive
{
    struct Panel
    {
        public bool Useless;
        public int SetTag;
        public double SetCost;
        public int AttributeTag;
        public double AttributeCost;
    }

    //Based on the first improvment strategy
    public class SmartLocalSearch 
    {
        SCP _problem = null;
        SCPSolution _opSolution = null;
        SCPSolution _currSolution = null;
        Stopwatch _sp = new Stopwatch();
        

        public TimeSpan Elapsed
        {
            get
            {
                return _sp.Elapsed;
            }
        }

        public SCPSolution OptimumSultion
        {
            get
            {
                return _opSolution;
            }
        }

        public IProblem Problem
        {
            get
            {
                return _problem;
            }
        }

        public double Execute(IProblem problem)
        {
            _sp.Restart();
            Initializer(problem);
            _currSolution.ComputeAttributeRedundancies();
            List<double> cost_history = new List<double>();
  

            while (true)
            {
                if (cost_history.Count > 2)
                    if (cost_history[cost_history.Count - 1] == cost_history[cost_history.Count - 2])
                        break;

                //STEP#1
                ImproveByOneNeighborhood();
                if (_currSolution.Cost < _opSolution.Cost)
                    _opSolution = _currSolution.Clone();

                //STEP#2
                ImproveByTwoNeighborhoods();
                if (_currSolution.Cost < _opSolution.Cost)
                    _opSolution = _currSolution.Clone();

                //STEP#3
                ImproveByThreeNeighborhoods();
                if (_currSolution.Cost < _opSolution.Cost)
                    _opSolution = _currSolution.Clone();

                //STEP#4
                RemoveRedundantSet();
                if (_currSolution.Cost < _opSolution.Cost)
                    _opSolution = _currSolution.Clone();

                cost_history.Add(_opSolution.Cost);
            }



            _sp.Stop();
            return OptimumSultion.Cost;
        }

        private void ImproveByOneNeighborhood()
        {
            int index = 0;
            int setcount = _currSolution.Sets.Count;

            _currSolution.ComputeAttributeRedundancies();
            while (index < setcount)
            {
                SCPSet target = _currSolution.Sets[index];
                List<int> need_to_covered = new List<int>();
                foreach (var attribute in target.Attributes)
                {
                    if (attribute.Redundancy <= 1)
                        need_to_covered.Add(attribute.Tag);
                }

                List<SCPSet> neighbors = _problem.Source.GetNeighbors(need_to_covered);

                foreach (var set in neighbors)
                {
                    if ((set.Cost - target.Cost) < 0)
                    {
                        if (_currSolution.Sets.Exists(s => s.Tag.ToString() == set.Tag.ToString()) == false)
                        {
                            _currSolution.Sets.Remove(target);
                            _currSolution.Sets.Add(set);
                            _currSolution.ComputeAttributeRedundancies();
                            index = 0;
                            break;
                        }
                    }
                }
                index++;
            }
        }

        private void ImproveByTwoNeighborhoods()
        {
            //Create panel
            List<Panel> panels = null;
            panels = GenerateAuctionPanel();

            //Create possibility of pairs in the panel by making possibility matrix
            Matrix<int> matrix = new Matrix<int>();
            matrix = GeneratePossibilityMatrix(panels);

            //TODO: Create new pair (set) and compair with neighbors to see if it is profitable exchange
            List<SCPSet> candidates = GenerateTwoNeighborCandidateSet(matrix, panels);

            List<SCPSet> removes = new List<SCPSet>();
            List<SCPSet> adds = new List<SCPSet>();

            _currSolution.Sets = _currSolution.Sets.OrderBy(s => s.Tag).ToList();
            candidates.ForEach(set => 
            {
                List<SCPSet> neighbors = _problem.Source.GetNeighbors(set);
                SCPSet best = neighbors.OrderBy(n => n.Cost).First();
                if (set.Cost > best.Cost)
                {
                    adds.Add(best);
                    removes.Add(set);
                }
            });

            //Add
            adds.ForEach(a => _currSolution.Sets.Add(a));

            //Remove
            removes.ForEach(b =>
            {
                b.Parents.ForEach(p =>
                {
                    SCPSet set = _currSolution.Sets.Find(s => s.Tag == p);
                    if(set != null)
                        _currSolution.RemoveSet(set);
                });
            });

            //Fix the died attributes by finding a cheapest set for them
            List<SCPSet> recovery =  RecoverDeadAttributes(_currSolution.LostedAttributes.Select(l=>l.Item2).ToList());
            _currSolution.Sets.AddRange(recovery);
        }

        private List<SCPSet> RecoverDeadAttributes(List<int> losted)
        {
            List<SCPSet> list = new List<SCPSet>();
            Random rnd = new Random();
            bool recovered = false;
            int split = 0;

            while (recovered == false)
            {
                List<List<int>> partitions = new List<List<int>>();

                if (split != 0)
                {
                    int size = (int)Math.Round(losted.Count / (double)split);
                    for (int i = 0; i < split; i++)
                    {
                        partitions.Add(losted.Skip(i * size).Take(size).ToList());
                    }
                }
                else
                    partitions.Add(losted);

                partitions.ForEach(p =>
                {
                    List<SCPSet> neighbors = _problem.Source.GetNeighbors(p);
                    
                    if (neighbors.Count > 0)
                    {
                        SCPSet best = neighbors.OrderBy(n => n.Cost).First();
                        list.Add(best);
                        losted = losted.Except(p).ToList();
                    }
                });
                if(losted.Count == 0)
                    recovered = true;
                else
                    split = rnd.Next(1, (losted.Count / rnd.Next(1,losted.Count)) + 1);
            }
            
            return list;
        }

        private void ImproveByThreeNeighborhoods()
        {
            //Create panel
            List<Panel> panels = null;
            panels = GenerateAuctionPanel();

            //Create possibility of pairs in the panel by making possibility matrix
            Matrix<int> matrix = new Matrix<int>();
            matrix = GeneratePossibilityMatrix(panels);

            //TODO: Create new pair (set) and compair with neighbors to see if it is profitable exchange
            List<SCPSet> candidates = GenerateMultiNeighborCandidateSet(matrix, panels);

            List<SCPSet> removes = new List<SCPSet>();
            List<SCPSet> adds = new List<SCPSet>();

            _currSolution.Sets = _currSolution.Sets.OrderBy(s => s.Tag).ToList();
            candidates.ForEach(set =>
            {
                List<SCPSet> neighbors = _problem.Source.GetNeighbors(set);
                if (neighbors.Count > 1)
                {
                    SCPSet best = neighbors.OrderBy(n => n.Cost).First();
                    if (set.Cost > best.Cost)
                    {
                        adds.Add(best);
                        removes.Add(set);
                    }
                }
            });

            //Add
            adds.ForEach(a => _currSolution.Sets.Add(a));

            //Remove
            removes.ForEach(b =>
            {
                b.Parents.ForEach(p =>
                {
                    _currSolution.ComputeAttributeRedundancies();
                    SCPSet set = _currSolution.Sets.Find(s => s.Tag == p);
                    if (set != null)
                        _currSolution.RemoveSet(set);
                });
            });

            //Fix the died attributes by finding a cheapest set for them
            List<SCPSet> recovery = RecoverDeadAttributes(_currSolution.LostedAttributes.Select(l=>l.Item2).ToList());
            _currSolution.Sets.AddRange(recovery);
        }

        private List<SCPSet> GenerateTwoNeighborCandidateSet(Matrix<int> matrix, List<Panel> panels)
        {
            List<SCPSet> candidates = new List<SCPSet>();
            for (int i = 0; i < matrix.Size.X; i++)
            {
                for (int j = 0; j < matrix.Size.Y; j++)
                {
                    if (matrix.Read(i, j) >= 1)
                    {
                        //Generate new candidate set by combination of possible pairs
                        SCPSet candidate = CreateNewVirtualSet(new List<Constructive.Panel>() { panels[i],panels[j] });
                        candidates.Add(candidate);
                    }
                }
            }
            
            return candidates;
        }

        private List<SCPSet> GenerateMultiNeighborCandidateSet(Matrix<int> matrix, List<Panel> panels)
        {
            List<SCPSet> candidates = new List<SCPSet>();
            for (int i = 0; i < matrix.Size.X; i++)
            {
                List<Panel> pl = new List<Panel>();
                for (int j = 0; j < matrix.Size.Y; j++)
                {
                    if (matrix.Read(i, j) >= 2)
                    {
                        //Generate new candidate set by combination of possible pairs
                        if (pl.Contains(panels[i]) == false)
                            pl.Add(panels[i]);
                        if (pl.Contains(panels[j]) == false)
                            pl.Add(panels[j]);
                    }
                }
                if (pl.Count > 0)
                {
                    SCPSet candidate = CreateNewVirtualSet(pl);
                    candidates.Add(candidate);
                }
            }

            //Make the cost of each set realistic by not take into account cost of each set more than once in each pair
            //List<int> visited = new List<int>();
            //candidates = candidates.OrderByDescending(c => c.SetCost).ToList();
            //candidates.ForEach(c =>
            //{
            //    c.Parents.ForEach(p =>
            //    {
            //        if (visited.Contains(p))
            //            c.SetCost -= _problem.Source.Sets.Find(s => s.SetTag == p).SetCost;
            //        else
            //            visited.Add(p);
            //    });
            //});
            return candidates;
        }

        private SCPSet CreateNewVirtualSet(List<Panel> list)
        {
            SCPSet candidate = new SCPSet();
            candidate.Tag = 0;
     
            list.ForEach(i => 
            {
                if (candidate.Parents == null)
                {
                    candidate.Parents = new List<int>();
                    candidate.Parents.Add(i.SetTag);
                    candidate.Cost += i.SetCost;
                }
                else if (candidate.Parents.Contains(i.SetTag) == false) //Don`t add the same set more than twice and don`t cost the same set more than once
                {
                    candidate.Parents.Add(i.SetTag);
                    candidate.Cost += i.SetCost;
                }
                SCPAttribute attribute = new SCPAttribute();
                attribute.Tag = i.AttributeTag;
                attribute.Cost = i.AttributeCost;
                candidate.Attributes.Add(attribute);
            });
            return candidate;
        }

        private Matrix<int> GeneratePossibilityMatrix(List<Panel> panels)
        {
            Matrix<int> matrix = new Matrix<int>(panels.Count,panels.Count);
      
            //Initialize halph of matrix
            for (int i = 0; i < matrix.Size.X; i++)
            {
                for (int j = i + 1; j < matrix.Size.Y; j++)
                {
                    int[] set = new int[] { panels[i].AttributeTag, panels[j].AttributeTag };
                    int count = _problem.Source.GetNeighbors(set).Count;
                    matrix.Write(count, i, j);
                }
            }

            return matrix;
        }

        private List<Panel> GenerateAuctionPanel()
        {
            int index = 0;
            int setcount = _currSolution.Sets.Count;
            List<Panel> panel = new List<Panel>();

            _currSolution.ComputeAttributeRedundancies();
            while (index < setcount)
            {
                SCPSet target = _currSolution.Sets[index];
                List<int> need_to_covered = new List<int>();
                foreach (var attribute in target.Attributes)
                {
                    if (attribute.Redundancy <= 1)
                        need_to_covered.Add(attribute.Tag);
                }

                List<SCPSet> neighbors = _problem.Source.GetNeighbors(need_to_covered);

                if (neighbors.Count > 1)
                {
                    need_to_covered.ForEach(attribute =>
                    {
                        int set = target.Tag;
                        double setcost = target.Cost;
                        double attcost = target.Cost / target.Attributes.Count;

                        Panel tuple = new Panel() { Useless = true, SetTag = set, SetCost = setcost, AttributeTag = attribute, AttributeCost = attcost };
                        panel.Add(tuple);
                    });
                }

                index++;
            }
            return panel;
        }

        private void RemoveRedundantSet()
        {
            SCPSolution improved = _currSolution.Clone();
            List<int> blacklist = new List<int>();

            _currSolution.ComputeAttributeRedundancies();

            foreach (var set in _currSolution.Sets)
            {
                bool useless = true;

                foreach (var a in set.Attributes)
                {
                    if (a.Redundancy <= 1)
                    {
                        useless = false;
                        break;
                    }
                }
                if (useless)
                {
                    set.Attributes.ForEach(a => a.Redundancy--);
                    blacklist.Add(set.Tag);
                }
            }

            blacklist.ForEach(b => improved.Sets.Remove(improved.Sets.Find(s => s.Tag.ToString() == b.ToString())));
            if (improved.Cost < _currSolution.Cost)
                _currSolution = improved;
        }

        private void Initializer(IProblem problem)
        {
            _problem = (SCP)problem;
            _currSolution = _problem.Solution.Clone();
            _opSolution = _currSolution.Clone();
        }

        private bool IsFeasible(SCPSolution solution)
        {
            if (solution.Sets.Count <= 0) return false;
            #region Step#1 Exhustive test
            List<int> attributes = new List<int>();
            solution.Sets.ForEach(s => s.Attributes.ForEach(a => attributes.Add(a.Tag)));
            attributes = attributes.OrderBy(a => a).ToList();
            for (int i = 1; i < attributes.Count; i++)
            {
                if (attributes[i] - attributes[i - 1] > 1)
                    return false;
            }
            #endregion

            #region Step#2 Formulation test nx(n-1)/2
            int n = _problem.Matrix.Size.X;
            int sum1 = n * (n + 1) / 2;

            int seed = attributes[0];
            int sum2 = seed;
            for (int i = 1; i < attributes.Count; i++)
            {
                if (attributes[i] != seed)
                {
                    seed = attributes[i];
                    sum2 += seed;
                }
            }
            if (sum2 != sum1) return false;
            #endregion

            #region Step#3 Hashing test
            int[] list = new int[n];
            foreach (int item in attributes)
                list[item - 1] = item;
            #endregion

            return true;
        }
    }

}

