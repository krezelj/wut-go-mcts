using System.Diagnostics;
using wut_go_mcts.Core;

namespace wut_go_mcts.Players
{
    public class NaiveMCTS : Player
    {
        private Random _rng;
        private long _nodes;

        public NaiveMCTS()
        {
            _rng = new Random(0);
        }

        public override Move Think(Board board)
        {
            _nodes = 0;
            var sw = new Stopwatch();

            if (board.Pass)
                return Move.Pass();

            var moves = board.GetMoves();
            float[] rewards = new float[moves.Length];

            int n_sims = 300;
            sw.Start();
            for (int i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                for (int j = 0; j < n_sims; j++)
                {
                    Board copy = new Board(board);
                    copy.ApplyMove(move);
                    rewards[i] += (board.BlackToPlay ? 1.0f : -1.0f) * Simulate(copy);
                }
            }
            sw.Stop();

            Console.WriteLine($"Nodes: {_nodes} | kNPS: {(float)_nodes/sw.ElapsedMilliseconds} | {(rewards.Max() + n_sims)/(2*n_sims)}");

            return moves[rewards.ToList().IndexOf(rewards.Max())];
        }

        private float Simulate(Board board)
        {
            int depth = 0;
            while (!board.Finished)
            {
                _nodes++;
                depth++;
                if (depth == 1000)
                {
                    board.Display();
                    Console.ReadKey();
                }
                var moves = board.GetMoves();
                if (moves.Length == 0)
                    board.ApplyMove(Move.Pass());
                else
                    board.ApplyMove(moves[_rng.Next(moves.Length)]);

            }

            _nodes++;
            return board.Evaluate();
        }
    }
}
