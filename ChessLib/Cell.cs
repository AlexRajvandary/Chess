using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLib
{
    public class Cell
    {
        private IPiece piece;
        public bool isAtacked { get; set; }

        public bool isFilled { get; set; }

        public IPiece Piece
        {
            get => piece;
            set
            {
                if (isFilled)
                {
                    piece = value;
                }
                else
                {
                    piece = null;
                }
            }
        }

        public Cell()
        {
            isAtacked = false;
            isFilled = false;
        }
    }
}
