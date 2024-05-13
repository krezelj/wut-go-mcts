namespace wut_go_mcts.Core
{
    public enum Direction
    {
        Left = 1, Right = -1, Up = 10, Down = -10,
        LeftUp = 11, LeftDown = -9, RightUp = 9, RightDown = -11
    };

    public enum Flags { SideToPlay = 0b001, Pass = 0b010, Finished = 0b100 }
}
