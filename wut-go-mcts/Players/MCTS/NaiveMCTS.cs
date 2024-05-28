﻿using System.Diagnostics;
using wut_go_mcts.Core;

namespace wut_go_mcts.Players.MCTS
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
            // throw new NotImplementedException(); // change evaluate handling to 1 - 0

            _nodes = 0;
            var sw = new Stopwatch();

            var moves = board.GetMoves();
            //if (board.Pass || moves.Length == 0)
            //    return Move.Pass();

            float[] rewards = new float[moves.Length];

            int n_sims = 125;
            sw.Start();
            for (int i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                for (int j = 0; j < n_sims; j++)
                {
                    Board copy = new Board(board);
                    copy.ApplyMove(move);
                    float value = Simulate(copy);
                    if (!board.BlackToPlay)
                        value = 1.0f - value;
                    rewards[i] += value;
                }
            }
            sw.Stop();

            float winProb = rewards.Max() / n_sims;
            Console.WriteLine($"Nodes: {_nodes,-9} | MN/S: {Math.Round((float)_nodes / (1000 * sw.ElapsedMilliseconds), 2),-6} | {Math.Round(winProb, 2),-6}");
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
                int allowedCount = allowedPositions.PopCount();
                if (allowedCount <= 20 && _rng.NextDouble() < 0.1)
                    return board.Evaluate(); // randomly decide that both players passed
                else if (allowedCount <= 10 && _rng.NextDouble() < 0.8)
                    board.ApplyMove(Move.Pass()); // randomly decide that only the current player passed
                else
                    board.ApplyMove(board.GetRandomMove(ref allowedPositions));
            }

            _nodes++;
            return board.Evaluate();
        }
    }
}
