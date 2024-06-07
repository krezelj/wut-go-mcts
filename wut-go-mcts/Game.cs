using wut_go_mcts.Core;
using wut_go_mcts.Players;

namespace wut_go_mcts
{
    public class Game
    {
        private Player _blackPlayer;
        private Player _whitePlayer;
        private Board _board;

        public Game(Player blackPlayer, Player whitePlayer)
        {
            _blackPlayer = blackPlayer;
            _whitePlayer = whitePlayer;
            _board = new Board();
        }

        public Game(Player blackPlayer, Player whitePlayer, Board board) : this(blackPlayer, whitePlayer)
        {
            _board = board;
        }

        public (float score, string verdict) Play()
        {
            float score;
            string verdict;

            Timer p1Timer = new Timer(4000, 0);
            Timer p2Timer = new Timer(4000, 0);

            bool blackToPlay = true;
            while (!_board.Finished)
            {
                Player currentPlayer = blackToPlay ? _blackPlayer : _whitePlayer;
                Timer currentTimer = blackToPlay ? p1Timer : p2Timer;

                currentTimer.StartTurn();
                var move = currentPlayer.Think(_board, currentTimer);
                if (currentTimer.StopTurn())
                {
                    score = blackToPlay ? 0.0f : 1.0f;
                    verdict = String.Format(
                        "{0} Wins. {1} exceeded time limit.", blackToPlay ? "White" : "Black", blackToPlay ? "Black" : "White");
                    return (score, verdict);
                }


                _board.ApplyMove(move);
                blackToPlay = !blackToPlay;

                // Console.Clear();
                // _board.Display();
            }
            // Thread.Sleep(1000);
            // _board.Display();
            score = _board.Evaluate();
            verdict = String.Format("{0} Wins.", score == 0.0f ? "White" : "Black");
            return (score, verdict);
        }
    }
}
