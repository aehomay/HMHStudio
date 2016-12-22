using HeuristicStudio.Core.Model.DataStructure;
using HeuristicStudio.Core.Model.DataStructure.SCP;
using HeuristicStudio.Core.Model.MetaHeuristic;
using HeuristicStudio.Core.Model.Problems;
using HeuristicStudio.Core.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HeuristicStudio.Core.Model.Heuristic.Constructive
{
    public enum Mode
    {
        Deterministic,
        NonDeterministic
    }
    public enum eState : int
    {
        Distroy = 0,
        Construct,
        Planner,
        Alpha,
        IncDelta,
        DecDelta,
        Beta,
        Council,
        Feasiblity,
        Start,
        Save,
        Recovery,
        Iteration,
        Final
    }

    //Based on the first improvment strategy
    public class DestructiveConstructive : IMetaHeuristic<SCPSolution>
    {
        Mode _mode = Mode.Deterministic;
        eState _state = eState.Start;
        SCP _problem = null;
        SCPSolution _solution = null;
        List<SCPSolution> _collections = new List<SCPSolution>();
        Stopwatch _sp = new Stopwatch();
        List<Tuple<int, double, int>> _losted = new List<Tuple<int, double, int>>();
        List<SCPSet> _cadidate = new List<SCPSet>();
        List<SCPSet> _recovery = new List<SCPSet>();
        List<Tuple<double, double, List<SCPSet>>> _alpha_recovery = new List<Tuple<double, double, List<SCPSet>>>();
        double _ratio = 10;
        double _delta = 10;
        double _epsilon = 0.9;
        double _maxprice = 0.0;
        double _saved = 0.0;
        double _maxAlpha, _minAlpha;
        int _iteration = 0;
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
                _collections = _collections.OrderBy(s => s.Cost).ToList();
                return _collections.First();
            }
        }

        public IProblem Problem
        {
            get
            {
                return _problem;
            }
        }

        public double Execute(IProblem problem,Mode mode)
        {
            _mode = mode;
            return Execute(problem);
        }

        public double Execute(IProblem problem)
        {

            _sp.Restart();
            Initializer(problem);

            double gain = 0;
            double budget = 0;
            double alpha = 0.0;
            double estimation = 0.0;

            while (_state != eState.Final)
            {

                switch (_state)
                {
                    case eState.Recovery://-----------------------------------Recovery State
                        _recovery = RemoveRedundantSet(_recovery);
                        _recovery.ForEach(s => _solution.Sets.Add(s));
                        _recovery.Clear();
                        _state = eState.DecDelta;
                        break;

                    case eState.Distroy://------------------------------------Distruction State
                        if (_mode == Mode.NonDeterministic)
                            gain = Destructor(_solution, alpha);
                        else if (_mode == Mode.Deterministic)
                            gain = Destructor(_solution);
                        if (gain > 0)
                        {
                            budget += gain;
                            _state = eState.Save;
                        }
                        else if (gain == 0)
                            _state = eState.Council;
                        break;

                    case eState.Construct://----------------------------------Construction State
                        Constructor(new List<SCPSet>(_cadidate));
                        estimation = 0;
                        if (_losted.Count == 0)
                            _state = eState.Feasiblity;
                        else
                            _state = eState.Distroy;
                        break;

                    case eState.Planner://-------------------------------------Planner
                        estimation = double.MaxValue;
                        _cadidate = Order(budget);
                        if (_cadidate.Count > 0)
                            estimation = 0.0;
                        _cadidate.ForEach(s => estimation += s.Cost);
                        _state = eState.Council;
                        break;

                    case eState.Alpha://---------------------------------------Alpha State
                        alpha = alpha - (alpha * (_ratio / 100));
                        _state = eState.Distroy;
                        break;

                    case eState.DecDelta://---------------------------------------Decerese Delta State
                        _delta *= _epsilon;
                        if (_delta > _epsilon)
                            _state = eState.Distroy;
                        else
                            _state = eState.Final;
                        break;

                    case eState.IncDelta://---------------------------------------Increse Delta State
                        _delta /= _epsilon;
                        _state = eState.Council;
                        break;

                    case eState.Beta://----------------------------------------Beta State
                        double rquest = (estimation - budget);
                        budget += (rquest > _saved) ? _saved : rquest;
                        _saved = Math.Max((_saved - rquest), 0);
                        _state = eState.DecDelta;
                        break;

                    case eState.Council://-------------------------------------Council State
                        if (estimation < budget)
                        {
                            _saved += budget - estimation;
                            budget = 0;
                            _state = eState.Construct;
                            break;

                        }
                        else if (_saved > budget)
                        {
                            if (_saved > 0)
                                _state = eState.Beta;
                            else
                                _state = eState.Recovery;
                            break;
                        }
                        else if (_saved == _maxprice)
                        {
                            _state = eState.Recovery;
                            break;
                        }
                        _state = eState.Distroy;
                        break;
                       
                    case eState.Feasiblity://--------------------------------------Feasiblity State
                        _solution = RemoveRedundantSet(_solution);
                        bool feasible = IsFeasible(_solution);
                        if (feasible)
                        {
                            _collections.Add(_solution.Clone());
                            _state = eState.Iteration;
                        }
                        else
                        _state = eState.Council;
                        break;

                    case eState.Start://-------------------------------------------Start State
                        _losted.Clear();
                        _recovery.Clear();
                        _solution = _problem.Solution.Clone();
                        _solution.ComputeAttributeRedundancies();
                        _maxprice = _solution.Cost;
                        if (_mode == Mode.NonDeterministic)
                        {
                            _state = eState.Alpha;
                            Fitness(_solution);
                            alpha = _maxAlpha;
                        }
                        else if (_mode == Mode.Deterministic)
                            _state = eState.Distroy;
                        budget = 0;
                        estimation = 0.0;
                        break;

                    case eState.Iteration://---------------------------------------Iteration State
                        _losted.Clear();
                        _recovery.Clear();
                        _maxprice = _solution.Cost;
                        if (_mode == Mode.NonDeterministic)
                        {
                            _state = eState.Alpha;
                            Fitness(_solution);
                            alpha = _maxAlpha;
                        }
                        else if (_mode == Mode.Deterministic)
                            _state = eState.Distroy;

                        _delta /= _epsilon;
                        _ratio /= _epsilon;
                        budget = 0;
                        estimation = 0.0;
                        _iteration++;
                        break;

                    case eState.Save://---------------------------------------------Save State
                        budget += _saved;
                        _saved += budget * (_delta / 100);
                        budget -= _saved;
                        _state = eState.Planner;
                        break;

                    case eState.Final:
                        _sp.Stop();
                        break;

                    default:
                        break;
                }
            }

            return OptimumSultion.Cost;
        }

        private double Destructor(SCPSolution solution, double alpha)
        {
            double budget = 0;
            List<SCPSet> blacklist = new List<SCPSet>();
            // _crSolution.Sets = _crSolution.Sets.OrderByDescending(s=>s.Cost).ThenByDescending(s => s.Overhead).ToList();

            solution.Sets.ForEach(set =>
            {
                if (set.Overhead >= alpha)
                    blacklist.Add(set);
            });

            blacklist.ForEach(bSet =>
            {
                SCPSet remove = bSet;// attribute.UsedIn.OrderByDescending(i => i.Cost).ThenByDescending(i => i.Overhead).First();
                int cla = 0;//Counter of losted attributes from the set that is going to be removed, I will divide the cost of set by the attributes that are going to be replace
                cla = remove.Attributes.Where(a => a.Redundancy - 1 <= 0).Count();

                remove.Attributes.ForEach(attribute =>
                {
                    if (attribute.Redundancy - 1 <= 0)
                    {
                        Tuple<int, double, int> lost = new Tuple<int, double, int>(remove.Tag, remove.Cost / cla, attribute.Tag);
                        _losted.Add(lost);
                    }
                });

                budget += remove.Cost;
                solution.Sets.Remove(solution.Sets.Find(s => s.Tag == remove.Tag));
                if (_recovery.Exists(r => r.Tag == remove.Tag) == false)
                    _recovery.Add(remove);
            });

            return budget;
        }

        private double Destructor(SCPSolution solution)
        {
            if (solution.Sets.Count() == 0) return 0;

            double budget = 0;
            solution.Sets = solution.Sets.OrderByDescending(s => s.Cost).ToList();
            SCPSet remove = solution.Sets.First();

            remove.Attributes.ForEach(attribute =>
            {
                if (attribute.Redundancy - 1 <= 0)
                {
                    Tuple<int, double, int> lost = new Tuple<int, double, int>(remove.Tag, remove.Cost, attribute.Tag);
                    if (_losted.Exists(l => l.Item3 == attribute.Tag) == false)
                        _losted.Add(lost);
                }
                else
                    attribute.Redundancy--;
            });

            budget += remove.Cost;
            solution.Sets.Remove(remove);
            if (_recovery.Exists(r => r.Tag == remove.Tag) == false)
                _recovery.Add(remove);

            //if (solution.Sets.Where(s => s.Visit == false).Count() <= ) return -1;
            // SCPSet set = solution.Sets.Where(s=>s.Visit == false).First();
            //set.Visit = true;
            //blacklist.Add(set);
            //blacklist.ForEach(bSet =>
            //{
            //    SCPSet remove = bSet;// attribute.UsedIn.OrderByDescending(i => i.Cost).ThenByDescending(i => i.Overhead).First();
            //    int cla = 0;//Counter of losted attributes from the set that is going to be removed, I will divide the cost of set by the attributes that are going to be replace
            //    cla = remove.Attributes.Where(a => a.Redundancy - 1 <= 0).Count();

            //    remove.Attributes.ForEach(attribute =>
            //    {
            //        if (attribute.Redundancy - 1 <= 0)
            //        {
            //            Tuple<int, double, int> lost = new Tuple<int, double, int>(remove.Tag, remove.Cost / cla, attribute.Tag);
            //            if (_losted.Exists(l => l.Item3 == attribute.Tag) == false)
            //                _losted.Add(lost);
            //        }
            //    });

            //    budget += remove.Cost;
            //    solution.Sets.Remove(solution.Sets.Find(s => s.Tag == remove.Tag));
            //    if (_recovery.Exists(r => r.Tag == remove.Tag) == false)
            //        _recovery.Add(remove);
            //});

            return budget;
        }

        private void Constructor(List<SCPSet> selected)
        {
            if (selected != null)
            {
                selected.ForEach(set =>
                {
                    if (_solution.Sets.Exists(s => s.Tag == set.Tag) == false)
                    {
                        _solution.Sets.Add(set);
                        set.Attributes.ForEach(a => _losted.Remove(_losted.Find(l => l.Item3 == a.Tag)));
                    }

                });
            }
            _solution.ComputeAttributeRedundancies();
        }

        private List<SCPSet> Order(double budget)
        {
            List<SCPSet> candidate = new List<SCPSet>();
            List<HashSet<int>> subsets = new List<HashSet<int>>();
            List<Tuple<double,int, HashSet<int>>> panel = new List<Tuple<double,int, HashSet<int>>>();

            subsets = SubsetMatrix();
            subsets = subsets.OrderByDescending(s => s.Count).ToList();

            foreach (var set in subsets)
            {
                List<SCPSet> neighbors = _problem.Source.GetNeighbors(set.ToArray());
                if (neighbors.Count > 0)
                {
                     SCPSet neighbor = (SCPSet)neighbors.OrderBy(n => n.Cost).First().Clone();
                    panel.Add(new Tuple<double, int, HashSet<int>>(neighbor.Cost, neighbor.Tag, set));
                    
                }
            }

            panel = panel.OrderBy(p => p.Item1 / p.Item3.Count).ToList();
            foreach (var p in panel)
            {
                if (p.Item1 < budget)
                {
                    candidate.Add(_problem.Source.Sets.Find(s => s.Tag == p.Item2));
                    budget -= p.Item1;
                }
            }
            return candidate;
        }
      
        private List<Tuple<double, int, HashSet<int>, int>> PanelOptimisation(List<Tuple<double, int, HashSet<int>, int>> panel_b)
        {
            Dictionary<int,int> frequencies = new Dictionary<int, int>();
            panel_b.ForEach(p => p.Item3.ToList().ForEach(a =>
            {
                if (frequencies.ContainsKey(a) == false)
                    frequencies.Add(a, 1);
                else
                    frequencies[a]++;
            }));

            panel_b = panel_b.OrderByDescending(p => p.Item1).ToList();

            List<int> blacklist = new List<int>();
            panel_b.ForEach(p => 
            {
                bool useless = true;
               foreach(var a in p.Item3)
                {
                    if (frequencies[a] - 1 == 0)
                    {
                        useless = false;
                        break;
                    }
                    else
                        frequencies[a]--;
                   
                }
                if (useless)
                    blacklist.Add(p.Item2);
            });

            blacklist.ForEach(b => panel_b.Remove(panel_b.Find(i => i.Item2 == b)));
            return panel_b;
        }

        private double BudgetPlanning()
        {
            List<SCPSet> sets = new List<SCPSet>();
            List<SCPAttribute> attributes = new List<SCPAttribute>();
            List<HashSet<int>> subsets = new List<HashSet<int>>();

            subsets = SubsetMatrix();

            foreach (var set in subsets)
            {
                List<SCPSet> neighbors = _problem.Source.GetNeighbors(set.ToArray());
                if (neighbors.Count > 0)
                {
                    neighbors.ForEach(s =>
                    {
                        sets.Add(s);
                    });
                }
            }

            FindCardinalityInSet(sets);


             sets.ForEach(s => s.Attributes.ForEach(a1 =>
             {
                 if (_losted.Exists(l => l.Item3 == a1.Tag))
                 {
                     if(attributes.Exists(a2=>a1.Tag == a2.Tag) == false)
                         attributes.Add(a1);
                 }
             }));

            SCPDataSet source = new SCPDataSet() { Sets = sets, Attributes = attributes };
            source.Resetset();
            SCP subproblem = new SCP(new MatrixSize() { X = attributes.Count, Y = sets.Count }) { Source = source };

            IConstructiveHeuristic fog = new SCPGRASP(0.9, 1e-9);
            double cost = fog.Execute(subproblem);
            _cadidate = subproblem.Solution.Sets;
            return cost;
        }

        private List<SCPSet> FindCardinalityInSet(List<SCPSet> candidate)
        {
            candidate.ForEach(s => s.Attributes.ForEach(a => a.UsedIn.Clear()));
            foreach (var s1 in candidate)
            {
                s1.Attributes.ForEach(a1 =>
                {
                    foreach (var s2 in candidate)
                    {
                        s2.Attributes.ForEach(a2 =>
                        {

                            if (a1.Tag == a2.Tag)
                            {
                                if (a1.UsedIn.Exists(s => s.Tag == s2.Tag) == false)
                                    a1.UsedIn.Add(s2);
                            }
                        });
                    }
                });
            }
            return candidate;
        }

        private List<SCPSet> FindCardinalityInUnion(List<SCPSet> candidate)
        {
            List<SCPSet> union = new List<SCPSet>(_solution.Sets);
            union.AddRange(candidate);

            candidate.ForEach(s => s.Attributes.ForEach(a => a.UsedIn.Clear()));
            foreach (var s1 in candidate)
            {
                s1.Attributes.ForEach(a1 =>
                {
                    foreach (var s2 in union)
                    {
                        s2.Attributes.ForEach(a2 =>
                        {

                            if (a1.Tag == a2.Tag)
                            {
                                if (a1.UsedIn.Exists(s => s.Tag == s2.Tag) == false)
                                    a1.UsedIn.Add(s2);
                            }
                        });
                    }
                });
            }
            return candidate;
        }

        private List<SCPSet> FindCardinalityInSolution(List<SCPSet> candidate,SCPSolution solution)
        {
            candidate.ForEach(s => s.Attributes.ForEach(a => a.UsedIn.Clear()));
            foreach (var s1 in candidate)
            {
                s1.Attributes.ForEach(a1 =>
                {
                    foreach (var s2 in solution.Sets)
                    {
                        s2.Attributes.ForEach(a2 =>
                        {

                            if (a1.Tag == a2.Tag)
                            {
                                if (a1.UsedIn.Exists(s => s.Tag == s2.Tag) == false)
                                    a1.UsedIn.Add(s2);
                            }
                        });
                    }
                });
            }
            return candidate;
        }

        private List<int> SelectBestCombination(List<Tuple<double, int, HashSet<int>>> panel)
        {
            Tuple<double, HashSet<int>> best = null;

            List<Tuple<double, HashSet<int>>> tablu = new List<Tuple<double, HashSet<int>>>();
            int index = 0;
            int length = panel.Count;
            while (index < length)
            {
                List<int> covered = new List<int>();
                Tuple<double, HashSet<int>> t;
                List<Tuple<double, int, HashSet<int>>> subset = new List<Tuple<double, int, HashSet<int>>>();
                for (int i = 0; i < length; i++)
                {
                    if (i == index) continue;
                    subset.Add(panel[i]);
                }
                t = new Tuple<double, HashSet<int>>(panel[index].Item1, new HashSet<int>() { panel[index].Item2 });
                panel[index].Item3.ToList().ForEach(a =>
                {
                    if (covered.Contains(a) == false)
                        covered.Add(a);
                });
                if (covered.Count == _losted.Count)
                {
                    tablu.Add(t);
                    index++;
                    continue;
                }
                for (int i = 0; i < length - 1; i++)
                {
                    for (int j = i; j < length - 1; j++)
                    {
                        bool add = false;
                        subset[j].Item3.ToList().ForEach(a =>
                        {
                            if (covered.Contains(a) == false)
                            {
                                add = true;
                                covered.Add(a);
                            }
                        });

                        if (add)
                        {
                            if (t.Item2.Contains(subset[j].Item2) == false)
                            {
                                t.Item2.Add(subset[j].Item2);
                                t = new Tuple<double, HashSet<int>>(t.Item1 + subset[j].Item1, t.Item2);
                            }
                        }

                        if (covered.Count == _losted.Count)
                            break;
                    }//end for

                    if (covered.Count == _losted.Count)
                    {
                        bool add = true;
                        foreach (var item in tablu)
                            if (t.Item2.IsSubsetOf(item.Item2))
                                add = false;

                        if (add)
                            tablu.Add(t);
                    }
                    covered = new List<int>();
                    t = new Tuple<double, HashSet<int>>(panel[index].Item1, new HashSet<int>() { panel[index].Item2 });
                    panel[index].Item3.ToList().ForEach(a =>
                    {
                        if (covered.Contains(a) == false)
                            covered.Add(a);
                    });

                }//end for

                index++;
            }//end while

            Tuple<double, HashSet<int>> temp = new Tuple<double, HashSet<int>>(0, null);
            panel.ForEach(p =>
            {
                temp = new Tuple<double, HashSet<int>>(temp.Item1 + p.Item1, new HashSet<int>() { p.Item2 });
            });
            tablu.Add(temp);

            //List<double> gold = new List<double>();
            //tablu.ForEach(t =>
            //{
            //    gold.Add(Delta(t.Item2.ToList(),_solution));
            //});


            if (tablu.Count > 0)
                best = tablu.OrderBy(t => t.Item1).First();
            else
                best = tablu.FirstOrDefault();
            return best.Item2.ToList();
        }

        public void Fitness(SCPSolution solution)
        {
            FindCardinalityInSolution(solution.Sets,_solution);
            List<Tuple<int, double>> alphas = new List<Tuple<int, double>>();
            
            foreach (var set in solution.Sets)
            {
                double overhead = 0.0;

                foreach (var attribute in set.Attributes)
                {
                    attribute.Cost = 0.0;
                    attribute.UsedIn.ForEach(s =>
                    {
                        attribute.Cost += s.Cost / s.Attributes.Count;
                        if (s.Tag != set.Tag)
                            overhead += s.Cost / s.Attributes.Count;
                    });
                }

                set.Overhead = set.Cost + overhead;
                alphas.Add(new Tuple<int, double>(set.Tag, overhead));
            }

            solution.Sets = solution.Sets.OrderBy(s => s.Overhead).ToList();

            _maxAlpha = alphas.Max(i => i.Item2);
            _minAlpha = alphas.Min(i => i.Item2);
        }

        public void Fitness(List<SCPSet> sets)
        {
            List<Tuple<int, double>> alphas = new List<Tuple<int, double>>();

            foreach (var set in sets)
            {
                double overhead = 0.0;

                foreach (var attribute in set.Attributes)
                {
                    attribute.Cost = 0.0;
                    attribute.UsedIn.ForEach(s =>
                    {
                        attribute.Cost += s.Cost / s.Attributes.Count;
                        if (s.Tag != set.Tag)
                            overhead += s.Cost / s.Attributes.Count;
                    });
                }
                set.Overhead = set.Cost + overhead;
                alphas.Add(new Tuple<int, double>(set.Tag, overhead));
            }

            sets = sets.OrderBy(s => s.Overhead).ToList();


            double cMin = sets.Min(s => s.Cost);
            double cMax = sets.Max(s => s.Cost);
            double oMin = sets.Min(s => s.Overhead);
            double oMax = sets.Max(s => s.Overhead);
            _maxAlpha = alphas.Max(i => i.Item2);
            _minAlpha = alphas.Min(i => i.Item2);
        }

        List<HashSet<int>> SubsetMatrix()
        {
            int[] original = _losted.Select(i => i.Item3).ToArray();
            List<HashSet<int>> subsets = new List<HashSet<int>>();
            /*
             * Add each losted attribute as an indvitual set to be used in the evaluation phase
             * it is important to know what is the minimum price to have this attribute
            */
            original.ToList().ForEach(o => subsets.Add(new HashSet<int>() { o }));

            int[][] bMatrix = new int[original.Length][];

            for (int i = 0; i < original.Length; i++)
                bMatrix[i] = _problem.Matrix.Read(original[i] - 1);

            for (int i = 0; i < bMatrix.Length; i++)
            {
                //Size says if a set with size = 1 added as a subset then the next set in the same matrix row should be bigger than the last size
                int size = 0;

                for (int j = 0; j < bMatrix[i].Length; j++)
                {
                    HashSet<int> list = new HashSet<int>();
                    if (bMatrix[i][j] == 1)
                    {

                        for (int k = 0; k < original.Length; k++)
                        {
                            if (j < bMatrix[i].Length)
                            {
                                if (bMatrix[k][j] == 1)
                                    list.Add(original[k]);
                            }
                        }
                    }

                    if (list.Count > size)//This condition says if the set is a combination of different attributes make sure it is not subset of any added set
                    {
                        bool add = true;
                        foreach (var set in subsets)
                        {
                            if (list.IsSubsetOf(set))
                            {
                                add = false;//The extracted set is a sub set of another set which already exist, forget adding this set go to the next set
                                break;
                            }
                        }
                        if (add)
                        {
                            subsets.Add(list);
                            size++;
                        }
                    }
                }
            }
            return subsets;
        }

        public void Subsets<T>(HashSet<T> set)
        {
            List<HashSet<T>> allsubsets = new List<HashSet<T>>();

            int length = set.Count;
            int index = 0;


            while (index < length)
            {
                allsubsets.Add(new HashSet<T>() { set.ElementAt(index) });

                HashSet<T> newsubset = new HashSet<T>();
                for (int k = 0; k < length; k++)
                {
                    if (k == index) continue;
                    newsubset.Add(set.ElementAt(k));
                }

                for (int i = 0; i < length - 1; i++)
                {
                    HashSet<T> subset = new HashSet<T>();
                    for (int j = i; j < length - 1; j++)
                    {
                        subset.Add(newsubset.ElementAt(j));
                    }
                    if (subset.Count > 0)
                    {
                        bool add = true;
                        foreach (var s in allsubsets)
                        {
                            if (newsubset.IsSubsetOf(s))
                            { add = false; break; }
                        }
                        if (add)
                            allsubsets.Add(subset);
                    }
                }
                index++;
            }
            allsubsets.Add(set);
        }

        private SCPSolution RemoveRedundantSet(SCPSolution solution)
        {
            SCPSolution improved = solution.Clone();
            List<int> blacklist = new List<int>();

            solution.ComputeAttributeRedundancies();
            solution.Sets = solution.Sets.OrderByDescending(s => s.Cost).ToList();

            foreach (var set in solution.Sets)
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
            if (improved.Cost < solution.Cost)
                solution = improved;
            return solution;
        }

        private List<SCPSet> RemoveRedundantSet(List<SCPSet> candidate)
        {
            FindCardinalityInUnion(candidate);
            List<int> blacklist = new List<int>();
            List<int> frequency = new List<int>();

            candidate.ForEach(s =>
            {
                s.Attributes.ForEach(a =>
            {
                if (_losted.Exists(l => l.Item3 == a.Tag))
                    frequency.Add(a.Tag);
            });
            });

            candidate = candidate.OrderByDescending(s => s.Cost).ToList();

            foreach (var set in candidate)
            {
                set.Attributes.ForEach(a => a.Redundancy = frequency.Where(f => f == a.Tag).Count());
                bool useless = true;

                foreach (var a in set.Attributes)
                {
                    if (a.Redundancy == 1)
                    {
                        useless = false;
                        break;
                    }
                }
                if (useless)
                {
                    set.Attributes.ForEach(a =>
                    {
                        if (frequency.Contains(a.Tag))
                            frequency.Remove(frequency.Where(f => f == a.Tag).First());
                    });
                    blacklist.Add(set.Tag);
                }
            }

            blacklist.ForEach(b => candidate.Remove(candidate.Find(s => s.Tag.ToString() == b.ToString())));
            return candidate;
        }

        private void Initializer(IProblem problem)
        {
            _problem = (SCP)problem;
            _problem.MarkPrimeSets();
            _collections.Add(_problem.Solution.Clone());
        }

        private bool IsFeasible(SCPSolution solution)
        {
            HashSet<int> problem_att = new HashSet<int>();
            _problem.Source.Attributes.ForEach(a => problem_att.Add(a.Tag));

            HashSet<int> solution_att = new HashSet<int>();
            solution.USet.ForEach(a => solution_att.Add(a));

            return (problem_att.IsSubsetOf(solution_att));
        }

        public double Delta(SCPSet set, SCPSolution solution)
        {
            double gain = 0;
            double price = 0;

            Dictionary<int, int> suFreq = solution.SUFrequency;
            int[,] catalog = solution.Catalog;
            List<int> targets = new List<int>();

            price += set.Cost;
            foreach (var attribute in set.Attributes)
            {
                for (int i = 0; i < catalog.Length / 2; i++)
                {
                    if (catalog[1, i] == attribute.Tag)
                        targets.Add(catalog[0, i]);
                }
            }

            foreach (var t in targets)
            {
                suFreq[t]--;
            }

            foreach (var item in suFreq)
            {
                if (item.Value <= 0)
                    gain += solution.Sets.Find(s => s.Tag == item.Key).Cost;
            }

            return set.Cost - gain;
        }

        public double Delta(List<SCPSet> sets, SCPSolution solution)
        {
            double gain = 0;
            double price = 0;
            Dictionary<int, int> suFreq = solution.SUFrequency;
            int[,] catalog = solution.Catalog;
            List<int> targets = new List<int>();

            foreach (var set in sets)
            {
                price += set.Cost;
                foreach (var attribute in set.Attributes)
                {
                    for (int i = 0; i < catalog.Length / 2; i++)
                    {
                        if (catalog[1, i] == attribute.Tag)
                            targets.Add(catalog[0, i]);
                    }
                }
            }

            foreach (var t in targets)
                suFreq[t]--;

            foreach (var item in suFreq)
            {
                if (item.Value <= 0)
                    gain += solution.Sets.Find(s => s.Tag == item.Key).Cost;
            }

            return  Math.Max((gain- price),0);
        }

        public double Delta(List<int> sets, SCPSolution solution)
        {
            List<SCPSet> list = new List<DataStructure.SCPSet>();
            sets.ForEach(s1 => list.Add(_problem.Source.Sets.Find(s2 => s1 == s2.Tag)));
            return Delta(list, solution);
        }
    }

}

