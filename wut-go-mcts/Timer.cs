using System.Diagnostics;

namespace wut_go_mcts
{
    public class Timer
    {
        private Stopwatch _sw;

        public long Limit;
        public long Increment;
        public long TotalElapsedMiliseconds;
        public long MilisecondsElapsedThisTurn => _sw.ElapsedMilliseconds;
        public long MilisecondsRemaining => Limit - (TotalElapsedMiliseconds + MilisecondsElapsedThisTurn);

        public Timer(long limit, long increment) 
        {
            Limit = limit;
            Increment = increment;

            _sw = new Stopwatch();
        }

        public void StartTurn()
        {
            _sw.Restart();
        }

        public bool StopTurn()
        {
            _sw.Stop();
            TotalElapsedMiliseconds += MilisecondsElapsedThisTurn;
            bool limitReachedDuringTurn = TotalElapsedMiliseconds > Limit;
            Limit += Increment;
            return limitReachedDuringTurn;
        }

    }
}
