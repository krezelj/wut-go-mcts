using System.Diagnostics;
using wut_go_mcts.Core;

namespace wut_go_mcts.Players.MCTS
{
    public class NaiveMCTS : MCTS
    {
        public override Move Think(Board board)
        {
            int MAX_ITERS = 100_000;
            var sw = new Stopwatch();
            sw.Start();

            InitThink(board);
            int childIdx = 0;
            for (int iter = 0; iter < MAX_ITERS; iter++)
            {
                childIdx %= _root.Children.Length;
                TreeNode current = _root.Children[childIdx++];
                current.VisitCount++;

                float value = Rollout(current.Board);
                current.ValueSum += value;

            }
            sw.Stop();

            // return most visited/best value
            // TODO optimise this
            Move[] moves = board.GetMoves();
            int bestIdx = _root.GetBestChildIndex((TreeNode n) => n.ValueSum / n.VisitCount);
            sw.Stop();

            float winProb = _root.Children[bestIdx].ValueSum / _root.Children[bestIdx].VisitCount;
            Console.WriteLine($"Nodes: {_nodes,-9} | MN/S: {Math.Round((float)_nodes / (1000 * sw.ElapsedMilliseconds), 2),-6} | {Math.Round(winProb, 2),-6}");

            return moves[bestIdx];
        }
    }
}
