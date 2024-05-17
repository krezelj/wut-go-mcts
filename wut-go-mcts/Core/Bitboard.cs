using System;
using System.Numerics;

namespace wut_go_mcts.Core
{
    public struct Bitboard
    {
        private static Random _rng = new Random(0);

        private const int N_BITS_BOT = 60;
        
        private const ulong MASK_BOT = 0x0FFBFEFFBFEFFBFE;
        private const ulong MASK_TOP = 0x000000003FEFFBFE;

        private const int BOARD_SIZE = 9;

        private ulong _c0;
        private ulong _c1;

        public Bitboard()
        {
            _c0 = 0;
            _c1 = 0;
        }

        public static Bitboard operator |(Bitboard a, Bitboard b)
        {
            Bitboard c = a;
            c._c0 |= b._c0;
            c._c1 |= b._c1;
            return c;
        }

        public static Bitboard operator &(Bitboard a, Bitboard b)
        {
            Bitboard c = a;
            c._c0 &= b._c0;
            c._c1 &= b._c1;
            return c;
        }

        public static Bitboard operator ^(Bitboard a, Bitboard b)
        {
            Bitboard c = a;
            c._c0 ^= b._c0;
            c._c1 ^= b._c1;
            return c;
        }

        public static Bitboard operator ~(Bitboard bb)
        {
            Bitboard c = bb;
            c._c0 = ~c._c0 & MASK_BOT;
            c._c1 = ~c._c1 & MASK_TOP;
            return c;
        }

        public static Bitboard operator -(Bitboard a, Bitboard b)
        {
            // a & ~b
            Bitboard c = a;
            c._c0 = c._c0 & (~b._c0 & MASK_BOT);
            c._c1 = c._c1 & (~b._c1 & MASK_TOP);
            return c;
        }

        public static bool operator ==(Bitboard a, Bitboard b)
        {
            return a._c0 == b._c0
                && a._c1 == b._c1;
        }

        public static bool operator !=(Bitboard a, Bitboard b)
        {
            return !(a == b);
        }

        public bool IsEmpty()
        {
            return _c0 == 0 && _c1 == 0;
        }

        public void SetBit(int index)
        {
            index += index / BOARD_SIZE + 1;
            if (index < N_BITS_BOT)
                _c0 |= 1UL << index;
            else
                _c1 |= 1UL << index - N_BITS_BOT;
        }

        public void ClearBit(int index)
        {
            index += index / BOARD_SIZE + 1;
            if (index < N_BITS_BOT)
                _c0 &= 0U << index;
            else
                _c1 &= 0U << index - N_BITS_BOT;
        }

        public void ToggleBit(int index)
        {
            index += index / BOARD_SIZE + 1;
            if (index < N_BITS_BOT)
                _c0 ^= 1UL << index;
            else
                _c1 ^= 1UL << index - N_BITS_BOT;
        }

        public bool IsBitSet(int index)
        {
            index += index / BOARD_SIZE + 1;
            if (index < N_BITS_BOT)
                return ((_c0 >> index) & 1) == 1;
            else
                return ((_c1 >> (index - N_BITS_BOT)) & 1) == 1;
        }

        public int PopCount()
        {
            return BitOperations.PopCount(_c0)
                + BitOperations.PopCount(_c1);
        }

        //public int PopLSB()
        //{
        //    if (PopCount() == 0)
        //        return 81;
        //    int i = 0;
        //    if (_c0 != 0)
        //    {
        //        i = BitOperations.TrailingZeroCount(_c0);
        //        _c0 &= _c0 - 1;
        //    }
        //    else if (_c1 != 0)
        //    {
        //        i = BitOperations.TrailingZeroCount(_c1) + N_USEFUL_BITS;
        //        _c1 &= _c1 - 1;
        //    }
        //    else if (_c2 != 0)
        //    {
        //        i = BitOperations.TrailingZeroCount(_c2) + 2 * N_USEFUL_BITS;
        //        _c2 &= _c2 - 1;
        //    }
        //    i -= i / (BOARD_SIZE + 1) + 1;
        //    return i;
        //}

        public Bitboard[] SetBits()
        {
            Bitboard[] output = new Bitboard[PopCount()];
            Bitboard copy = this;
            int i = 0;
            while (copy._c0 != 0)
            {
                output[i]._c0 = copy._c0 & ~(copy._c0 - 1);
                copy._c0 ^= output[i++]._c0;
            }

            while (copy._c1 != 0)
            {
                output[i]._c1 = copy._c1 & ~(copy._c1 - 1);
                copy._c1 ^= output[i++]._c1;
            }
            return output;
        }

        public Bitboard Expand(Bitboard mask)
        {
            Bitboard copy = this;
            copy._c0 = (_c0 | (_c0 << 1) | (_c0 >> 1) | (_c0 >> 10) | (_c0 << 10) | (_c1 << 50)) & MASK_BOT & mask._c0;
            copy._c1 = (_c1 | (_c1 << 1) | (_c1 >> 1) | (_c1 >> 10) | (_c1 << 10) | (_c0 >> 50)) & MASK_TOP & mask._c1;
            return copy;
        }

        public bool ExpandInplace(Bitboard mask)
        {
            ulong old_c0 = _c0;
            ulong old_c1 = _c1;

            _c0 = (_c0 | (_c0 << 1) | (_c0 >> 1) | (_c0 >> 10) | (_c0 << 10) | (_c1 << 50)) & MASK_BOT & mask._c0;
            _c1 = (_c1 | (_c1 << 1) | (_c1 >> 1) | (_c1 >> 10) | (_c1 << 10) | (_c0 >> 50)) & MASK_TOP & mask._c1;

            return old_c0 != _c0 || old_c1 != _c1;
        }

        public Bitboard GetNeighboursMask(Bitboard mask)
        {
            Bitboard copy = this;
            copy._c0 = ((_c0 << 1) | (_c0 >> 1) | (_c0 >> 10) | (_c0 << 10) | (_c1 << 50)) & MASK_BOT & mask._c0;
            copy._c1 = ((_c1 << 1) | (_c1 >> 1) | (_c1 >> 10) | (_c1 << 10) | (_c0 >> 50)) & MASK_TOP & mask._c1;
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
            return ((_c0 & other._c0) != 0) | ((_c1 & other._c1) != 0);
        }

        public Bitboard GetRandomBit()
        {
            int i;
            while (!IsBitSet(i = _rng.Next(81))) { }
            Bitboard position = new Bitboard();
            position.SetBit(i);
            return position;
        }

        public override string ToString()
        {
            string bitstr = Convert.ToString((long)_c1, 2).PadLeft(32, '0') + Convert.ToString((long)_c0, 2).PadLeft(64, '0');
            return bitstr;
        }

    }
}
