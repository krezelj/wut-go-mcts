using System.Numerics;

namespace wut_go_mcts.Core
{
    public struct Bitboard
    {
        private const int N_COMPONENTS = 3;
        private const int N_USEFUL_BITS = 30;
        private const int MASK = 0x3FEFFBFE;
        private const int BOARD_SIZE = 9;

        private uint _c0;
        private uint _c1;
        private uint _c2;

        public Bitboard()
        {
            _c0 = 0;
            _c1 = 0;
            _c2 = 0;
        }

        public static Bitboard operator |(Bitboard a, Bitboard b)
        {
            Bitboard c = a;
            c._c0 |= b._c0;
            c._c1 |= b._c1;
            c._c2 |= b._c2;
            return c;
        }

        public static Bitboard operator &(Bitboard a, Bitboard b)
        {
            Bitboard c = a;
            c._c0 &= b._c0;
            c._c1 &= b._c1;
            c._c2 &= b._c2;
            return c;
        }

        public static Bitboard operator ^(Bitboard a, Bitboard b)
        {
            Bitboard c = a;
            c._c0 ^= b._c0;
            c._c1 ^= b._c1;
            c._c2 ^= b._c2;
            return c;
        }

        public static Bitboard operator ~(Bitboard bb)
        {
            Bitboard c = bb;
            c._c0 = ~c._c0 & MASK;
            c._c1 = ~c._c1 & MASK;
            c._c2 = ~c._c2 & MASK;
            return c;
        }

        public static Bitboard operator-(Bitboard a, Bitboard b)
        {
            // a & ~b
            Bitboard c = a;
            c._c0 = c._c0 & (~b._c0 & MASK);
            c._c1 = c._c1 & (~b._c1 & MASK);
            c._c2 = c._c2 & (~b._c2 & MASK);
            return c;
        }

        public static bool operator ==(Bitboard a, Bitboard b)
        {
            return a._c0 == b._c0
                && a._c1 == b._c1
                && a._c2 == b._c2;
        }

        public static bool operator !=(Bitboard a, Bitboard b)
        {
            return !(a == b);
        }

        public bool IsEmpty()
        {
            return _c0 == 0 && _c1 == 0 && _c2 == 0;
        }

        public void SetBit(int index)
        {
            index += index / BOARD_SIZE + 1;
            if (index < N_USEFUL_BITS)
                _c0 |= 1U << index % N_USEFUL_BITS;
            else if (index < 2 * N_USEFUL_BITS)
                _c1 |= 1U << index % N_USEFUL_BITS;
            else
                _c2 |= 1U << index % N_USEFUL_BITS;
        }

        public void ClearBit(int index)
        {
            index += index / BOARD_SIZE + 1;
            if (index < N_USEFUL_BITS)
                _c0 &= 0U << index % N_USEFUL_BITS;
            else if (index < 2 * N_USEFUL_BITS)
                _c1 &= 0U << index % N_USEFUL_BITS;
            else
                _c2 &= 0U << index % N_USEFUL_BITS;
        }

        public void ToggleBit(int index)
        {
            index += index / BOARD_SIZE + 1;
            if (index < N_USEFUL_BITS)
                _c0 ^= 1U << index % N_USEFUL_BITS;
            else if (index < 2 * N_USEFUL_BITS)
                _c1 ^= 1U << index % N_USEFUL_BITS;
            else
                _c2 ^= 1U << index % N_USEFUL_BITS;
        }

        public bool IsBitSet(int index)
        {
            index += index / BOARD_SIZE + 1;
            if (index < N_USEFUL_BITS)
                return (_c0 >> index % N_USEFUL_BITS & 1) == 1;
            else if (index < 2 * N_USEFUL_BITS)
                return (_c1 >> index % N_USEFUL_BITS & 1) == 1;
            else
                return (_c2 >> index % N_USEFUL_BITS & 1) == 1;

        }

        public int PopCount()
        {
            return BitOperations.PopCount(_c0)
                + BitOperations.PopCount(_c1)
                + BitOperations.PopCount(_c2);
        }

        public int PopLSB()
        {
            if (PopCount() == 0)
                return 81;
            int i = 0;
            if (_c0 != 0)
            {
                i = BitOperations.TrailingZeroCount(_c0);
                _c0 &= _c0 - 1;
            }
            else if (_c1 != 0)
            {
                i = BitOperations.TrailingZeroCount(_c1) + N_USEFUL_BITS;
                _c1 &= _c1 - 1;
            }
            else if (_c2 != 0)
            {
                i = BitOperations.TrailingZeroCount(_c2) + 2 * N_USEFUL_BITS;
                _c2 &= _c2 - 1;
            }
            i -= i / (BOARD_SIZE + 1) + 1;
            return i;
        }

        public Bitboard[] SetBits()
        {
            Bitboard[] output = new Bitboard[PopCount()];
            Bitboard placeholder = new Bitboard();
            Bitboard copy = this;
            int i = 0;
            while (copy._c0 != 0)
            {
                placeholder._c0 = 1U << BitOperations.TrailingZeroCount(copy._c0);
                copy._c0 &= copy._c0 - 1;
                output[i++] = placeholder;
            }
            placeholder._c0 = 0;

            while (copy._c1 != 0)
            {
                placeholder._c1 = 1U << BitOperations.TrailingZeroCount(copy._c1);
                copy._c1 &= copy._c1 - 1;
                output[i++] = placeholder;
            }
            placeholder._c1 = 0;

            while (copy._c2 != 0)
            {
                placeholder._c2 = 1U << BitOperations.TrailingZeroCount(copy._c2);
                copy._c2 &= copy._c2 - 1;
                output[i++] = placeholder;
            }
            return output;
        }

        public Bitboard Expand()
        {
            Bitboard copy = this;
            copy._c0 |= ((_c0 << 1) | (_c0 >> 1) | (_c0 >> 10) | (_c0 << 10) | (_c1 << 20)) & MASK;
            copy._c1 |= ((_c1 << 1) | (_c1 >> 1) | (_c1 >> 10) | (_c1 << 10) | (_c2 << 20) | (_c0 >> 20)) & MASK;
            copy._c2 |= ((_c2 << 1) | (_c2 >> 1) | (_c2 >> 10) | (_c2 << 10) | (_c1 >> 20)) & MASK;
            return copy;
        }

        public Bitboard Expand(Bitboard mask)
        {
            Bitboard copy = this;
            copy._c0 = (_c0 | (_c0 << 1) | (_c0 >> 1) | (_c0 >> 10) | (_c0 << 10) | (_c1 << 20)) & MASK & mask._c0;
            copy._c1 = (_c1 | (_c1 << 1) | (_c1 >> 1) | (_c1 >> 10) | (_c1 << 10) | (_c2 << 20) | (_c0 >> 20)) & MASK & mask._c1;
            copy._c2 = (_c2 | (_c2 << 1) | (_c2 >> 1) | (_c2 >> 10) | (_c2 << 10) | (_c1 >> 20)) & MASK & mask._c2;
            return copy;
        }

        public bool ExpandInplace(Bitboard mask)
        {
            uint old_c0 = _c0;
            uint old_c1 = _c1;
            uint old_c2 = _c2;

            _c0 = (_c0 | (_c0 << 1) | (_c0 >> 1) | (_c0 >> 10) | (_c0 << 10) | (_c1 << 20)) & MASK & mask._c0;
            _c1 = (_c1 | (_c1 << 1) | (_c1 >> 1) | (_c1 >> 10) | (_c1 << 10) | (_c2 << 20) | (_c0 >> 20)) & MASK & mask._c1;
            _c2 = (_c2 | (_c2 << 1) | (_c2 >> 1) | (_c2 >> 10) | (_c2 << 10) | (_c1 >> 20)) & MASK & mask._c2;

            return old_c0 != _c0 || old_c1 != _c1 || old_c2 != _c2;
        }

        public Bitboard GetNeighbours()
        {
            Bitboard copy = this;
            copy._c0 = ((_c0 << 1) | (_c0 >> 1) | (_c0 >> 10) | (_c0 << 10) | (_c1 << 20)) & MASK;
            copy._c1 = ((_c1 << 1) | (_c1 >> 1) | (_c1 >> 10) | (_c1 << 10) | (_c2 << 20) | (_c0 >> 20)) & MASK;
            copy._c2 = ((_c2 << 1) | (_c2 >> 1) | (_c2 >> 10) | (_c2 << 10) | (_c1 >> 20)) & MASK;
            return copy;
        }

        public Bitboard GetNeighboursMask(Bitboard mask)
        {
            Bitboard copy = this;
            copy._c0 = ((_c0 << 1) | (_c0 >> 1) | (_c0 >> 10) | (_c0 << 10) | (_c1 << 20)) & MASK & mask._c0;
            copy._c1 = ((_c1 << 1) | (_c1 >> 1) | (_c1 >> 10) | (_c1 << 10) | (_c2 << 20) | (_c0 >> 20)) & MASK & mask._c1;
            copy._c2 = ((_c2 << 1) | (_c2 >> 1) | (_c2 >> 10) | (_c2 << 10) | (_c1 >> 20)) & MASK & mask._c2;
            return copy;
        }

        public Bitboard Floodfill(Bitboard open)
        {
            Bitboard floodfill = this;
            while (floodfill.ExpandInplace(open)) { }
            return floodfill;
        }

        public bool Intersects(Bitboard other)
        {
            return ((_c0 & other._c0) != 0) | ((_c1 & other._c1) != 0) | ((_c2 & other._c2) != 0);
        }

        public override string ToString()
        {
            const int ROWS_PER_COMPONENT = 3;

            string output = "";
            return Convert.ToString(_c2, 2).PadLeft(32, '0')
                + Convert.ToString(_c1, 2).PadLeft(32, '0')
                + Convert.ToString(_c0, 2).PadLeft(32, '0');
            //for (int i = N_COMPONENTS - 1; i >= 0; i--)
            //{
            //    string binaryRepresentation = Convert.ToString(_components[i], 2).PadLeft(32, '0');
            //    int offset = 2;
            //    for (int j = 0; j < ROWS_PER_COMPONENT; j++)
            //    {
            //        output += binaryRepresentation.Substring(offset, BOARD_SIZE);
            //        output += '\n';
            //        offset += BOARD_SIZE + 1;
            //    }
            //}
            return output;
        }

    }
}
