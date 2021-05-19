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
            int numOfElements = 1;//Для вывбора фигуры по номеру из списка фигур
            int numOfElementsInLine = 1;//для отображения фигур по 8 в строке

            uint chosenPiece = 0;//Считывает пользовательский ввод для выбора фигуры из предложенного списка
            uint chosenMove = 0;//Считывает пользовательский ввод для выбора доступного хода из предложенного списка

            Console.WriteLine();
            Console.WriteLine("Выберите фигуру");

            //Выводит список доступных фигур по 8 штук в строку
            foreach (var piece in currentPlayer.MyPieces)
            {
                if (numOfElementsInLine > 8)
                {
                    Console.WriteLine();
                    numOfElementsInLine = 1;
                }
                Console.Write($"{numOfElements}.  {piece}" + "\t");
                numOfElements++;
                numOfElementsInLine++;
            }

            chosenPiece = UserInput(currentPlayer.MyPieces.Count);

            int counter = 1;//счетчик

            var AvailableMoves = currentPlayer.MyPieces[(int)(chosenPiece - 1)].AvailableMoves(GameField);//список возможных ходов (список (int,int)-координата клетки)

            //выводим доступные ходы или сообщение о том, что их нет
            if (AvailableMoves.Count == 0)
            {
                Console.WriteLine("доступных ходов нет");
            }
            else
            {
                Console.WriteLine("Выберите ход");
                foreach (var p in AvailableMoves)
                {
                    Console.WriteLine(counter + " " + alphabet[p.Item1] + $"{p.Item2 + 1}");
                    counter++;
                }
            }

           
            var availableKills = currentPlayer.MyPieces[(int)(chosenPiece - 1)].AvailableKills(GameField);//спиксок фигур, которые можно съесть
            //выводим список фигур для атаки или сообщение, что таковых нет
            if (availableKills.Count == 0)
            {
                Console.WriteLine("Съесть никого нельзя");
               
            }
            else
            {
                AvailableMoves.AddRange(availableKills);//список возможных ходов и убийств фигур противника (список (int,int)-координата клетки)
                Console.WriteLine("можно съесть:");
                foreach (var piece in availableKills)
                {
                    Console.WriteLine(counter + " " + alphabet[piece.Item1] + $"{piece.Item2 + 1}");
                    counter++;
                }

            }
           
            

            if (AvailableMoves.Count == 0)
            {
                Console.WriteLine("Для выбранной фигуры доступных ходов нет!\n" +
                    "Выберите другую фигуру");
                Move(currentPlayer, GameField, Pieces);
                return;
            }
            else
            {
                chosenMove = UserInput(AvailableMoves.Count);//переменная служит для выбора хода
            }

           

            //Проверка не является ли желаемый ход попыткой съесть фигуру (если среди фигур есть та, которая уже находиться на позиции, на которую текущий игрок собирается пойти, то текущий игрок съедает эту фигуру)
            if (Pieces.Find(x => x.Position == AvailableMoves[(int)(chosenMove - 1)]) != null)
            {
                Pieces.Find(x => x.Position == AvailableMoves[(int)(chosenMove - 1)]).IsDead = true;
            }


            currentPlayer.MyPieces[(int)(chosenPiece - 1)].Position = AvailableMoves[(int)(chosenMove - 1)];

        }
        /// <summary>
        /// Пользовательский ввод
        /// </summary>
        /// <param name="numberOfelements"></param>
        /// <returns></returns>
        private static uint UserInput( int numberOfelements)
        {
            uint chosenElement;
            while (!uint.TryParse(Console.ReadLine(), out chosenElement) || !(chosenElement <= numberOfelements))
            {
                Console.WriteLine("Неверный ввод!\n" +
                    "Повторите попытку");
            }

            return chosenElement;
        }

        /// <summary>
        /// Создает шахматные фигуры и устанавливает начальные позиции
        /// </summary>
        /// <returns>Возвращает список шахматных фигур</returns>
        public static List<IPiece> GetPieces()
        {
            var Piece = new List<IPiece>();
            //Создаем пешки
            for (int i = 0; i < 8; i++)
            {
                var wPawn = new Pawn(PieceColor.White, (i, 1));
                var bPawn = new Pawn(PieceColor.Black, (i, 6));
                Piece.Add(wPawn);
                Piece.Add(bPawn);
            }
            //создаем слонов
            Piece.Add(new Bishop((2, 0), PieceColor.White));
            Piece.Add(new Bishop((5, 0), PieceColor.White));
            Piece.Add(new Bishop((2, 7), PieceColor.Black));
            Piece.Add(new Bishop((5, 7), PieceColor.Black));

            //создаем ладьи
            Piece.Add(new Rook((0, 0), PieceColor.White));
            Piece.Add(new Rook((7, 0), PieceColor.White));
            Piece.Add(new Rook((0, 7), PieceColor.Black));
            Piece.Add(new Rook((7, 7), PieceColor.Black));

            //создаем коней
            Piece.Add(new Knight((1, 0), PieceColor.White));
            Piece.Add(new Knight((6, 0), PieceColor.White));
            Piece.Add(new Knight((1, 7), PieceColor.Black));
            Piece.Add(new Knight((6, 7), PieceColor.Black));

            //создаем ферзей
            Piece.Add(new Queen(PieceColor.Black, (3, 7)));
            Piece.Add(new Queen(PieceColor.White, (3, 0)));

            //создаем королей
            Piece.Add(new King((4, 0), PieceColor.White));
            Piece.Add(new King((4, 7), PieceColor.Black));

            return Piece;
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
        /// <summary>
        /// Удаляет с поля убитые фигуры
        /// </summary>
        /// <param name="pieces"></param>
        public static void Update(List<IPiece> pieces)
        {
            pieces.RemoveAll(x => x.IsDead == true);
        }

        /// <summary>
        /// Визуализация доски, в зависимости от текущего игрока доска поворачивается к текущему игроку
        /// </summary>
        /// <param name="gamefield">Игровая доска</param>
        /// <param name="CurrentPlayer">Текущий игрок (1 - ход белых; -1 - ход черных)</param>
        public static void Visualize(string[,] gamefield, int CurrentPlayer)
        {
            if (CurrentPlayer == 1)
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
                            // номер поля по вертикали
                            Console.Write(9 - j + " ");
                        }
                        else if (j == 0 && i > 0)
                        {
                            // буква поля по горизонтали
                            Console.Write(alphabet[i - 1].ToString().ToUpper() + " ");
                        }
                        else
                        {
                            if ((i + j) % 2 == 0)
                            {
                                //Белая клетка и белым цветом красим обозначение фигуры
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.ForegroundColor = ConsoleColor.Black;
                            }
                            else
                            {
                                //Черная клетка и белым цветом красим обозначение фигуры
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
                            Console.Write(alphabet[7 - i + 1].ToString().ToUpper() + " ");
                        }
                        else
                        {
                            if ((i + j) % 2 == 0)
                            {
                                //Белая клетка и белым цветом красим обозначение фигуры
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.ForegroundColor = ConsoleColor.Black;
                            }
                            else
                            {
                                //Черная клетка и белым цветом красим обозначение фигуры
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

        /// <summary>
        /// Название клеток по горизонтали
        /// </summary>
        static string alphabet = "abcdefgh";
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
        /// <summary>
        /// Текущий игрок
        /// </summary>
        static int CurrentPlayer;
        /// <summary>
        /// Процесс игры
        /// </summary>
        static void Game()
        {
            Console.Clear();

            //получаем фигуры на доске, у каждой фигуры записаны текущее местоположение на доске
            GameField = GetGameField(Pieces);

            //отрисовываем доску
            Visualize(GameField, CurrentPlayer);
            Console.ResetColor();

            if (CurrentPlayer == 1)
            {

                //ход белых
                Move(player1, GameField, Pieces);

                Update(Pieces);
                Console.Clear();
                GameField = GetGameField(Pieces);
                Visualize(GameField, CurrentPlayer);
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
                Visualize(GameField, CurrentPlayer);
                Console.WriteLine("Любую клавишу для продолжения...");
                Console.ReadLine();
                //меняем текущего игрока
                CurrentPlayer *= -1;

            }
        }
        /// <summary>
        /// Начинает новую игру
        /// </summary>
        static void CreateNewGame()
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

            CreateNewGame();


            Console.ReadLine();
        }
    }
}
