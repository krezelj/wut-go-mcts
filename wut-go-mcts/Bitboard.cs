using System.ComponentModel;
using System.Numerics;

namespace wut_go_mcts
{
    public struct Bitboard
    {
        private const int N_COMPONENTS = 3;
        private const int N_USEFUL_BITS = 27;
        private const int USEFUL_MASK = 0x7FFFFFF;

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

        private static uint LSC(uint component, int shift)
        {
            return shift >= sizeof(uint) * 8 ? 0 : component << shift;
        }

        private static uint RSC(uint component, int shift)
        {
            return shift >= sizeof(uint) * 8 ? 0 : component >> shift;
        }

        public static Bitboard operator <<(Bitboard bb, int shift)
        {
            return new Bitboard([
                LSC(bb._components[0], shift) 
                    & USEFUL_MASK,
                (LSC(bb._components[1], shift) 
                    | RSC(bb._components[0], N_USEFUL_BITS - shift)) 
                    & USEFUL_MASK,
                (LSC(bb._components[2], shift) 
                    | RSC(bb._components[1], N_USEFUL_BITS - shift) 
                    | RSC(bb._components[0], 2 * N_USEFUL_BITS - shift)) 
                    & USEFUL_MASK
            ]);
        }

        public static Bitboard operator >>(Bitboard bb, int shift)
        {
            return new Bitboard([
                (RSC(bb._components[0], shift)
                    | LSC(bb._components[1], N_USEFUL_BITS - shift)
                    | LSC(bb._components[2], 2 * N_USEFUL_BITS - shift))
                    & USEFUL_MASK,
                (RSC(bb._components[1], shift) 
                    | LSC(bb._components[2], N_USEFUL_BITS - shift))
                    & USEFUL_MASK,
                RSC(bb._components[2], shift) 
                    & USEFUL_MASK
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
                ~bb._components[0] & USEFUL_MASK,
                ~bb._components[1] & USEFUL_MASK,
                ~bb._components[2] & USEFUL_MASK,
            ]);
        }

        public static bool operator ==(Bitboard a, Bitboard b)
        {
            for (int i = 0; i < N_COMPONENTS; i++)
                if (a._components[i] != b._components[i])
                    return false;
            return true;
        }

        public static bool operator !=(Bitboard a, Bitboard b)
        {
            return !(a == b);
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < N_COMPONENTS; i++)
                if (_components[i] != 0)
                    return false;
            return true;
        }

        public void SetBit(int index)
        {
            _components[index / N_USEFUL_BITS] |= 1U << (index % N_USEFUL_BITS);
        }

        public void ClearBit(int index)
        {
            _components[index / N_USEFUL_BITS] &= 0U << (index % N_USEFUL_BITS);
        }

        public void ToggleBit(int index)
        {
            _components[index / N_USEFUL_BITS] ^= 1U << (index % N_USEFUL_BITS);
        }

        public bool IsBitSet(int index)
        {
            return (_components[index / N_USEFUL_BITS] >> (index % N_USEFUL_BITS)) == 1;
        }

        public int PopCount()
        {
            int popCount = 0;
            for (int i = 0; i < N_COMPONENTS; i++)
            {
                popCount += BitOperations.PopCount(_components[i]);
            }
            return popCount;
        }

        public int PopLSB()
        {
            throw new NotImplementedException();
        }

        public void Expand()
        {
            throw new NotImplementedException();
        }


        public override string ToString()
        {
            const int BOARD_WIDTH = 9;
            const int ROWS_PER_COMPONENT = 3;

            string output = "";
            for (int i = N_COMPONENTS - 1; i >= 0; i--)
            {
                string binaryRepresentation = Convert.ToString(_components[i], 2).PadLeft(32, '0');
                int offset = 5;
                for (int j = 0; j < ROWS_PER_COMPONENT; j++)
                {
                    output += binaryRepresentation.Substring(offset, BOARD_WIDTH);
                    output += '\n';
                    offset += BOARD_WIDTH;
                }
            }
            return output;
        }

    }
}
