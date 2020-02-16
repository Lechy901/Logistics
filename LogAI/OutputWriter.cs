using System;
using System.Collections.Generic;
using System.Text;

namespace LogAI {
    class OutputWriter {
        public void WriteOutput(string filepath, List<int[]> history) {
            string[] actions = new string[] { "drive", "load", "unload", "fly", "pickup", "dropoff" };

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath)) {
                for (int i = 0; i < history.Count; i++) {
                    file.WriteLine(actions[history[i][0]] + " " + history[i][1] + " " + history[i][2]);
                }
            }
            
        }
    }
}
