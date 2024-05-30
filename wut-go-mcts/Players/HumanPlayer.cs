using wut_go_mcts.Core;

namespace wut_go_mcts.Players
{
    public class HumanPlayer : Player
    {
        private Func<Board, Move> _thinkCallback;

        public HumanPlayer(Func<Board, Move> thinkCallback) 
        {
            _thinkCallback = thinkCallback;
        }

        public override Move Think(Board board, Timer timer)
        {
            return _thinkCallback(board);
        }

        public static Move ReadMove(Board board)
        {
            var moves = board.GetMoves();
            Bitboard movePositions = new Bitboard();
            foreach (var move in moves)
                movePositions |= move.Position;

            board.Display();
            Console.Write("\nMove ({0}): ", board.BlackToPlay ? "Black" : "White");
            string input = Console.ReadLine();
            Console.Clear();
            if (input == "Pass")
                return Move.Pass();
            int row = int.Parse(input[1].ToString());
            int col = (8 - (input[0] - 65)); 

            int idx = (row - 1) * 9 + col;
            foreach (var move in moves)
                if (move.Position.IsBitSet(idx))
                    return move;
            return moves[0];
        }
    }
}
