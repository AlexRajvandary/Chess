using ChessLib;
using System;
using System.Collections.Generic;
using System.Linq;
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
