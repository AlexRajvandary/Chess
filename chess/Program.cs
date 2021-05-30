using ChessLib;
using System;

namespace chess
{
    class Program
    {

        static void Main(string[] args)
        {
            do
            {
                Console.Clear();

                Game game = new Game(new ConsoleView());

                game.CreateNewGame();

                Console.WriteLine("Для выхода нажмите escape");

                Console.WriteLine("Или любую клавишу для того, чтобы начать игру заново");

            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
         

          
        }
    }
}
