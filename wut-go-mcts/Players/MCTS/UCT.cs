using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using wut_go_mcts.Core;

namespace wut_go_mcts.Players.MCTS
{
    public class UCT : MCTS
    {
        public override Move Think(Board board, Timer timer)
        {
            float timeLimit = timer.MilisecondsRemaining / 30.0f;

            int MAX_ITERS = 100_000;
            var sw = new Stopwatch();
            sw.Start();

            InitThink(board);
            for (int iter = 0; iter < MAX_ITERS; iter++)
            {
                if (timer.MilisecondsElapsedThisTurn >= timeLimit)
                    break;

                // select
                TreeNode current = Select();

                // expand?
                if (current.VisitCount == 2 && !current.Terminal)
                {
                    // current.Expand();
                    current = current.GetBestChild(UCB);
                    current.VisitCount++;
                }

                // rollout
                float value = Rollout(current.Board);

                // backtrack
                while (current != null)
                {
                    current.ValueSum += value;
                    value = 1.0f - value;
                    current = current.Parent;
                }
            }

            // return most visited/best value
            // TODO optimise this
            Move[] moves = board.GetMoves();
            int bestIdx = _root.GetBestChildIndex((TreeNode n) => n.ValueSum / n.VisitCount);
            sw.Stop();

            float winProb = _root.Children[bestIdx].ValueSum / _root.Children[bestIdx].VisitCount;

            Console.WriteLine($"Nodes: {_nodes,-9} | MN/S: {Math.Round((float)_nodes / (1000 * sw.ElapsedMilliseconds), 2),-6} | {Math.Round(winProb, 2),-6} | {timer.MilisecondsElapsedThisTurn, -4}");

            // TreeNode tmp = _root;
            //while (tmp.Expanded)
            //{
            //    Console.WriteLine(tmp.ValueSum / tmp.VisitCount);
            //    tmp = tmp.GetBestChild((TreeNode n) => n.ValueSum / n.VisitCount);
            //}
            //Console.WriteLine(moves[bestIdx].Position);

            // _root.Children[bestIdx].Board.Display();
            if (winProb < 0.01)
                return Move.Pass(); // resign
            return moves[bestIdx];
        }
    }
}
