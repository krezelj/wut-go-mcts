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
    public class UCT : Player
    {
        private long _nodes;
        private TreeNode _root = null;

        public override Move Think(Board board)
        {
            int MAX_ITERS = 1_000_000;
            var sw = new Stopwatch();

            _nodes = 0;

            sw.Start();
            _root = new TreeNode(board);
            _root.Expand();

            for (int iter = 0; iter < MAX_ITERS; iter++)
            {
                // select
                TreeNode current = Select();

                // expand?
                if (current.VisitCount == 2 && !current.Terminal)
                {
                    current.Expand();
                    current = current.GetBestChild(UCB);
                    current.VisitCount++;
                }

                // rollout
                float value = Rollout(current.Board);
                if (current.Board.BlackToPlay)
                    value = 1.0f - value;

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
            Console.WriteLine($"Nodes: {_nodes,-9} | MN/S: {Math.Round((float)_nodes / (1000 * sw.ElapsedMilliseconds), 2),-6} | {Math.Round(winProb, 2),-6}");

            // TreeNode tmp = _root;
            //while (tmp.Expanded)
            //{
            //    Console.WriteLine(tmp.ValueSum / tmp.VisitCount);
            //    tmp = tmp.GetBestChild((TreeNode n) => n.ValueSum / n.VisitCount);
            //}
            //Console.WriteLine(moves[bestIdx].Position);

            // _root.Children[bestIdx].Board.Display();
            return moves[bestIdx];
        }

        private TreeNode Select()
        {
            TreeNode current = _root;
            while (current.Expanded)
            {
                _nodes++;
                current.VisitCount++;
                current = current.GetBestChild(UCB);
            }
            current.VisitCount++;
            return current;
        }

        private float Rollout(Board board)
        {
            while (!board.Finished)
            {
                _nodes++;
                Bitboard allowedPositions = board.Empty;
                int allowedCount = allowedPositions.PopCount();
                if (allowedCount <= 20 && RNG.Generator.NextDouble() < 0.1)
                    return board.Evaluate(); // randomly decide that both players passed
                else if (allowedCount <= 10 && RNG.Generator.NextDouble() < 0.8)
                    board.ApplyMove(Move.Pass()); // randomly decide that only the current player passed
                else
                    board.ApplyMove(board.GetRandomMove(ref allowedPositions));
            }

            _nodes++;
            return board.Evaluate();
        }

        private static float UCB(TreeNode node)
        {
            if (node.VisitCount == 0)
                return float.MaxValue;

            float ucb = node.ValueSum / node.VisitCount + 2 * MathF.Sqrt(MathF.Log(node.Parent.VisitCount) / node.VisitCount);
            return ucb;
        }
    }
}
