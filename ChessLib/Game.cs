using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLib
{
    public class Game
    {
        string alphabet = "abcdefgh";
        IView view;
        string[,] GameField;
        int CurrentPlayer;
        List<IPiece> Pieces;

        public Player player1;

        public Player player2;
        

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
         List<IPiece> GetPieces()
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
        public void CreateNewGame()
        {
            Pieces = new List<IPiece>();
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

                GameProcess();

            }
        }
        string[,] GetGameField(List<IPiece> pieces)
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

         void Move(Player currentPlayer, string[,] GameField, List<IPiece> Pieces)
        {
            int numOfElements = 1;//Для вывбора фигуры по номеру из списка фигур
            int numOfElementsInLine = 1;//для отображения фигур по 8 в строке

            uint chosenPiece = 0;//Считывает пользовательский ввод для выбора фигуры из предложенного списка
            uint chosenMove = 0;//Считывает пользовательский ввод для выбора доступного хода из предложенного списка


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

            int counter = 1;//счетчик

            var AvailableMoves = currentPlayer.MyPieces[(int)(chosenPiece - 1)].AvailableMoves(GameField);//список возможных ходов (список (int,int)-координата клетки)

            //выводим доступные ходы или сообщение о том, что их нет
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


            var availableKills = currentPlayer.MyPieces[(int)(chosenPiece - 1)].AvailableKills(GameField);//спиксок фигур, которые можно съесть
            //выводим список фигур для атаки или сообщение, что таковых нет
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



            if (AvailableMoves.Count == 0)
            {
                view.Show("Для выбранной фигуры доступных ходов нет!\n" +
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

         void Update(List<IPiece> pieces)
        {
            pieces.RemoveAll(x => x.IsDead == true);
        }

       public void GameProcess()
        {
            Console.Clear();

            //получаем фигуры на доске, у каждой фигуры записаны текущее местоположение на доске
            GameField = GetGameField(Pieces);

            //отрисовываем доску
            view.Visualize(GameField, CurrentPlayer);
            Console.ResetColor();

            if (CurrentPlayer == 1)
            {

                //ход белых
                Move(player1, GameField, Pieces);

                Update(Pieces);
                Console.Clear();
                GameField = GetGameField(Pieces);
                view.Visualize(GameField, CurrentPlayer);
                view.Show("Любую клавишу для продолжения...");
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
                view.Visualize(GameField, CurrentPlayer);
                view.Show("Любую клавишу для продолжения...");
                Console.ReadLine();
                //меняем текущего игрока
                CurrentPlayer *= -1;

            }
        }

        public Game(IView view)
        {
          

            this.view = view;
        }
    }
}
