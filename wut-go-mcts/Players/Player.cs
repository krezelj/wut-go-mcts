using wut_go_mcts.Core;

namespace wut_go_mcts.Players
{
    public abstract class Player
    {
        public abstract Move Think(Board board, Timer timer);
    }
}
