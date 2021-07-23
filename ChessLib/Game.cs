using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib
{
    public class Game
    {
        /// <summary>
        /// Для подписи клеток
        /// </summary>
        string[] alphabet = new string[] { "a", "b", "c", "d", "e", "f", "g", "h" };
        /// <summary>
        /// Для визуализации
        /// </summary>
        IView view;
        public GameField GameField { get; set; }

        /// <summary>
        /// Игровое поле
        /// </summary>
        string[,] GameFieldString;
        /// <summary>
        /// Текущий игрок
        /// </summary>
        public int CurrentPlayer;
        /// <summary>
        /// Фигуры
        /// </summary>
        public List<IPiece> Pieces;

        public List<Player> players;

        public bool isGameOver;

        /// <summary>
        /// Пользовательский ввод
        /// </summary>
        /// <param name="numberOfelements"></param>
        /// <returns></returns>
        uint UserInput(int numberOfelements)
        {
            uint chosenElement;
            while (!uint.TryParse(Console.ReadLine(), out chosenElement) || !(chosenElement <= numberOfelements))
            {
                view.Show("Неверный ввод!\n" +
                    "Повторите попытку");
            }

            return chosenElement;
        }

        /// <summary>
        /// Устанавливает начальные позиции фигурам
        /// </summary>
        /// <returns>Список фигур</returns>
        public List<IPiece> GetPiecesStartPosition()
        {
            var Pieces = new List<IPiece>();
            //Создаем пешки
            for (int i = 0; i < 8; i++)
            {
                var wPawn = new Pawn(PieceColor.White, (i, 1));
                var bPawn = new Pawn(PieceColor.Black, (i, 6));
                Pieces.Add(wPawn);
                Pieces.Add(bPawn);
            }
            //создаем слонов
            Pieces.Add(new Bishop((2, 0), PieceColor.White));
            Pieces.Add(new Bishop((5, 0), PieceColor.White));
            Pieces.Add(new Bishop((2, 7), PieceColor.Black));
            Pieces.Add(new Bishop((5, 7), PieceColor.Black));

            //создаем ладьи
            Pieces.Add(new Rook((0, 0), PieceColor.White));
            Pieces.Add(new Rook((7, 0), PieceColor.White));
            Pieces.Add(new Rook((0, 7), PieceColor.Black));
            Pieces.Add(new Rook((7, 7), PieceColor.Black));

            //создаем коней
            Pieces.Add(new Knight((1, 0), PieceColor.White));
            Pieces.Add(new Knight((6, 0), PieceColor.White));
            Pieces.Add(new Knight((1, 7), PieceColor.Black));
            Pieces.Add(new Knight((6, 7), PieceColor.Black));

            //создаем ферзей
            Pieces.Add(new Queen(PieceColor.Black, (3, 7)));
            Pieces.Add(new Queen(PieceColor.White, (3, 0)));

            //создаем королей
            Pieces.Add(new King((4, 0), PieceColor.White));
            Pieces.Add(new King((4, 7), PieceColor.Black));

            return Pieces;
        }

        /// <summary>
        /// Игровой процесс, пока isGameOver - false, новый ход
        /// </summary>
        public void CreateNewGame()
        {

            while (!isGameOver)
            {
                GameProcess();
                if (CurrentPlayer > 2) CurrentPlayer -= 2;
            }
            Console.WriteLine("Конец игры");
        }

        /// <summary>
        /// Получаем строковое представление игровой доски из позиций фигур
        /// </summary>
        /// <param name="pieces">Список фигур</param>
        /// <returns>Строковое представление игровой доски</returns>
        public string[,] GetGameField(List<IPiece> pieces)
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
        /// Выбор фигуры и выбор хода
        /// </summary>
        /// <param name="currentPlayer">Текущий игрок</param>
        /// <param name="GameField">Строковое представление игрового поля</param>
        /// <param name="Pieces">Список фигур</param>
        void ConsoleMove(Player currentPlayer, string[,] GameField, List<IPiece> Pieces)
        {
            this.GameField.Update(Pieces, GameField, currentPlayer.Color);

            ///Если королю стоит шах, то нужно убрать короляв безопасное место
            if (this.GameField.IsCheck())
            {
                view.Show("У вас шах!");

                MoveAfterCheck(currentPlayer, GameField, Pieces);


                return;
            }

            int numOfElements = 1;//Для вывбора фигуры по номеру из списка фигур
            int numOfElementsInLine = 1;//для отображения фигур по 8 в строке

            uint chosenPiece = 0;//Считывает пользовательский ввод для выбора фигуры из предложенного списка
            uint chosenMove = 0;//Считывает пользовательский ввод для выбора доступного хода из предложенного списка


            chosenPiece = ChosePiece(currentPlayer, ref numOfElements, ref numOfElementsInLine);

            int counter = 1;//счетчик

            var AvailableMoves = currentPlayer.MyPieces[(int)(chosenPiece - 1)].AvailableMoves(GameField);//список возможных ходов (список (int,int)-координата клетки)

            //выводим доступные ходы или сообщение о том, что их нет
            counter = ShowAvailableMoves(counter, AvailableMoves);

            var availableAttacks = currentPlayer.MyPieces[(int)(chosenPiece - 1)].AvailableKills(GameField);//спиксок фигур, которые можно съесть
                                                                                                            //выводим список фигур для атаки или сообщение, что таковых нет
            counter = ShowAvailableAttacks(counter, AvailableMoves, availableAttacks);

            if (AvailableMoves.Count == 0)
            {
                view.Show("Для выбранной фигуры доступных ходов нет!\n" +
                    "Выберите другую фигуру");
                ConsoleMove(currentPlayer, GameField, Pieces);
                return;
            }
            else
            {
                chosenMove = UserInput(AvailableMoves.Count);//переменная служит для выбора хода
            }

            CheckIfPieceWasKilled(Pieces, chosenMove, AvailableMoves);

            //Устанавливает новую позицию выбранной фигуре
            ChangePositionOfCurrentPiece(currentPlayer, chosenPiece, chosenMove, AvailableMoves);

        }

        private int ShowAvailableAttacks(int counter, List<(int, int)> AvailableMoves, List<(int, int)> availableKills)
        {
            if (availableKills.Count == 0)
            {
                view.Show("Съесть никого нельзя\n");
            }
            else
            {
                AvailableMoves.AddRange(availableKills);//список возможных ходов и убийств фигур противника (список (int,int)-координата клетки)
                view.Show("можно съесть:\n");
                foreach (var piece in availableKills)
                {
                    view.Show(counter + " " + alphabet[piece.Item1] + $"{piece.Item2 + 1}");
                    counter++;
                }
            }
            return counter;
        }

        private int ShowAvailableMoves(int counter, List<(int, int)> AvailableMoves)
        {
            if (AvailableMoves.Count == 0)
            {
                view.Show("доступных ходов нет\n");
            }
            else
            {
                view.Show("Выберите ход\n");
                foreach (var p in AvailableMoves)
                {
                    view.Show($"{counter} {alphabet[p.Item1]} {p.Item2 + 1} \n");
                    counter++;
                }
            }
            return counter;
        }

        private static void ChangePositionOfCurrentPiece(Player currentPlayer, uint chosenPiece, uint chosenMove, List<(int, int)> AvailableMoves)
        {
            currentPlayer.MyPieces[(int)(chosenPiece - 1)].ChangePosition(AvailableMoves[(int)(chosenMove - 1)]);
        }

        private static void CheckIfPieceWasKilled(List<IPiece> Pieces, uint chosenMove, List<(int, int)> AvailableMoves)
        {
            //Проверка не является ли желаемый ход попыткой съесть фигуру (если среди фигур есть та, которая уже находиться на позиции, на которую текущий игрок собирается пойти, то текущий игрок съедает эту фигуру)
            if (Pieces.Find(x => x.Position == AvailableMoves[(int)(chosenMove - 1)]) != null)
            {
                Pieces.Find(x => x.Position == AvailableMoves[(int)(chosenMove - 1)]).IsDead = true;
            }
        }
        /// <summary>
        /// Метод для wpf-версии: проверяет, является ли текущей ход атакой на фигуру, если да, то присваеваем ей статус атакованной
        /// </summary>
        /// <param name="PiecePosition">Позиция выбранной фигуры</param>
        /// <param name="AttackedPosition">Ход</param>
        /// <param name="gameField">Игровое поле</param>
        /// <param name="pieces">Фигуры</param>
        public void CheckIfPieceWasKilled((int, int) PiecePosition, (int, int) AttackedPosition, string[,] gameField, List<IPiece> pieces)
        {
            //Проверка не является ли желаемый ход попыткой съесть фигуру (если среди фигур есть та, которая уже находиться на позиции, на которую текущий игрок собирается пойти, то текущий игрок съедает эту фигуру)
            if (gameField[AttackedPosition.Item1, AttackedPosition.Item2] != "")
            {
                pieces.Find(x => x.Position == AttackedPosition).IsDead = true;
                pieces.Find(x => x.Position == PiecePosition).Position = AttackedPosition;
            }
        }

        /// <summary>
        /// Выбор фигуры для хода
        /// </summary>
        /// <param name="currentPlayer"></param>
        /// <param name="numOfElements"></param>
        /// <param name="numOfElementsInLine"></param>
        /// <returns></returns>
        private uint ChosePiece(Player currentPlayer, ref int numOfElements, ref int numOfElementsInLine)
        {
            uint chosenPiece;
            view.Show("Выберите фигуру\n");

            //Выводит список доступных фигур по 8 штук в строку
            foreach (var piece in currentPlayer.MyPieces)
            {
                if (numOfElementsInLine > 8)
                {
                    view.Show("\n");
                    numOfElementsInLine = 1;
                }
                view.Show($"{numOfElements}.  {piece}" + "\t");
                numOfElements++;
                numOfElementsInLine++;
            }

            chosenPiece = UserInput(currentPlayer.MyPieces.Count);
            return chosenPiece;
        }

        /// <summary>
        /// Ход, когда поставили шах королю. Если ходов у короля нет, то мат. Метод реализует выбор доступного хода для короля, если доступные ходы есть.
        /// </summary>
        /// <param name="currentPlayer"></param>
        /// <param name="GameField"></param>
        /// <param name="Pieces"></param>
        void MoveAfterCheck(Player currentPlayer, string[,] GameField, List<IPiece> Pieces)
        {
            List<(int, int)> AvailableKingMoves = currentPlayer.MyPieces[currentPlayer.MyPieces.Count - 1].AvailableMoves(GameField);

            AvailableKingMoves.AddRange(currentPlayer.MyPieces[currentPlayer.MyPieces.Count - 1].AvailableKills(GameField));

            var ValidMoves = AvailableKingMoves?.Where(move => !this.GameField.GetAtackStatus(Pieces, move, GameField)).ToList();



            int counter = 1;
            if (ValidMoves.Count() != 0)
            {
                foreach (var moves in ValidMoves)
                {
                    view.Show($"{counter}. {"ABCDEFGH"[moves.Item1]} {moves.Item2 + 1}");
                    counter += 1;
                }

                int chosenMove = (int)UserInput(ValidMoves.Count());

                if (Pieces.Find(x => x.Position == ValidMoves[chosenMove - 1]) != null)
                {
                    Pieces.Find(x => x.Position == ValidMoves[chosenMove - 1]).IsDead = true;
                }

                isGameOver = false;

                currentPlayer.MyPieces[currentPlayer.MyPieces.Count - 1].Position = ValidMoves[chosenMove - 1];
            }
            else
            {
                Console.WriteLine("Шах и мат!");
                Console.ReadLine();

                isGameOver = true;
            }



        }
        /// <summary>
        /// Убираем убитые фигуры
        /// </summary>
        /// <param name="pieces"></param>
        public void RemoveDeadPieces(List<IPiece> pieces)
        {
            pieces.RemoveAll(x => x.IsDead == true);
        }
        /// <summary>
        /// Один ход (Отрисовка доски, изменение позицицй фигур, выбор хода и т.д.)
        /// </summary>
        void GameProcess()
        {
            //получаем фигуры на доске, у каждой фигуры записаны текущее местоположение на доске
            GameFieldString = GetGameField(Pieces);

            //отрисовываем доску
            view.Visualize(GameFieldString, CurrentPlayer);

            //ход 
            ConsoleMove(players[CurrentPlayer % 2], GameFieldString, Pieces);

            RemoveDeadPieces(Pieces);
            Console.Clear();
            GameFieldString = GetGameField(Pieces);
            view.Visualize(GameFieldString, CurrentPlayer);
            view.Show("Любую клавишу для продолжения...");
            Console.ReadLine();
            //меняем текущего игрока
            CurrentPlayer++;


        }

        public Game()
        {
            //хз

            //переменная служит для очереди игроков
            CurrentPlayer = 0;

            Pieces = new List<IPiece>();
            //Создаем шахматные фигуры и устанавливаем первоначальные позиции
            Pieces = GetPiecesStartPosition();

            GameField = new GameField();

            //Игрок с белыми фигурами
            Player player1 = new Player(PieceColor.White, Pieces.Where(x => x.Color == PieceColor.White).ToList(), "user1");

            //Игрок с черными фигурами
            Player player2 = new Player(PieceColor.Black, Pieces.Where(x => x.Color == PieceColor.Black).ToList(), "user2");
            players = new List<Player>
            {
                player1,
                player2
            };
        }

        public Game(IView view)
        {

            this.view = view;

            Pieces = new List<IPiece>();
            //Создаем шахматные фигуры и устанавливаем первоначальные позиции
            Pieces = GetPiecesStartPosition();

            GameField = new GameField();

            //переменная служит для очереди игроков
            CurrentPlayer = 0;

            //Игрок с белыми фигурами
            Player player1 = new Player(PieceColor.White, Pieces.Where(x => x.Color == PieceColor.White).ToList(), "user1");

            //Игрок с черными фигурами
            Player player2 = new Player(PieceColor.Black, Pieces.Where(x => x.Color == PieceColor.Black).ToList(), "user2");
            players = new List<Player>();
            players.Add(player1);
            players.Add(player2);

            isGameOver = false;
        }
    }
}
