using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wut_go_mcts
{
    public static class RNG
    {
        public static Random Generator;

        static RNG()
        {
            Generator = new Random();
        }

        public static void SetSeed(int seed)
        {
            Generator = new Random(seed);
        }

    }
}
