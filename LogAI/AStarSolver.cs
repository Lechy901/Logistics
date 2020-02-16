using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace LogAI {
    class AStarSolver {

        private bool opt;
        private Dictionary<int, State> visited;

        public AStarSolver(bool optimal) {
            opt = optimal;
            visited = new Dictionary<int, State>();
        }
        // classic A* implementation with heap
        public List<int[]> Solve(State start) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int statesSearched = 0;
            StateHeap stateSpace = new StateHeap();
            stateSpace.Insert(start, opt);

            while (stateSpace.Count > 0) {
                State current = stateSpace.ExtractMin();
                int curHash = current.GetHashCode();
                bool previouslyVisited = visited.TryGetValue(curHash, out State previous);
                if (previouslyVisited) {
                    if (current.Equals(previous) && current.PriceSoFar >= previous.PriceSoFar) {
                        continue;
                    }
                }

                visited[curHash] = current;

                if (++statesSearched % 100000 == 0) {
                    Console.WriteLine("Time elapsed: " + stopwatch.Elapsed);
                    Console.WriteLine("States searched: " + statesSearched);
                    Console.WriteLine("Current best heuristic: " + current.Heuristic);
                    Console.WriteLine();
                }

                if (current.IsFinal()) {
                    Console.WriteLine("Search finished!");
                    Console.WriteLine("Time elapsed: " + stopwatch.Elapsed);
                    Console.WriteLine("States searched: " + statesSearched);
                    return GetHistory(current);
                }
                
                foreach (State next in current.GetNextStates(opt)) {
                    stateSpace.Insert(next, opt);
                }
            }

            throw new ArgumentException("There is no solution");
        }
        
        // recalculate history from state parents
        private List<int[]> GetHistory(State s) {
            List<int[]> history = new List<int[]>();

            State cur = s;
            while (cur.Parent != null) {
                history.Add(cur.Action);
                cur = cur.Parent;
            }

            history.Reverse();
            return history;
        }
    }

    // compares states based on f score
    class StateComparer : IComparer<State> {
        public int Compare([AllowNull] State x, [AllowNull] State y) {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return 1;
            if (y == null)
                return -1;
            int xVal = x.Heuristic + x.PriceSoFar;
            int yVal = y.Heuristic + y.PriceSoFar;
            return xVal.CompareTo(yVal);
        }
    }

    // a classic binary leftist heap which automatically deletes worst states when its size reaches a threshold
    class StateHeap {
        private List<State> data;
        private IComparer<State> comparer;
        public int Count {
            get {
                return data.Count - 1;
            }
        }

        public StateHeap() {
            comparer = new StateComparer();
            data = new List<State>();
            data.Add(null);
        }

        public void Insert(State s, bool opt) {
            data.Add(s);
            BubbleUp(data.Count - 1);
            if (!opt)
                CheckAndCut();
        }

        public State ExtractMin() {
            if (Count <= 0) {
                throw new ArgumentException("Cannot extract from an empty heap");
            }
            State r = data[1];
            data[1] = data[data.Count - 1];
            data.RemoveAt(data.Count - 1);
            BubbleDown(1);
            return r;
        }

        private void CheckAndCut() {
            if (data.Count >= 1 << 23) {
                // cca 8 million items = cca 9.3 GB of RAM, cut the second half of the heap (last layer)
                data.RemoveRange(1 << 22, 1 << 22);
            }
        }

        private void BubbleUp(int index) {
            int cur = index;
            while(true) {
                State current = data[cur];
                State parent = data[cur / 2];
                
                if (parent == null)
                    break;
                if (comparer.Compare(parent, current) < 0)
                    break;

                data[cur] = parent;
                data[cur / 2] = current;
                cur /= 2;
            }
        }

        private void BubbleDown(int index) {
            if (Count == 0) {
                return;
            }
            int cur = index;
            while(true) {
                State current = data[cur];
                State left = null;
                State right = null;

                if (cur * 2 < data.Count) {
                    left = data[cur * 2];
                }
                if (cur * 2 + 1 < data.Count) {
                    right = data[cur * 2 + 1];
                }

                bool left_smaller = comparer.Compare(current, left) >= 0;
                bool right_smaller = comparer.Compare(current, right) >= 0;

                if (left_smaller && right_smaller) {
                    if (comparer.Compare(left, right) <= 0) {
                        data[cur * 2] = current;
                        data[cur] = left;
                        cur *= 2;
                        continue;
                    } else {
                        data[cur * 2 + 1] = current;
                        data[cur] = right;
                        cur *= 2;
                        cur += 1;
                        continue;
                    }
                }

                if (left_smaller) {
                    data[cur * 2] = current;
                    data[cur] = left;
                    cur *= 2;
                    continue;
                }

                if (right_smaller) {
                    data[cur * 2 + 1] = current;
                    data[cur] = right;
                    cur *= 2;
                    cur += 1;
                    continue;
                }

                break;
            }
        }
    }

}
