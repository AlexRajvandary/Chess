using ChessLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace chess
{
    /// <summary>
    /// Контроллер для управления консольной версией игры
    /// </summary>
    public class ConsoleGameController
    {
        private readonly Game game;
        private readonly IView view;
        private readonly string[] alphabet = new string[] { "a", "b", "c", "d", "e", "f", "g", "h" };

        public ConsoleGameController(Game game, IView view)
        {
            this.game = game;
            this.view = view;
        }

        /// <summary>
        /// Запускает консольную игру
        /// </summary>
        public void StartGame()
        {
            while (!game.IsGameOver)
            {
                ProcessTurn();
                if (game.CurrentPlayer > 2)
                {
                    game.CurrentPlayer -= 2;
                }
            }
            view.Show("Game Over\n");
        }

        /// <summary>
        /// Обрабатывает один ход
        /// </summary>
        private void ProcessTurn()
        {
            // Получаем строковое представление доски
            var gameFieldString = game.GetGameField(game.Pieces);

            // Отрисовываем доску
            view.Visualize(gameFieldString, game.CurrentPlayer);

            // Выполняем ход
            ExecuteMove(game.players[game.CurrentPlayer % 2], gameFieldString, game.Pieces);

            // Убираем убитые фигуры
            game.RemoveDeadPieces(game.Pieces);

            // Очищаем консоль и показываем результат
            Console.Clear();
            gameFieldString = game.GetGameField(game.Pieces);
            view.Visualize(gameFieldString, game.CurrentPlayer);
            view.Show("Press any key to continue...");
            Console.ReadLine();

            // Меняем текущего игрока
            game.CurrentPlayer++;
        }

        /// <summary>
        /// Выполняет ход игрока
        /// </summary>
        private void ExecuteMove(Player currentPlayer, string[,] gameField, List<IPiece> pieces)
        {
            game.GameField.Update(pieces, gameField, currentPlayer.Color);

            // Если королю стоит шах, обрабатываем специально
            if (game.GameField.IsCheck())
            {
                view.Show("Check!\n");
                HandleCheck(currentPlayer, gameField, pieces);
                return;
            }

            int numOfElements = 1;
            int numOfElementsInLine = 1;

            // Выбираем фигуру
            uint chosenPiece = ChoosePiece(currentPlayer, ref numOfElements, ref numOfElementsInLine);

            int counter = 1;

            // Получаем доступные ходы
            var availableMoves = currentPlayer.MyPieces[(int)(chosenPiece - 1)].AvailableMoves(gameField);
            counter = ShowAvailableMoves(counter, availableMoves);

            // Получаем доступные атаки
            var availableAttacks = currentPlayer.MyPieces[(int)(chosenPiece - 1)].AvailableKills(gameField);
            counter = ShowAvailableAttacks(counter, availableMoves, availableAttacks);

            if (availableMoves.Count == 0)
            {
                view.Show("No available moves for selected piece!\n" +
                    "Choose another piece\n");
                ExecuteMove(currentPlayer, gameField, pieces);
                return;
            }

            // Выбираем ход
            uint chosenMove = UserInput(availableMoves.Count);

            // Проверяем, была ли съедена фигура
            CheckIfPieceWasKilled(pieces, chosenMove, availableMoves);

            // Устанавливаем новую позицию выбранной фигуре
            currentPlayer.MyPieces[(int)(chosenPiece - 1)].ChangePosition(availableMoves[(int)(chosenMove - 1)]);
        }

        /// <summary>
        /// Обрабатывает ситуацию шаха
        /// </summary>
        private void HandleCheck(Player currentPlayer, string[,] gameField, List<IPiece> pieces)
        {
            var king = currentPlayer.MyPieces.LastOrDefault(p => p is King);
            if (king == null) return;

            var availableKingMoves = king.AvailableMoves(gameField);
            availableKingMoves.AddRange(king.AvailableKills(gameField));

            var validMoves = availableKingMoves?.Where(move => !game.GameField.GetAtackStatus(pieces, move, gameField)).ToList();

            int counter = 1;
            if (validMoves != null && validMoves.Count != 0)
            {
                foreach (var move in validMoves)
                {
                    view.Show($"{counter}. {"ABCDEFGH"[move.X]} {move.Y + 1}\n");
                    counter++;
                }

                int chosenMove = (int)UserInput(validMoves.Count);

                var pieceAtPosition = pieces.Find(x => x.Position == validMoves[chosenMove - 1]);
                if (pieceAtPosition != null)
                {
                    pieceAtPosition.IsDead = true;
                }

                game.IsGameOver = false;
                king.Position = validMoves[chosenMove - 1];
            }
            else
            {
                view.Show("Checkmate!\n");
                Console.ReadLine();
                game.IsGameOver = true;
            }
        }

        /// <summary>
        /// Пользовательский ввод
        /// </summary>
        private uint UserInput(int numberOfElements)
        {
            uint chosenElement;
            while (!uint.TryParse(Console.ReadLine(), out chosenElement) || chosenElement == 0 || chosenElement > numberOfElements)
            {
                view.Show("Invalid input!\n" +
                    "Try again\n");
            }
            return chosenElement;
        }

        /// <summary>
        /// Выбор фигуры для хода
        /// </summary>
        private uint ChoosePiece(Player currentPlayer, ref int numOfElements, ref int numOfElementsInLine)
        {
            view.Show("Choose a piece\n");

            // Выводит список доступных фигур по 8 штук в строку
            foreach (var piece in currentPlayer.MyPieces)
            {
                if (numOfElementsInLine > 8)
                {
                    view.Show("\n");
                    numOfElementsInLine = 1;
                }
                view.Show($"{numOfElements}.  {piece}\t");
                numOfElements++;
                numOfElementsInLine++;
            }
            view.Show("\n");

            return UserInput(currentPlayer.MyPieces.Count);
        }

        /// <summary>
        /// Shows available moves
        /// </summary>
        private int ShowAvailableMoves(int counter, List<Position> availableMoves)
        {
            if (availableMoves.Count == 0)
            {
                view.Show("No available moves\n");
            }
            else
            {
                view.Show("Choose a move\n");
                foreach (var p in availableMoves)
                {
                    view.Show($"{counter} {alphabet[p.X]} {p.Y + 1} \n");
                    counter++;
                }
            }
            return counter;
        }

        /// <summary>
        /// Shows available attacks
        /// </summary>
        private int ShowAvailableAttacks(int counter, List<Position> availableMoves, List<Position> availableKills)
        {
            if (availableKills.Count == 0)
            {
                view.Show("Cannot capture any piece\n");
            }
            else
            {
                availableMoves.AddRange(availableKills);
                view.Show("Can capture:\n");
                foreach (var piece in availableKills)
                {
                    view.Show(counter + " " + alphabet[piece.X] + $"{piece.Y + 1}\n");
                    counter++;
                }
            }
            return counter;
        }

        /// <summary>
        /// Checks if a piece was captured
        /// </summary>
        private static void CheckIfPieceWasKilled(List<IPiece> pieces, uint chosenMove, List<Position> availableMoves)
        {
            var pieceAtPosition = pieces.Find(x => x.Position == availableMoves[(int)(chosenMove - 1)]);
            if (pieceAtPosition != null)
            {
                pieceAtPosition.IsDead = true;
            }
        }
    }
}