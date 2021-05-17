using System;
using System.Collections.Generic;
using System.Linq;
using ChessLib;
namespace chess
{


    class Program
    {
        /// <summary>
        /// метод реализует ход текущего игрока:
        /// Предлагает выбрать фигуру, затем предлагает выбрать возможный ход
        /// </summary>
        /// <param name="currentPlayer">Текущий игрок</param>
        /// <param name="GameField">Игровое поле</param>
        /// <param name="Pieces">Фигуры</param>
        public static void Move(Player currentPlayer, string[,] GameField, List<IPiece> Pieces)
        {
            int i = 1;
            Console.WriteLine("Выберите фигуру");
            foreach (var p in currentPlayer.MyPieces)
            {
                Console.WriteLine(i + " " + p.ToString());
                i++;
            }
            int num = int.Parse(Console.ReadLine());
            int counter = 1;
            Console.WriteLine("Выберите ход");
            var avalbleMaves = currentPlayer.MyPieces[num - 1].AvalableMoves(GameField);
            foreach (var p in avalbleMaves)
            {
                Console.WriteLine(counter + " " + alp[p.Item1] + $"{p.Item2 + 1}");
                counter++;
            }
            var kills = currentPlayer.MyPieces[num - 1].AvalableKills(GameField);
            foreach (var p in kills)
            {
                Console.WriteLine(counter + " " + alp[p.Item1] + $"{p.Item2 + 1}");
                counter++;
            }
            int num1 = int.Parse(Console.ReadLine());

            var moves = currentPlayer.MyPieces[num - 1].AvalableMoves(GameField);
            moves.AddRange(currentPlayer.MyPieces[num - 1].AvalableKills(GameField));

            if(Pieces.Find(x => x.Position == moves[num1 - 1])!= null)
            {
                Pieces.Find(x => x.Position == moves[num1 - 1]).Dead = true;
            }


            currentPlayer.MyPieces[num - 1].Position = moves[num1 - 1];




        }
        /// <summary>
        /// Создает шахматные фигуры и устанавливает начальные позиции
        /// </summary>
        /// <returns>Возвращает список шахматных фигур</returns>
        public static List<IPiece> GetPieces()
        {
            var result = new List<IPiece>();
            //Создаем пешки
            for (int i = 0; i < 8; i++)
            {
                var wPawn = new Pawn(PieceColor.White, (i, 1));
                var bPawn = new Pawn(PieceColor.Black, (i, 6));
                result.Add(wPawn);
                result.Add(bPawn);
            }
            //создаем слонов
            result.Add(new Bishop((2, 0), PieceColor.White));
            result.Add(new Bishop((5, 0), PieceColor.White));
            result.Add(new Bishop((2, 7), PieceColor.Black));
            result.Add(new Bishop((5, 7), PieceColor.Black));

            //создаем ладьи
            result.Add(new Rook((0, 0), PieceColor.White));
            result.Add(new Rook((7, 0), PieceColor.White));
            result.Add(new Rook((0, 7), PieceColor.Black));
            result.Add(new Rook((7, 7), PieceColor.Black));

            //создаем коней
            result.Add(new Knight((1, 0), PieceColor.White));
            result.Add(new Knight((6, 0), PieceColor.White));
            result.Add(new Knight((1, 7), PieceColor.Black));
            result.Add(new Knight((6, 7), PieceColor.Black));

            //создаем ферзей
            result.Add(new Queen(PieceColor.Black, (3, 7)));
            result.Add(new Queen(PieceColor.White, (3, 0)));

            //создаем королей
            result.Add(new king((4, 0), PieceColor.White));
            result.Add(new king((4, 7), PieceColor.Black));

            return result;
        }
        /// <summary>
        /// Игровое поле
        /// </summary>
        /// <param name="pieces"></param>
        /// <returns>Возвращает двумерный массив строк, содержащий обозначения каждой клетки игрового поля</returns>
        public static string[,] GetGameField(List<IPiece> pieces)
        {
            string[,] GameField = new string[8, 8];
            foreach (var piece in pieces)
            {
                GameField[piece.Position.Item1, piece.Position.Item2] = piece.Color == PieceColor.White ? piece.ToString().ToUpper() : piece.ToString();
            }
            return GameField;
        }

        public static void Update(List<IPiece> pieces)
        {
            pieces.RemoveAll(x => x.Dead == true);
        }
        /// <summary>
        /// Визуализация игрового поля
        /// </summary>
        /// <param name="gameField"></param>
        public static void Render(string[,] gameField, int curPlayer)
        {
            Console.Clear();
            //если текущий ход белых, то отрисовываем доску снизу вверх
            if (curPlayer == -1)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (gameField[i, j] == null)
                        {
                            Console.Write(" ");
                        }
                        else
                        {
                            Console.Write(gameField[i, j]);
                        }


                    }
                    Console.WriteLine();
                }
            }
            //если текущий ход черных, то отрисовываем доску сверху вниз
            else
            {
                for (int j = 7; j > -1; j--)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (gameField[i, j] == null)
                        {
                            Console.Write(" ");
                        }
                        else
                        {
                            Console.Write(gameField[i, j]);
                        }

                    }
                    Console.WriteLine();
                }
            }

        }
        static string alp = "abcdefgh";
        /// <summary>
        /// Игрок за белых
        /// </summary>
        static Player player1;
        /// <summary>
        /// Игрок за черных
        /// </summary>
        static Player player2;
        /// <summary>
        /// Игровое поле
        /// </summary>
        static string[,] GameField;
        /// <summary>
        /// Шахматные фигуры
        /// </summary>
        static List<IPiece> Pieces;
        static void Main(string[] args)
        {
            //Создаем шахматные фигуры и устанавливаем первоначальные позиции
            Pieces = GetPieces();

            //переменная служит для очереди игроков
            int CurrentPlayer = 1;

            //Игрок с белыми фигурами
            player1 = new Player(PieceColor.White, Pieces.Where(x => x.Color == PieceColor.White).ToList(), "user1");//CurrentPlayer = 1

            //Игрок с черными фигурами
            player2 = new Player(PieceColor.Black, Pieces.Where(x => x.Color == PieceColor.Black).ToList(), "user2");//CurrentPlayer = -1

            bool isGameOver = false;

            while (!isGameOver)
            {
                //получаем фигуры на доске, у каждой фигуры записаны текущее местоположение на доске
                GameField = GetGameField(Pieces);

                //отрисовываем доску
                Render(GameField, CurrentPlayer);

                if (CurrentPlayer == 1)
                {

                    //ход белых
                    Move(player1, GameField, Pieces);

                    Update(Pieces);

                    //меняем текущего игрока
                    CurrentPlayer *= -1;

                }
                else
                {
                    //ход черных
                    Move(player2, GameField, Pieces);
                    Update(Pieces);
                    //меняем текущего игрока
                    CurrentPlayer *= -1;

                }


            }



            Console.ReadLine();
        }
    }
}
