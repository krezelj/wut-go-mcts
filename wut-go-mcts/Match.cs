using wut_go_mcts.Core;
using wut_go_mcts.Players;

namespace wut_go_mcts
{
    public class Match
    {
        int GamesPerSide;
        Player Player1;
        Player Player2;


        public Match(Player p1, Player p2, int gamesPerSide)
        {
            GamesPerSide = gamesPerSide;
            Player1 = p1;
            Player2 = p2;
        }

        public float Play()
        {
            float totalScore = 0.0f;
            int p1Wins = 0;
            int p2Wins = 0;

            Game game;
            for (int i = 0; i < GamesPerSide; i++)
            {
                // RNG.SetSeed(i);

                Console.Write("Starting Game {0} (1/2)...\t", i + 1);
                game = new Game(Player1, Player2, new Board());
                var result = game.Play();
                p1Wins += (int)result.score;
                p2Wins += 1 - (int)result.score;
                Console.WriteLine("{0} ({1}-{2})", result.verdict, p1Wins, p2Wins);

                Console.Write("Starting Game {0} (2/2)...\t", i + 1);
                game = new Game(Player2, Player1, new Board());
                result = game.Play();
                p1Wins += 1 - (int)result.score;
                p2Wins += (int)result.score;
                Console.WriteLine("{0} ({1}-{2})", result.verdict, p1Wins, p2Wins);
            }
            return totalScore;
        }
    }
}
