namespace wut_go_mcts.Core
{
    public struct Move
    {
        public Bitboard Position;
        public Bitboard Captures;
        public bool CapturesCalculated;
        public bool IsPass => Position.IsEmpty();

        public Move(Bitboard position)
        {
            Position = position;
            CapturesCalculated = false;
        }

        public Move(Bitboard position, Bitboard captures)
        {
            Position = position;
            Captures = captures;
            CapturesCalculated = true;
        }

        public static Move Pass()
        {
            return new Move(new Bitboard(), new Bitboard());
        }
    }
}
