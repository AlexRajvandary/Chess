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
        /// Starts the console game
        /// </summary>
        public void StartGame()
        {
            while (!game.IsGameOver)
            {
                ProcessTurn();
            }
            
            var state = game.GetState();
            view.Show($"Game Over! {state.GameOverReason}\n");
        }

        /// <summary>
        /// Processes one turn
        /// </summary>
        private void ProcessTurn()
        {
            var state = game.GetState();
            
            // Display board
            view.Visualize(state.BoardRepresentation, game.CurrentPlayer);

            // Check for check/checkmate
            if (state.IsCheck)
            {
                view.Show("Check!\n");
            }

            if (state.IsCheckmate)
            {
                view.Show("Checkmate!\n");
                return;
            }

            // Execute move
            ExecuteMove();

            // Clear console and show result
            Console.Clear();
            state = game.GetState();
            view.Visualize(state.BoardRepresentation, game.CurrentPlayer);
            view.Show("Press any key to continue...");
            Console.ReadLine();
        }

        /// <summary>
        /// Executes a player move
        /// </summary>
        private void ExecuteMove()
        {
            var currentPlayer = game.Players[game.CurrentPlayer];
            
            // Choose piece
            int numOfElements = 1;
            int numOfElementsInLine = 1;
            uint chosenPieceIndex = ChoosePiece(currentPlayer, ref numOfElements, ref numOfElementsInLine);
            var chosenPiece = currentPlayer.MyPieces[(int)(chosenPieceIndex - 1)];

            // Get valid moves using Game API
            var validMoves = game.GetValidMovesForPiece(chosenPiece);

            if (validMoves.Count == 0)
            {
                view.Show("No available moves for selected piece!\n" +
                    "Choose another piece\n");
                ExecuteMove();
                return;
            }

            // Show available moves
            int counter = ShowAvailableMoves(1, validMoves);

            // Choose move
            uint chosenMoveIndex = UserInput(validMoves.Count);
            var destination = validMoves[(int)(chosenMoveIndex - 1)];

            // Execute move using Game API
            var result = game.MakeMove(chosenPiece.Position, destination);

            if (!result.IsValid)
            {
                view.Show($"Invalid move: {result.ErrorMessage}\n");
                ExecuteMove();
                return;
            }

            // Show result
            if (result.IsCheck)
                view.Show("Check!\n");
            if (result.IsCheckmate)
                view.Show("Checkmate!\n");
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

    }
}