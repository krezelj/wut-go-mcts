namespace wut_go_mcts
{
    public struct Move
    {
        public Bitboard Position;
        public Bitboard Captures;
        public bool IsPass => Position.IsEmpty();

        public Move()
        {
            Position = new Bitboard();
            Captures = new Bitboard();
        }

        public Move(Bitboard position, Bitboard captures)
        {
            Position = position;
            Captures = captures;
        }

        public static Move Pass()
        {
            return new Move(new Bitboard(), new Bitboard());
        }
    }
}
