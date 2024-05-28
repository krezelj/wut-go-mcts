using wut_go_mcts.Core;

namespace wut_go_mcts.Players
{
    public class AllRandomPlayer : Player
    {
        public override Move Think(Board board)
        {
            Bitboard empty = board.Empty;
            return board.GetRandomMove(ref empty);
        }
    }
}
