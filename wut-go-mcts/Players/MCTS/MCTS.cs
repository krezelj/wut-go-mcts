
using System.Xml.Linq;
using wut_go_mcts.Core;

namespace wut_go_mcts.Players.MCTS
{
    public abstract class MCTS : Player
    {
        protected long _nodes;
        protected TreeNode _root;
        protected Func<TreeNode, float> _estimator;

        protected void InitThink(Board board, bool keepRoot = false)
        {
            _nodes = 0;
            if (!keepRoot || _root == null)
                _root = new TreeNode(board);

            bool newRootFound = false;
            if (keepRoot && _root.Children != null)
            {
                // find the child of current root that matches the current state of the board
                foreach (var child in _root.Children)
                {
                    if (child != null && child.Board == board)
                    {
                        _root = child;
                        newRootFound = true;
                        break;
                    }
                }
            }
            if (keepRoot && !newRootFound)
                _root = new TreeNode(board);

            _root.Expand();
        }

        public MCTS(Func<TreeNode, float> estimator)
        {
            _estimator = estimator;
        }

        protected TreeNode Select()
        {
            TreeNode current = _root;
            while (current.Expanded)
            {
                _nodes++;
                current.VisitCount++;
                current = current.GetBestChild(_estimator);
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

        public static float UCB(TreeNode node)
        {
            if (node.VisitCount == 0)
                return float.MaxValue;

            float ucb = node.ValueSum / node.VisitCount + MathF.Sqrt(2 * MathF.Log(node.Parent.VisitCount) / node.VisitCount);
            return ucb;
        }

        public static float UCBTuned(TreeNode node)
        {
            if (node.VisitCount == 0)
                return float.MaxValue;

            float mean = node.ValueSum / node.VisitCount;
            float var = mean - mean * mean;
            float V = var + MathF.Sqrt(2 * MathF.Log(node.Parent.VisitCount) / node.VisitCount);

            float ucb1tuned = mean + MathF.Sqrt(MathF.Min(0.25f, V) * MathF.Log(node.Parent.VisitCount) / node.VisitCount);
            return ucb1tuned;
        }

        public static float MOSS(TreeNode node)
        {
            if (node.VisitCount == 0)
                return float.MaxValue;

            int k = node.Children == null ? node.MoveMask.PopCount() + 1 : node.Children.Length;
            float V = MathF.Log(node.Parent.VisitCount / (k * node.VisitCount));

            float moss = node.ValueSum / node.VisitCount + MathF.Sqrt(2 * MathF.Max(0, V) / node.VisitCount);
            return moss;
        }
    }
}
