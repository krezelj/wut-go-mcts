using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace wut_go_mcts.Core
{
    public struct Board
    {
        private static Random _rng = new Random(0);

        public const int Size = 81;
        private Flags _flags;
        private Bitboard _black;
        private Bitboard _white;
        private Bitboard _oldBlack;
        private Bitboard _oldWhite;
        private int _capturedBlack;
        private int _capturedWhite;

        public Bitboard Empty;
        private Bitboard _full => _black | _white;
        private Bitboard _pBoard => BlackToPlay ? _black : _white;
        private Bitboard _oBoard => BlackToPlay ? _white : _black;
        private Bitboard _oldPBoard => BlackToPlay ? _oldBlack : _oldWhite;
        private Bitboard _oldOBoard => BlackToPlay ? _oldWhite : _oldBlack;

        public bool BlackToPlay => (_flags & Flags.SideToPlay) == 0;
        public bool Pass => (_flags & Flags.Pass) > 0;
        public bool Finished => (_flags & Flags.Finished) > 0;


        public Board()
        {
            _black = new Bitboard();
            _white = new Bitboard();
            _oldBlack = new Bitboard();
            _oldWhite = new Bitboard();
            Empty = ~_black - _white;
            _flags = 0;
            _capturedBlack = 0;
            _capturedWhite = 0;
        }

        public Board(Board board)
        {
            _black = board._black;
            _white = board._white;
            _oldBlack = board._oldBlack;
            _oldWhite = board._oldWhite;
            Empty = board.Empty;
            _flags = board._flags;
            _capturedBlack = board._capturedBlack;
            _capturedWhite = board._capturedWhite;
        }

        public Board(Bitboard black, Bitboard white)
        {
            _black = black;
            _white = white;
        }

        public Move[] GetMoves()
        {
            Bitboard moveMask = GetMovesMask();
            Move[] moves = new Move[moveMask.PopCount() + 1];
            int i = 0;
            foreach (var movePosition in moveMask.SetBits())
                moves[i++] = new Move(movePosition);
            moves[i] = Move.Pass();
            return moves;
        }

        public Bitboard GetMovesMask()
        {
            Bitboard movesToProve = Empty;
            Bitboard nonSuicides = new Bitboard();

            ProveConnectedToOpenLiberties(ref movesToProve, ref nonSuicides);
            ProveConnectedToClosedLiberties(ref movesToProve, ref nonSuicides);
            ProveCapturesWithNoLiberties(ref movesToProve, ref nonSuicides);

            // positions where there was own stone in the previous position but where we can also place a stone now
            Bitboard koDiff = _oldPBoard & nonSuicides;
            if (koDiff.PopCount() == 1)
            {
                Bitboard captures = GetCapturedByMove(koDiff);
                // if the position is not actually repeated
                if ((_oBoard - captures) == _oldOBoard)
                    nonSuicides.SubtractInplace(koDiff);
            }

            return nonSuicides;
        }

        private Bitboard GetCapturedByMove(Bitboard movePosition)
        {
            //Bitboard captures = new Bitboard();
            //Bitboard checkedMask = new Bitboard();
            //Bitboard emptyAfterMove = Empty - movePosition;
            //foreach (var adjecentOpponent in movePosition.GetNeighboursMask(_oBoard).SetBits())
            //{
            //    if (adjecentOpponent.Intersects(checkedMask))
            //        continue; // the capture nature of this stone is already determined

            //    Bitboard floodfill = adjecentOpponent.Floodfill(_oBoard);
            //    if (floodfill.Expand(emptyAfterMove).IsEmpty()) // no liberties
            //        captures |= floodfill;
            //    checkedMask |= floodfill;
            //}
            //return captures;

            Bitboard captures = new Bitboard();
            Bitboard newEmpty = Empty - movePosition;
            Bitboard open = newEmpty | _oBoard;
            Bitboard adjecentOpponents = movePosition.GetNeighboursMask(_oBoard);
            Bitboard adjecentOpponent;
            while (adjecentOpponents.PopLSB(out adjecentOpponent))
            {
                Bitboard floodfill = adjecentOpponent;
                bool libertyFound = false;
                while (floodfill.ExpandInplace(open))
                {
                    if (floodfill.Intersects(newEmpty))
                    {
                        libertyFound = true;
                        adjecentOpponents.SubtractInplace(floodfill);
                        break;
                    }
                }
                if (!libertyFound)
                    captures.OrInplace(floodfill - newEmpty);
            }
            return captures;
        }

        private void ProveConnectedToOpenLiberties(ref Bitboard movesToProve, ref Bitboard proven)
        {
            Bitboard open = movesToProve | _pBoard;
            Bitboard hasAccessToLiberty = Empty.GetNeighboursMask(Empty).Floodfill(open);
            proven.OrInplace(movesToProve & hasAccessToLiberty);
            movesToProve.SubtractInplace(proven);
        }

        private void ProveConnectedToClosedLiberties(ref Bitboard movesToProve, ref Bitboard proven)
        {
            Bitboard open = _pBoard | Empty;
            foreach (var move in movesToProve.SetBits())
            {
                if (move.Intersects(proven))
                    continue; // move already proven in a previous iteration
                Bitboard floodfill = move.Floodfill(open);
                if (floodfill.Intersects(Empty - move))
                    proven .OrInplace(movesToProve & floodfill);
            }
            movesToProve.SubtractInplace(proven);
        }

        private void ProveCapturesWithNoLiberties(ref Bitboard movesToProve, ref Bitboard proven)
        {
            foreach (var move in movesToProve.SetBits())
            {
                Bitboard newEmpty = Empty - move;
                Bitboard open = _oBoard | newEmpty;
                foreach (var adjecentOpponent in move.GetNeighboursMask(_oBoard).SetBits())
                {
                    if (!ConnectsToLiberty(adjecentOpponent, open, newEmpty))
                    {
                        proven.OrInplace(move);
                        movesToProve.SubtractInplace(move); // TODO Move subtraction until the end of the function
                        break;
                    }
                }
            }
        }

        private bool ConnectsToLiberty(Bitboard position, Bitboard open, Bitboard empty)
        {
            Bitboard floodfill = position;
            while (floodfill.ExpandInplace(open))
            {
                if (floodfill.Intersects(empty))
                    return true;
            }
            return false;
        }

        public Move GetRandomMove(ref Bitboard allowedPositions)
        {
            bool koFound = false;
            while (!allowedPositions.IsEmpty())
            {
                Bitboard position = allowedPositions.GetRandomBit();
                if (!koFound && (_pBoard | position) == _oldPBoard)
                {
                    // potential ko
                    Bitboard koCaptures = GetCapturedByMove(position);
                    if ((_oBoard - koCaptures) == _oldOBoard)
                    {
                        // ko
                        koFound = true;
                        allowedPositions.SubtractInplace(position);
                        continue;
                    }
                }

                // proven to not be ko

                // does connect to liberty? if yes immediately return
                Bitboard newEmpty = Empty - position;
                Bitboard open = _pBoard | newEmpty;
                if (ConnectsToLiberty(position, open, newEmpty))
                    return new Move(position);

                // if not check if capture
                Bitboard captures = GetCapturedByMove(position);
                if (!captures.IsEmpty())
                    return new Move(position, captures);

                // not a capture, try again
                allowedPositions.SubtractInplace(position);
            }
            return Move.Pass();
        }

        public void ApplyMove(Move move)
        {
            _oldBlack = _black;
            _oldWhite = _white;

            if (move.IsPass)
            {
                if (Pass)
                {
                    _flags |= Flags.Finished;
                }
                _flags |= Flags.Pass;
            }

            if (!move.CapturesCalculated)
            {
                move.Captures = GetCapturedByMove(move.Position);
                move.CapturesCalculated = true;
            }

            if (BlackToPlay)
            {
                _black.OrInplace(move.Position);
                _white.SubtractInplace(move.Captures);
                _capturedWhite += move.Captures.PopCount();
                _flags |= Flags.SideToPlay;
            }
            else
            {
                _white.OrInplace(move.Position);
                _black.SubtractInplace(move.Captures);
                _capturedBlack += move.Captures.PopCount();
                _flags = _flags & ~Flags.SideToPlay;
            }
            Empty = ~_black - _white;
        }

        public float Evaluate()
        {
            float countDiff = (_black.PopCount() - _white.PopCount()) * 1f;
            countDiff += _capturedWhite - _capturedBlack;

            Bitboard blackTerritory = new Bitboard();
            Bitboard whiteTerritory = new Bitboard();
            Bitboard toVisit = Empty;
            Bitboard position;

            while (toVisit.PopLSB(out position))
            {
                Bitboard floodfill = position.Floodfill(Empty);
                floodfill.ExpandInplace();
                bool intersectsWithBlack = floodfill.Intersects(_black);
                bool intersectsWithWhite = floodfill.Intersects(_white);
                if (intersectsWithBlack && !intersectsWithWhite)
                {
                    blackTerritory.OrInplace(floodfill);
                    toVisit.SubtractInplace(floodfill);
                }
                else if (intersectsWithWhite && !intersectsWithBlack)
                {
                    whiteTerritory.OrInplace(floodfill);
                    toVisit.SubtractInplace(floodfill);
                }
                else // neutral territory, get the closest colour
                {
                    floodfill = position;
                    while (floodfill.ExpandInplace())
                    {
                        if (floodfill.Intersects(_black))
                        {
                            countDiff += 1f;
                            break;
                        }
                        if (floodfill.Intersects(_white))
                        {
                            countDiff -= 1f;
                            break;
                        }
                    }
                }
            }

            countDiff += blackTerritory.PopCount() - whiteTerritory.PopCount();
            countDiff -= 0.5f; // komi
            return countDiff > 0 ? 1.0f : 0f;
        }

        public override string ToString()
        {
            string columnIndicators = "--A-B-C-D-E-F-G-H-I--";
            string output = "";

            var moveMask = GetMovesMask();
            var moves = GetMoves();
            for (int i = 0; i < moves.Length; i++)
            {
                moves[i].Captures = GetCapturedByMove(moves[i].Position);
                moves[i].CapturesCalculated = true;
            }

            Bitboard blackTerritory = new Bitboard();
            Bitboard whiteTerritory = new Bitboard();
            Bitboard toVisit = Empty;
            Bitboard position;

            while (toVisit.PopLSB(out position))
            {
                Bitboard floodfill = position.Floodfill(Empty);
                floodfill.ExpandInplace(_full | Empty);
                bool intersectsWithBlack = floodfill.Intersects(_black);
                bool intersectsWithWhite = floodfill.Intersects(_white);
                if (intersectsWithBlack && !intersectsWithWhite)
                {
                    blackTerritory.OrInplace(floodfill);
                    toVisit.SubtractInplace(floodfill);
                }
                else if (intersectsWithWhite && !intersectsWithBlack)
                {
                    whiteTerritory.OrInplace(floodfill);
                    toVisit.SubtractInplace(floodfill);
                }
                else // neutral territory, get the closest colour
                {
                    floodfill = position;
                    bool closestFound = false;
                    while (floodfill.ExpandInplace(Empty | _full))
                    {
                        bool blackFound = floodfill.Intersects(_black);
                        bool whiteFound = floodfill.Intersects(_white);
                        closestFound = blackFound || whiteFound;
                        if (blackFound && !whiteFound)
                            blackTerritory |= position;
                        if (!blackFound && whiteFound)
                            whiteTerritory |= position;
                        if (closestFound)
                            break;
                    }
                }
            }

            output += columnIndicators + '\n';
            for (int i = 80; i >= 0; i--)
            {
                if (i % 9 == 8)
                    output += $"{i / 9 + 1} ";
                if (_black.IsBitSet(i))
                    output += "X ";
                else if (_white.IsBitSet(i))
                    output += "O ";
                else if (blackTerritory.IsBitSet(i))
                    output += "x ";
                else if (whiteTerritory.IsBitSet(i))
                    output += "o ";
                else if (moves.ToList().FindAll(m => m.Position.IsBitSet(i) && !m.Captures.IsEmpty()).Count != 0)
                    output += "* ";
                else if (moveMask.IsBitSet(i))
                    output += "+ ";
                else
                    output += ". ";
                if (i % 9 == 0)
                    output += $"{i / 9 + 1} \n";
            }
            output += columnIndicators + '\n';

            return output;
        }

        public void Display()
        {
            var bgcmap = new Dictionary<char, ConsoleColor>()
            {
                { 'X', ConsoleColor.Black },
                { 'O', ConsoleColor.White },
                { 'x', ConsoleColor.DarkGray },
                { 'o', ConsoleColor.Gray },
                { '+', ConsoleColor.Green },
                { '.', ConsoleColor.Magenta },
                { '*', ConsoleColor.Red },
                { '\n', ConsoleColor.Black },
            };
            var fgcmap = new Dictionary<char, ConsoleColor>()
            {
                { 'X', ConsoleColor.White },
                { 'O', ConsoleColor.Black },
                { 'x', ConsoleColor.White },
                { 'o', ConsoleColor.White },
                { '+', ConsoleColor.Magenta },
                { '.', ConsoleColor.White },
                { '*', ConsoleColor.White },
            };
            string representation = ToString();
            foreach (var c in representation)
            {
                if (c != ' ')
                {
                    Console.BackgroundColor = bgcmap.ContainsKey(c) ? bgcmap[c] : ConsoleColor.DarkRed;
                    Console.ForegroundColor = fgcmap.ContainsKey(c) ? fgcmap[c] : ConsoleColor.White;
                }
                Console.Write(c);
            }
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public static Board GetFromString()
        {
            string s = "OOOO..OOO" +
                       "OOOOOOOOO" +
                       "OOOOOOOOO" +
                       "OOOOOOOOO" +
                       "OOOOOOOOO" +
                       "OOOOOOOOO" +
                       "OOOOOOOOO" +
                       "OOOOOOOOO" +
                       "O.OOOOO.O";
            Board board = new Board();
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case 'X':
                        board._black.SetBit(80 - i);
                        break;
                    case 'O':
                        board._white.SetBit(80 - i);
                        break;
                }
            }
            board.Empty = ~board._black - board._white;
            return board;
        }
    }
}
