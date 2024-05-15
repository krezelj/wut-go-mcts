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
            var moves = board.GetMoves();
            int idx = _rng.Next(moves.Length);
            return moves[idx];
        }
    }
}
