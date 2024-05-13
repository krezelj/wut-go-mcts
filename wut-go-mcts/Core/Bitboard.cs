using System.Numerics;

namespace wut_go_mcts.Core
{
    public struct Bitboard
    {
        private const int N_COMPONENTS = 3;
        private const int N_USEFUL_BITS = 30;
        private const int MASK = 0x3FEFFBFE;
        private const int BOARD_SIZE = 9;

        private uint[] _components;

        public Bitboard()
        {
            _components = new uint[N_COMPONENTS];
        }

        public Bitboard(Bitboard bb)
        {
            _components = [
                bb._components[0],
                bb._components[1],
                bb._components[2],
            ];
        }

        private Bitboard(uint[] components)
        {
            _components = components;
        }
        public static Bitboard operator <<(Bitboard bb, int shift)
        {
            return new Bitboard([
                bb._components[0] << shift & MASK,
                (bb._components[1] << shift | bb._components[0] >> N_USEFUL_BITS - shift) & MASK,
                (bb._components[2] << shift | bb._components[1] >> N_USEFUL_BITS - shift) & MASK
            ]);
        }

        public static Bitboard operator >>(Bitboard bb, int shift)
        {
            return new Bitboard([
                (bb._components[0] >> shift | bb._components[1] << N_USEFUL_BITS - shift) & MASK,
                (bb._components[1] >> shift | bb._components[2] << N_USEFUL_BITS - shift) & MASK,
                bb._components[2] >> shift & MASK
            ]);
        }

        public static Bitboard operator |(Bitboard a, Bitboard b)
        {
            return new Bitboard([
                a._components[0] | b._components[0],
                a._components[1] | b._components[1],
                a._components[2] | b._components[2],
            ]);
        }

        public static Bitboard operator &(Bitboard a, Bitboard b)
        {
            return new Bitboard([
                a._components[0] & b._components[0],
                a._components[1] & b._components[1],
                a._components[2] & b._components[2],
            ]);
        }

        public static Bitboard operator ^(Bitboard a, Bitboard b)
        {
            return new Bitboard([
                a._components[0] ^ b._components[0],
                a._components[1] ^ b._components[1],
                a._components[2] ^ b._components[2],
            ]);
        }

        public static Bitboard operator ~(Bitboard bb)
        {
            return new Bitboard([
                ~bb._components[0] & MASK,
                ~bb._components[1] & MASK,
                ~bb._components[2] & MASK,
            ]);
        }

        public static bool operator ==(Bitboard a, Bitboard b)
        {
            return a._components[0] == b._components[0]
                && a._components[1] == b._components[1]
                && a._components[2] == b._components[2];
        }

        public static bool operator !=(Bitboard a, Bitboard b)
        {
            return !(a == b);
        }

        public Bitboard Shift(Direction dir)
        {
            if (dir < 0)
                return this >> -(int)dir;
            else
                return this << (int)dir;
        }

        public bool IsEmpty()
        {
            return _components[0] == 0 && _components[1] == 0 && _components[2] == 0;
        }

        public void SetBit(int index)
        {
            index += index / BOARD_SIZE + 1;
            _components[index / N_USEFUL_BITS] |= 1U << index % N_USEFUL_BITS;
        }

        public void ClearBit(int index)
        {
            index += index / BOARD_SIZE + 1;
            _components[index / N_USEFUL_BITS] &= 0U << index % N_USEFUL_BITS;
        }

        public void ToggleBit(int index)
        {
            index += index / BOARD_SIZE + 1;
            _components[index / N_USEFUL_BITS] ^= 1U << index % N_USEFUL_BITS;
        }

        public bool IsBitSet(int index)
        {
            index += index / BOARD_SIZE + 1;
            return (_components[index / N_USEFUL_BITS] >> index % N_USEFUL_BITS & 1) == 1;
        }

        public int PopCount()
        {
            return BitOperations.PopCount(_components[0])
                + BitOperations.PopCount(_components[1])
                + BitOperations.PopCount(_components[2]);
        }

        public int PopLSB()
        {
            if (PopCount() == 0)
                return 81;
            int i;
            int c = 0;
            if (_components[0] != 0)
                c = 0;
            else if (_components[1] != 0)
                c = 1;
            else if (_components[2] != 0)
                c = 2;

            i = BitOperations.TrailingZeroCount(_components[c]);
            _components[c] &= _components[c] - 1;
            i += c * N_USEFUL_BITS;
            i -= i / (BOARD_SIZE + 1) + 1;
            return i;
        }

        public IEnumerable<Bitboard> SetBits()
        {
            Bitboard copy = new Bitboard(this);
            while (!copy.IsEmpty())
            {
                int lsb = copy.PopLSB();
                Bitboard b = new Bitboard();
                b.SetBit(lsb);
                yield return b;
            }
        }

        public Bitboard Expand()
        {
            return new Bitboard([
                (_components[0] 
                    | (_components[0] << 1) 
                    | (_components[0] >> 1)
                    | (_components[0] >> 10)
                    | (_components[0] << 10)
                    | (_components[1] << 20)) & MASK,
                (_components[1] 
                    | (_components[1] << 1) 
                    | (_components[1] >> 1)
                    | (_components[1] >> 10)
                    | (_components[1] << 10)
                    | (_components[2] << 20) 
                    | (_components[0] >> 20)) & MASK,
                (_components[2] 
                    | (_components[2] << 1) 
                    | (_components[2] >> 1)
                    | (_components[2] >> 10)
                    | (_components[2] << 10)
                    | (_components[1] >> 20)) & MASK,
            ]);
        }

        public Bitboard GetNeighbours()
        {
            return new Bitboard([
                (
                    (_components[0] << 1)
                    | (_components[0] >> 1)
                    | (_components[0] >> 10)
                    | (_components[0] << 10)
                    | (_components[1] << 20)) & MASK,
                (
                    (_components[1] << 1)
                    | (_components[1] >> 1)
                    | (_components[1] >> 10)
                    | (_components[1] << 10)
                    | (_components[2] << 20)
                    | (_components[0] >> 20)) & MASK,
                (
                    (_components[2] << 1)
                    | (_components[2] >> 1)
                    | (_components[2] >> 10)
                    | (_components[2] << 10)
                    | (_components[1] >> 20)) & MASK,
            ]);
        }


        public override string ToString()
        {
            const int ROWS_PER_COMPONENT = 3;

            string output = "";
            for (int i = N_COMPONENTS - 1; i >= 0; i--)
            {
                string binaryRepresentation = Convert.ToString(_components[i], 2).PadLeft(32, '0');
                int offset = 2;
                for (int j = 0; j < ROWS_PER_COMPONENT; j++)
                {
                    output += binaryRepresentation.Substring(offset, BOARD_SIZE);
                    output += '\n';
                    offset += BOARD_SIZE + 1;
                }
            }
            return output;
        }

    }
}
