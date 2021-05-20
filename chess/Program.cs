using ChessLib;
using System;

namespace chess
{
    class Program
    {

        static void Main(string[] args)
        {
            Game game = new Game(new ConsoleView());

            game.CreateNewGame();

            Console.ReadLine();
        }
    }
}
