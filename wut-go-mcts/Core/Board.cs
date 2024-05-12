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

        private Bitboard _empty => ~_black & ~_white;
        private Bitboard _full => _black | _white;
        private bool _blackToPlay => (_flags & Flags.SideToPlay) == 0;
        private Bitboard _pBoard => _blackToPlay ? _black : _white;
        private Bitboard _oBoard => _blackToPlay ? _white : _black;
        private Bitboard _oldPBoard => _blackToPlay ? _oldBlack : _oldWhite;
        private Bitboard _oldOBoard => _blackToPlay ? _oldWhite : _oldBlack;

        public bool Pass => (_flags & Flags.Pass) > 0;
        public bool Finished => (_flags & Flags.Finished) > 0;


        public Board()
        {
            _black = new Bitboard();
            _white = new Bitboard();
            _oldBlack = new Bitboard();
            _oldWhite = new Bitboard();
            _flags = 0;
        }

        public Board(Bitboard black, Bitboard white)
        {
            _black = black;
            _white = white;
        }

        public List<Move> GetMoves()
        {
            Bitboard movesToProve = _empty;
            Bitboard nonSuicides = new Bitboard();

            nonSuicides |= ProveConnectedToOpenLiberties(ref movesToProve);
            nonSuicides |= ProveConnectedToClosedLiberties(ref movesToProve);
            nonSuicides |= ProveCapturesWithNoLiberties(ref movesToProve);

            List<Move> moves = new List<Move>();

            // positions where there was own stone in the previous position but where we can also place a stone now
            Bitboard koDiff = _oldPBoard & nonSuicides;
            if (koDiff.PopCount() == 1)
            {
                Bitboard captures = GetCapturedByMove(koDiff);

                // if the position is not actually repeated
                if ((_oBoard & ~captures) != _oldOBoard)
                    moves.Add(new Move(koDiff, captures));

                // either way, remove the move
                nonSuicides = nonSuicides & ~koDiff;
            }

            foreach (var movePosition in nonSuicides.SetBits())
                moves.Add(new Move(movePosition, GetCapturedByMove(movePosition)));

            return moves;
        }

        private Bitboard GetCapturedByMove(Bitboard movePosition)
        {
            Bitboard captures = new Bitboard();
            Bitboard checkedMask = new Bitboard();
            Bitboard emptyAfterMove = _empty & ~movePosition;
            foreach (var adjecentOpponent in (movePosition.GetNeighbours() & _oBoard).SetBits())
            {
                if (!(adjecentOpponent & checkedMask).IsEmpty())
                    continue; // the capture nature of this stone is already determined

                Bitboard floodfill = Floodfill(adjecentOpponent, _oBoard);
                if ((floodfill.Expand() & emptyAfterMove).IsEmpty()) // no liberties
                    captures |= floodfill;
                checkedMask |= floodfill;
            }
            return captures;
        }

        private Bitboard ProveConnectedToOpenLiberties(ref Bitboard movesToProve)
        {
            Bitboard proven = new Bitboard();
            Bitboard hasAccessToLiberty = _empty.GetNeighbours() & _empty;
            while (true)
            {
                proven = proven | movesToProve & hasAccessToLiberty;
                movesToProve = movesToProve & ~proven;
                if (movesToProve.IsEmpty())
                    break;

                Bitboard newHasAccessToLibery = hasAccessToLiberty.Expand() & (movesToProve | _pBoard);
                if ((newHasAccessToLibery ^ hasAccessToLiberty).IsEmpty())
                    break; // cannot prove any more moves
                hasAccessToLiberty = newHasAccessToLibery;
            }
            return proven;
        }

        private Bitboard ProveConnectedToClosedLiberties(ref Bitboard movesToProve)
        {
            Bitboard proven = new Bitboard();
            foreach (var move in movesToProve.SetBits())
            {
                if (ConnectsToLiberty(move, _pBoard | move, _empty & ~move))
                {
                    proven |= move;
                    movesToProve = movesToProve & ~move;
                }
            }
            return proven;
        }

        private Bitboard ProveCapturesWithNoLiberties(ref Bitboard movesToProve)
        {
            Bitboard proven = new Bitboard();
            foreach (var move in movesToProve.SetBits())
            {
                foreach (var adjecentOpponent in (move.GetNeighbours() & _oBoard).SetBits())
                {
                    if (!ConnectsToLiberty(adjecentOpponent, _oBoard, _empty & ~move))
                    {
                        proven |= move;
                        movesToProve = movesToProve & ~move;
                    }
                }
            }
            return proven;
        }

        private bool ConnectsToLiberty(Bitboard position, Bitboard friendly, Bitboard empty)
        {
            Bitboard floodfill = new Bitboard(position);
            bool emptyIntersectionFound = false;
            while (true)
            {
                Bitboard newFloodfill = floodfill.Expand() & (friendly | empty);
                if ((newFloodfill ^ floodfill).IsEmpty())
                    break;
                if (!(newFloodfill & empty).IsEmpty())
                {
                    emptyIntersectionFound = true;
                    break;
                }
                floodfill = newFloodfill;
            }
            return emptyIntersectionFound;
        }

        private Bitboard Floodfill(Bitboard b, Bitboard open)
        {
            Bitboard floodfill = new Bitboard(b);
            while (true)
            {
                Bitboard newFloodfill = floodfill.Expand() & open;
                if ((newFloodfill ^ floodfill).IsEmpty())
                    return floodfill;
                floodfill = newFloodfill;
            }
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

            if ((_flags & Flags.SideToPlay) == 0)
            {
                _black |= move.Position;
                _white = _white & ~move.Captures;
                _flags |= Flags.SideToPlay;
            }
            else
            {
                _white |= move.Position;
                _black = _black & ~move.Captures;
                _flags = _flags & ~Flags.SideToPlay;
            }
        }

        public float Evaluate()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string output = "";
            for (int i = 80; i >= 0; i--)
            {
                if (_black.IsBitSet(i))
                    output += 'X';
                else if (_white.IsBitSet(i))
                    output += 'O';
                else
                    output += '.';
                if (i % 9 == 0)
                    output += '\n';
            }
            return output;
        }

        public static Board GetFromString()
        {
            string s = "......XO." +
                       "...OOOXOO" +
                       "...O.OXXX" +
                       "OOOOXO..." +
                       "O.OOXOOOO" +
                       "OXO...OXO" +
                       "OXOOOOOXO" +
                       "O.OO.OO.O" +
                       "OOOOOOOOO";
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
