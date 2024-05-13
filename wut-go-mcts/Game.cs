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

        public void Play()
        {
            bool blackToPlay = true;
            while (!_board.Finished)
            {
                // TODO - for now assume Think always results in a legal move
                Player currentPlayer = blackToPlay ? _blackPlayer : _whitePlayer;

                var move = currentPlayer.Think(_board);
                _board.ApplyMove(move);
                blackToPlay = !blackToPlay;
            }

            throw new NotImplementedException();
        }
    }
}
