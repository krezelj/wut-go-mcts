using wut_go_mcts.Core;

namespace wut_go_mcts.Players
{
    public class AllRandomPlayer : Player
    {
        private Random _rng;
        public AllRandomPlayer(int seed)
        {
            _rng = new Random(seed);
        }

        public override Move Think(Board board)
        {
            //var movesMask = board.GetMovesMask();
            //if (board.Pass || movesMask.PopCount() <= 1)
            //    return Move.Pass();
            //return new Move(movesMask.GetRandomBit());

            Bitboard empty = board.Empty;
            return board.GetRandomMove(ref empty);
        }
    }
}
