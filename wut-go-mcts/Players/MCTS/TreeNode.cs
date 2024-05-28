using System.Runtime.InteropServices;
using wut_go_mcts.Core;

namespace wut_go_mcts.Players.MCTS
{
    public class TreeNode
    {
        public Board Board;

        public TreeNode Parent;
        public TreeNode[] Children;
        public bool Expanded;
        public bool Terminal => Board.Finished;
        
        // private Bitboard _moveMask;

        public int VisitCount;
        public float ValueSum;

        public TreeNode(Board board, TreeNode parent = null)
        {
            Board = board;
            Expanded = false;
            VisitCount = 0;
            ValueSum = 0f;
            Parent = parent;
        }

        public TreeNode GetBestChild(Func<TreeNode, float> estimator)
        {
            return Children[GetBestChildIndex(estimator)];
        }

        public int GetBestChildIndex(Func<TreeNode, float> estimator)
        {
            float maxValue = float.MinValue;
            int bestIdx = 0;
            for (int i = 0; i < Children.Length; i++)
            {
                float value = estimator(Children[i]);
                if (value == float.MaxValue)
                {
                    // Children[i] = GetNewRandomChild();
                    return i;
                }
                if (value > maxValue)
                {
                    maxValue = value;
                    bestIdx = i;
                }
            }
            return bestIdx;
        }

        public void Expand()
        {
            Expanded = true;
            Move[] moves = Board.GetMoves();
            Children = new TreeNode[moves.Length];
            for (int i = 0; i < moves.Length; i++)
            {
                Board newBoard = Board;
                newBoard.ApplyMove(moves[i]);
                Children[i] = new TreeNode(newBoard, this);
            }
        }

        //private TreeNode GetNewRandomChild()
        //{
        //    // todo randomly select pass
        //    Bitboard movePosition = _moveMask.GetRandomBit();
        //    _moveMask.SubtractInplace(movePosition);
        //    Move move = new Move(movePosition);

        //    Board newBoard = new Board(_board);
        //    newBoard.ApplyMove(move);

        //    return new TreeNode(newBoard);
        //}


    }
}
