using wut_go_mcts.Core;
using wut_go_mcts.Players;
using wut_go_mcts.Players.MCTS;

namespace wut_go_mcts
{
    internal class Program
    {
        static Bitboard GetBitboard()
        {
            string s = "000000000" +
                       "000000000" +
                       "000000000" +
                       "001000000" +
                       "000000000" +
                       "000000000" +
                       "000000000" +
                       "000000000" +
                       "000000000";

            Bitboard bb = new Bitboard();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '1')
                {
                    bb.SetBit(80 - i);
                }
            }
            return bb;
        }

        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            RNG.SetSeed(0);

            //Bitboard b = GetBitboard();
            //Console.WriteLine(b);
            //b.ExpandInplace(~new Bitboard());
            //Console.WriteLine(b);
            // NaiveMCTS p1 = new NaiveMCTS(MCTS.UCB);
            // AllRandomPlayer p1 = new AllRandomPlayer();
            // AllRandomPlayer p2 = new AllRandomPlayer();
            NaiveMCTS p1 = new NaiveMCTS(MCTS.UCB);


            // HumanPlayer p1 = new HumanPlayer(HumanPlayer.ReadMove);
            // HumanPlayer p2 = new HumanPlayer(HumanPlayer.ReadMove);
            // UCT p2 = new UCT(MCTS.UCB, false);
            UCT p2 = new UCT(MCTS.UCB, false);
            Match m = new Match(p1, p2, 500);
            m.Play();
            //Game g = new Game(p1, p2, new Board());
            //var result = g.Play();
            //Console.WriteLine(result.verdict);

            // Board b = Board.GetFromString();
            // UCT uct = new UCT();
            // uct.Think(b);
            // NaiveMCTS naive = new NaiveMCTS();
            // naive.Think(b);
        }
    }
}
