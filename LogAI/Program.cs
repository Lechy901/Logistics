using System;

namespace LogAI {
    class Program {
        static void Main(string[] args) {
            if (args.Length != 3) {
                Console.WriteLine("Need 3 arguments: input_file, output_file and a true|false value whether the program should try to find the optimal solution (on small problems)");
                return;
            }

            bool optimal = bool.Parse(args[2]);

            InputLoader loader = new InputLoader();
            State startState = loader.LoadInput(args[0], optimal);

            AStarSolver solver = new AStarSolver(optimal);
            var solved = solver.Solve(startState);

            OutputWriter ow = new OutputWriter();
            ow.WriteOutput(args[1], solved);
        }
    }
}
