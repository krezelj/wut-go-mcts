﻿using System.Diagnostics;
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

            var moves = board.GetMoves();
            if (board.Pass || moves.Length == 0)
                return Move.Pass();

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

            float winProb = (rewards.Max() + n_sims) / (2 * n_sims);
            Console.WriteLine($"Nodes: {_nodes} | kNPS: {(float)_nodes / sw.ElapsedMilliseconds} | {winProb}");
            return moves[rewards.ToList().IndexOf(rewards.Max())];
        }

        private float Simulate(Board board)
        {
            int depth = 0;
            while (!board.Finished)
            {
                _nodes++;
                depth++;
                if (depth >= 1000)
                {
                    board.Display();
                    Console.ReadLine();
                }

                Bitboard allowedPositions = board.Empty;
                Move m = board.GetRandomMove(ref allowedPositions);
                board.ApplyMove(m);
                //var movesMask = board.GetMovesMask();
                //if (movesMask.PopCount() <= 1 || board.Pass)
                //    board.ApplyMove(Move.Pass());
                //else
                //    board.ApplyMove(new Move(movesMask.GetRandomBit()));

            }

            _nodes++;
            return board.Evaluate();
        }
    }
}
