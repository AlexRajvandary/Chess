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
            
            // Display board (using game.CurrentPlayer as required by IView interface)
            view.Visualize(state.BoardRepresentation, game.CurrentPlayer);

            // Display move history
            var moveHistory = game.GetMoveHistory();
            if (!string.IsNullOrEmpty(moveHistory))
            {
                view.Show($"Moves: {moveHistory}\n\n");
            }
            
            // Display FEN notation
            var fen = game.GetFen();
            view.Show($"FEN: {fen}\n\n");

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

            // Process player input and make move
            ProcessPlayerInput();

            // Clear console and show result
            Console.Clear();
            state = game.GetState();
            view.Visualize(state.BoardRepresentation, game.CurrentPlayer);
            
            // Display updated move history
            moveHistory = game.GetMoveHistory();
            if (!string.IsNullOrEmpty(moveHistory))
            {
                view.Show($"Moves: {moveHistory}\n\n");
            }
            
            // Display updated FEN
            fen = game.GetFen();
            view.Show($"FEN: {fen}\n\n");
            
            view.Show("Press any key to continue...");
            Console.ReadLine();
        }

        /// <summary>
        /// Processes player input: selects piece, shows moves, gets destination, and makes move via Game API
        /// </summary>
        private void ProcessPlayerInput()
        {
            var state = game.GetState();
            
            // Get pieces of current player (UI logic - preparing data for display)
            var currentPlayerPieces = state.Pieces
                .Where(p => p.Color == state.CurrentPlayerColor && !p.IsDead)
                .ToList();

            if (currentPlayerPieces.Count == 0)
            {
                view.Show("No pieces available!\n");
                return;
            }

            // Choose piece (UI logic - user input)
            int numOfElements = 1;
            int numOfElementsInLine = 1;
            uint chosenPieceIndex = ChoosePiece(currentPlayerPieces, ref numOfElements, ref numOfElementsInLine);
            var chosenPiece = currentPlayerPieces[(int)(chosenPieceIndex - 1)];

            // Get valid moves using Game API
            var validMoves = game.GetValidMoves(chosenPiece.Position);

            if (validMoves.Count == 0)
            {
                view.Show("No available moves for selected piece!\n" +
                    "Choose another piece\n");
                ProcessPlayerInput();
                return;
            }

            // Show available moves (UI logic - display)
            int counter = ShowAvailableMoves(1, validMoves);

            // Choose move (UI logic - user input)
            uint chosenMoveIndex = UserInput(validMoves.Count);
            var destination = validMoves[(int)(chosenMoveIndex - 1)];

            // Execute move using Game API (business logic is in Game.MakeMove)
            var result = game.MakeMove(chosenPiece.Position, destination);

            if (!result.IsValid)
            {
                view.Show($"Invalid move: {result.ErrorMessage}\n");
                ProcessPlayerInput();
                return;
            }

            // Show result (UI logic - display)
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
        private uint ChoosePiece(List<IPiece> currentPlayerPieces, ref int numOfElements, ref int numOfElementsInLine)
        {
            view.Show("Choose a piece\n");

            // Выводит список доступных фигур по 8 штук в строку
            foreach (var piece in currentPlayerPieces)
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

            return UserInput(currentPlayerPieces.Count);
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