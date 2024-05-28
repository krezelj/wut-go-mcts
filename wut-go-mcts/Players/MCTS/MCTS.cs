
using System.Xml.Linq;
using wut_go_mcts.Core;

namespace wut_go_mcts.Players.MCTS
{
    public abstract class MCTS : Player
    {
        protected long _nodes;
        protected TreeNode _root;

        protected void InitThink(Board board)
        {
            _nodes = 0;
            _root = new TreeNode(board);
            _root.Expand();
        }

        protected TreeNode Select()
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

        protected float Rollout(Board board)
        {
            bool blackToPlay = board.BlackToPlay;
            while (!board.Finished)
            {
                _nodes++;
                Bitboard allowedPositions = board.Empty;
                int allowedCount = allowedPositions.PopCount();
                if (allowedCount <= 20 && RNG.Generator.NextDouble() < 0.1)
                {
                    // return board.Evaluate(); // randomly decide that both players passed
                    _nodes--;
                    break;
                }
                else if (allowedCount <= 10 && RNG.Generator.NextDouble() < 0.8)
                    board.ApplyMove(Move.Pass()); // randomly decide that only the current player passed
                else
                    board.ApplyMove(board.GetRandomMove(ref allowedPositions));
            }

            _nodes++;
            float value = board.Evaluate();
            if (blackToPlay)
                value = 1.0f - value;
            return value;
        }

        protected static float UCB(TreeNode node)
        {
            if (node.VisitCount == 0)
                return float.MaxValue;

            float ucb = node.ValueSum / node.VisitCount + 2 * MathF.Sqrt(MathF.Log(node.Parent.VisitCount) / node.VisitCount);
            return ucb;
        }
    }
}
