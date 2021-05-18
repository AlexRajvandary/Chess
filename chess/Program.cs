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
            int j = 1;

            Console.WriteLine();
            Console.WriteLine("Выберите фигуру");
            foreach (var p in currentPlayer.MyPieces)
            {
                if (j > 8)
                {
                    Console.WriteLine();
                    j = 1;
                }
                Console.Write(i + " " + p.ToString() + "\t");
                i++;
                j++;
            }
            int num = int.Parse(Console.ReadLine());
            int counter = 1;//счетчик
            var moves = currentPlayer.MyPieces[num - 1].AvalableMoves(GameField);//список возможных ходов (список (int,int)-координата клетки)
            Console.WriteLine("Выберите ход");
            if(moves.Count == 0)
            {
                Console.WriteLine("доступных ходов нет");
            }
            else
            {
                foreach (var p in moves)
                {
                    Console.WriteLine(counter + " " + alp[p.Item1] + $"{p.Item2 + 1}");
                    counter++;
                }
            }
            int num1 = -1;
            Console.WriteLine("можно съесть:");
            var kills = currentPlayer.MyPieces[num - 1].AvalableKills(GameField);//спиксок фигур, которые можно съесть
            if(kills.Count == 0)
            {
                num1 = int.Parse(Console.ReadLine());//переменная служит для выбора какую фигуру съесть
                Console.WriteLine("Съесть никого нельзя");
            }
            else
            {
                foreach (var p in kills)
                {
                    Console.WriteLine(counter + " " + alp[p.Item1] + $"{p.Item2 + 1}");
                    counter++;
                }
                num1 = int.Parse(Console.ReadLine());//переменная служит для выбора какую фигуру съесть
            }


            moves.AddRange(currentPlayer.MyPieces[num - 1].AvalableKills(GameField));//список возможных ходов и убийств фигур противника (список (int,int)-координата клетки)




            if (Pieces.Find(x => x.Position == moves[num1 - 1]) != null)
            {
                Pieces.Find(x => x.Position == moves[num1 - 1]).Dead = true;
            }

            if (num1 > 0)
            {
                currentPlayer.MyPieces[num - 1].Position = moves[num1 - 1];
            }
          




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
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (GameField[i, j] == null)
                    {
                        GameField[i, j] = " ";
                    }
                }
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
        public static string[,] Render(string[,] gameField, int curPlayer)
        {

            string[,] result = new string[8, 8];
            //если текущий ход белых, то отрисовываем доску снизу вверх
           
                for (int j = 0; j < 8; j++)
                {

                    for (int i = 0; i < 8; i++)
                    {
                        result[i, j] = gameField[i, j];

                    }

                }

        

            ////если текущий ход черных, то отрисовываем доску сверху вниз
            //else
            //{
            //    for (int j = 7; j > -1; j--)
            //    {
            //        for (int i = 7; i > -1; i--)
            //        {

            //            result[i, j] = gameField[7 -i, 7 - j];



            //        }

            //    }

            //}

           
            return result;
        }

       public static void Visualize(string[,] gamefield, int CurrentPlayer)
        {
            if(CurrentPlayer == 1)
            {
                Console.WriteLine("Ход белых");
                Console.WriteLine();
                for (int j = 0; j < 9; j++)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i == 0 && j == 0)
                        {
                            Console.Write("  ");
                            continue;
                        }
                        else if (i == 0 && j > 0)
                        {
                            Console.Write(9 -j + " ");
                        }
                        else if (j == 0 && i > 0)
                        {
                            Console.Write(alp[i - 1].ToString().ToUpper() + " ");
                        }
                        else
                        {
                            if ((i + j) % 2 == 0)
                            {
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.ForegroundColor = ConsoleColor.Black;
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            Console.Write(gamefield[i - 1, 8 - j] + " ");
                            Console.ResetColor();
                        }
                    }
                    Console.WriteLine();
                }
          
            }
            else
            {
                Console.WriteLine("Ход черных");
                Console.WriteLine();
                for (int j = 0; j < 9; j++)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i == 0 && j == 0)
                        {
                            Console.Write("  ");
                            continue;
                        }
                        else if (i == 0 && j > 0)
                        {
                            Console.Write(j + " ");
                        }
                        else if (j == 0 && i > 0)
                        {
                            Console.Write(alp[7 - i + 1].ToString().ToUpper() + " ");
                        }
                        else
                        {
                            if ((i + j) % 2 == 0)
                            {
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.ForegroundColor = ConsoleColor.Black;
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            Console.Write(gamefield[8 - i, j - 1] + " ");
                            Console.ResetColor();
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
        static int CurrentPlayer;
        static void Game()
        {
            Console.Clear();

            //получаем фигуры на доске, у каждой фигуры записаны текущее местоположение на доске
            GameField = GetGameField(Pieces);

            //отрисовываем доску
            Visualize(Render(GameField, CurrentPlayer), CurrentPlayer);
            Console.ResetColor();

            if (CurrentPlayer == 1)
            {

                //ход белых
                Move(player1, GameField, Pieces);

                Update(Pieces);
                Console.Clear();
                GameField = GetGameField(Pieces);
                Visualize(Render(GameField, CurrentPlayer), CurrentPlayer);
                Console.WriteLine("Любую клавишу для продолжения...");
                Console.ReadLine();
                //меняем текущего игрока
                CurrentPlayer *= -1;

            }
            else
            {
                //ход черных
                Move(player2, GameField, Pieces);
                Update(Pieces);
                Console.Clear();
                GameField = GetGameField(Pieces);
                Visualize(Render(GameField, CurrentPlayer), CurrentPlayer);
                Console.WriteLine("Любую клавишу для продолжения...");
                Console.ReadLine();
                //меняем текущего игрока
                CurrentPlayer *= -1;

            }
        }
        static void NewGame()
        {
            //Создаем шахматные фигуры и устанавливаем первоначальные позиции
            Pieces = GetPieces();

            //переменная служит для очереди игроков
            CurrentPlayer = 1;

            //Игрок с белыми фигурами
            player1 = new Player(PieceColor.White, Pieces.Where(x => x.Color == PieceColor.White).ToList(), "user1");//CurrentPlayer = 1

            //Игрок с черными фигурами
            player2 = new Player(PieceColor.Black, Pieces.Where(x => x.Color == PieceColor.Black).ToList(), "user2");//CurrentPlayer = -1

            bool isGameOver = false;

            while (!isGameOver)
            {

                Game();

            }
        }
        static void Main(string[] args)
        {

            NewGame();


            Console.ReadLine();
        }
    }
}
