namespace wut_go_mcts.Core
{
    public struct Board
    {
        public const int Size = 81;
        private Flags _flags;
        private Bitboard _black;
        private Bitboard _white;
        private Bitboard _oldBlack;
        private Bitboard _oldWhite;
        private Bitboard _empty;
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
            _empty = ~_black - _white;
            _flags = 0;
        }

        public Board(Board board)
        {
            _black = board._black;
            _white = board._white;
            _oldBlack = board._oldBlack;
            _oldWhite = board._oldWhite;
            _flags = board._flags;
        }

        public Board(Bitboard black, Bitboard white)
        {
            _black = black;
            _white = white;
        }

        public Move[] GetMoves()
        {
            Bitboard movesToProve = _empty;
            Bitboard nonSuicides = new Bitboard();

            nonSuicides |= ProveConnectedToOpenLiberties(ref movesToProve);
            nonSuicides |= ProveConnectedToClosedLiberties(ref movesToProve);
            nonSuicides |= ProveCapturesWithNoLiberties(ref movesToProve);

            int moveCount = nonSuicides.PopCount();
            // positions where there was own stone in the previous position but where we can also place a stone now
            Bitboard koDiff = _oldPBoard & nonSuicides;
            Move koMove = Move.Pass();
            if (koDiff.PopCount() == 1)
            {
                Bitboard captures = GetCapturedByMove(koDiff);

                // if the position is not actually repeated
                if ((_oBoard - captures) != _oldOBoard)
                    koMove = new Move(koDiff, captures);
                else
                    moveCount--;

                // either way, remove the move
                nonSuicides = nonSuicides - koDiff;
            }

            Move[] moves = new Move[moveCount];
            if (!koMove.IsPass)
                moves[moveCount - 1] = koMove;
            int i = 0;
            foreach (var movePosition in nonSuicides.SetBits())
                moves[i++] = new Move(movePosition);

            return moves;
        }

        private Bitboard GetCapturedByMove(Bitboard movePosition)
        {
            Bitboard captures = new Bitboard();
            Bitboard checkedMask = new Bitboard();
            Bitboard emptyAfterMove = _empty - movePosition;
            foreach (var adjecentOpponent in movePosition.GetNeighbours(_oBoard).SetBits())
            {
                if (!(adjecentOpponent & checkedMask).IsEmpty())
                    continue; // the capture nature of this stone is already determined

                Bitboard floodfill = adjecentOpponent.Floodfill(_oBoard);
                if (floodfill.Expand(emptyAfterMove).IsEmpty()) // no liberties
                    captures |= floodfill;
                checkedMask |= floodfill;
            }
            return captures;
        }

        private Bitboard ProveConnectedToOpenLiberties(ref Bitboard movesToProve)
        {
            Bitboard proven = new Bitboard();
            Bitboard hasAccessToLiberty = _empty.GetNeighbours(_empty);
            while (true)
            {
                proven = proven | movesToProve & hasAccessToLiberty;
                movesToProve = movesToProve - proven;
                if (movesToProve.IsEmpty())
                    break;

                if (!hasAccessToLiberty.ExpandInplace(movesToProve | _pBoard))
                    break; // cannot prove any more moves
            }
            return proven;
        }

        private Bitboard ProveConnectedToClosedLiberties(ref Bitboard movesToProve)
        {
            Bitboard proven = new Bitboard();
            foreach (var move in movesToProve.SetBits())
            {
                if (ConnectsToLiberty(move, _pBoard | move, _empty - move))
                {
                    proven |= move;
                    movesToProve = movesToProve - move;
                }
            }
            return proven;
        }

        private Bitboard ProveCapturesWithNoLiberties(ref Bitboard movesToProve)
        {
            Bitboard proven = new Bitboard();
            foreach (var move in movesToProve.SetBits())
            {
                foreach (var adjecentOpponent in move.GetNeighbours(_oBoard).SetBits())
                {
                    if (!ConnectsToLiberty(adjecentOpponent, _oBoard, _empty - move))
                    {
                        proven |= move;
                        movesToProve = movesToProve - move;
                        break;
                    }
                }
            }
            return proven;
        }

        private bool ConnectsToLiberty(Bitboard position, Bitboard friendly, Bitboard empty)
        {
            Bitboard floodfill = position;
            Bitboard open = friendly | empty;
            while (floodfill.ExpandInplace(open))
            {
                if (!(floodfill & empty).IsEmpty())
                    return true;
            }
            return false;
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
                _black |= move.Position;
                _white = _white - move.Captures;
                _flags |= Flags.SideToPlay;
            }
            else
            {
                _white |= move.Position;
                _black = _black - move.Captures;
                _flags = _flags & ~Flags.SideToPlay;
            }
            _empty = ~_black - _white;
        }

        public float Evaluate()
        {
            return _black.PopCount() > _white.PopCount() ? 1.0f : -1.0f;
        }

        public override string ToString()
        {
            string columnIndicators = "--A-B-C-D-E-F-G-H-I--";
            string output = "";
            var moves = GetMoves();

            output += columnIndicators + '\n';
            for (int i = 80; i >= 0; i--)
            {
                if (i % 9 == 8)
                    output += $"{i / 9 + 1} ";
                if (_black.IsBitSet(i))
                    output += "X ";
                else if (_white.IsBitSet(i))
                    output += "O ";
                else if (moves.ToList().FindAll(m => m.Position.IsBitSet(i) && !m.Captures.IsEmpty()).Count != 0)
                    output += "* ";
                else if (moves.ToList().FindAll(m => m.Position.IsBitSet(i)).Count != 0)
                    output += "+ ";
                else
                    output += ". ";
                if (i % 9 == 0)
                    output += $"{i / 9 + 1} \n";
            }
            output += columnIndicators;

            return output;
        }

        public void Display()
        {
            var bgcmap = new Dictionary<char, ConsoleColor>()
            {
                { 'X', ConsoleColor.Black },
                { 'O', ConsoleColor.White },
                { '+', ConsoleColor.Green },
                { '.', ConsoleColor.DarkGray },
                { '*', ConsoleColor.Red },
            };
            var fgcmap = new Dictionary<char, ConsoleColor>()
            {
                { 'X', ConsoleColor.White },
                { 'O', ConsoleColor.Black },
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
            string s = "........." +
                       "........." +
                       "........." +
                       "........." +
                       "........." +
                       "........." +
                       "........." +
                       "........." +
                       ".........";
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
            return board;
        }
    }
}
