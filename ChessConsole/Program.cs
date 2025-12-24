using ChessLib;
using System;

namespace ChessConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                Console.Clear();

                Game game = new Game();
                IView view = new ConsoleView();
                var controller = new ConsoleGameController(game, view);

                controller.StartGame();

                Console.WriteLine("Press Escape to exit");
                Console.WriteLine("Or press any key to start a new game");

            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}