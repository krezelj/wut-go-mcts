using System.Runtime.InteropServices;
using wut_go_mcts.Core;

namespace wut_go_mcts.Players.MCTS
{
    public class TreeNode
    {
        public Board Board;
        public Bitboard MoveMask;

        public TreeNode Parent;
        public TreeNode[] Children;
        public bool Expanded;
        public int PopulatedChildren;
        public bool Terminal => Board.Finished;
        
        // private Bitboard _moveMask;

        public int VisitCount;
        public float ValueSum;

        public TreeNode(Board board, TreeNode parent = null)
        {
            Board = board;
            MoveMask = board.GetMovesMask();

            Expanded = false;
            VisitCount = 0;
            ValueSum = 0f;
            Parent = parent;
            PopulatedChildren = 0;
        }

        public TreeNode GetBestChild(Func<TreeNode, float> estimator)
        {
            int bestIdx = GetBestChildIndex(estimator);
            return Children[bestIdx];
        }

        public int GetBestChildIndex(Func<TreeNode, float> estimator)
        {
            if (Children == null)
            {
                Children = new TreeNode[MoveMask.PopCount() + 1];

                Board newBoard = new Board(Board);
                newBoard.ApplyMove(Move.Pass());

                Children[PopulatedChildren] = new TreeNode(newBoard, this);
                return PopulatedChildren++;
            }

            if (MoveMask.PopCount() > 0)
            {
                Move move = new Move(MoveMask.GetRandomBit());
                MoveMask.SubtractInplace(move.Position);

                Board newBoard = new Board(Board);
                newBoard.ApplyMove(Move.Pass());

                Children[PopulatedChildren] = new TreeNode(newBoard, this);
                return PopulatedChildren++;
            }

            float maxValue = float.MinValue;
            int bestIdx = 0;
            for (int i = 0; i < Children.Length; i++)
            {
                float value = estimator(Children[i]);
                if (value == float.MaxValue)
                {
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
            //int moveCount = MoveMask.PopCount() + 1;

            //if (Children == null)
            //{    
            //    Children = new TreeNode[moveCount];
            //    Board newBoard = new Board(Board);
            //    newBoard.ApplyMove(Move.Pass());
            //    Children[PopulatedChildren++] = new TreeNode(newBoard, this);
            //}
            //if (moveCount > 0)
            //{
            //    Board newBoard;
            //    foreach (var movePosition in MoveMask.SetBits())
            //    {
            //        Move move = new Move(movePosition);
            //        newBoard = new Board(Board);
            //        newBoard.ApplyMove(move);
            //        Children[PopulatedChildren++] = new TreeNode(newBoard, this);
            //    }
            //}

            MoveMask = new Bitboard();
            Move[] moves = Board.GetMoves();
            Children = new TreeNode[moves.Length];
            for (int i = 0; i < moves.Length; i++)
            {
                Board newBoard = new Board(Board);
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
