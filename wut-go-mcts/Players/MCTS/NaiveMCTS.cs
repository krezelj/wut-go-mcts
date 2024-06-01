using System.Diagnostics;
using wut_go_mcts.Core;

namespace wut_go_mcts.Players.MCTS
{
    public class NaiveMCTS : MCTS
    {
        public NaiveMCTS(Func<TreeNode, float> estimator) : base(estimator) { }

        public override Move Think(Board board, Timer timer)
        {
            float timeLimit = timer.MilisecondsRemaining / 60.0f;

            int MAX_ITERS = 1_000_000;
            var sw = new Stopwatch();
            sw.Start();

            InitThink(board);
            int childIdx = 0;
            for (int iter = 0; iter < MAX_ITERS; iter++)
            {
                if (timer.MilisecondsElapsedThisTurn >= timeLimit)
                    break;

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
            int bestIdx = _root.GetBestChildIndex((TreeNode n) => {
                if (n.Board.Pass && !n.Board.Finished)
                    return float.MinValue;
                return n.ValueSum / n.VisitCount;
            });
            sw.Stop();

            float winProb = _root.Children[bestIdx].ValueSum / _root.Children[bestIdx].VisitCount;
            //Console.WriteLine($"Nodes: {_nodes,-9} | MN/S: {Math.Round((float)_nodes / (1000 * sw.ElapsedMilliseconds), 2),-6} | {Math.Round(winProb, 2),-6} | {timer.MilisecondsElapsedThisTurn,-4}");

            if (winProb < 0.01)
                return Move.Pass(); // resign
            return moves[bestIdx];
        }
    }
}
