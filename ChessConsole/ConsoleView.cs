using ChessLib;
using System;

namespace ChessConsole
{
    /// <summary>
    /// Class for visualizing the game in console
    /// </summary>
    public class ConsoleView : IView
    {
        /// <summary>
        /// Background color
        /// </summary>
        public ConsoleColor BackgroundColor { get; private set; }

        /// <summary>
        /// Foreground color
        /// </summary>
        public ConsoleColor ForegroundColor { get; private set; }
        
        /// <summary>
        /// Sets background color
        /// </summary>
        /// <param name="color"></param>
        public void SetBackgroundColor(ConsoleColor color)
        {
            BackgroundColor = color;
        }
        
        /// <summary>
        /// Sets foreground color
        /// </summary>
        /// <param name="color"></param>
        public void SetForegroundColor(ConsoleColor color)
        {
            ForegroundColor = color;
        }
        
        /// <summary>
        /// Sets default colors (Black background, Gray foreground)
        /// </summary>
        public void SetDefaultColors()
        {
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.Gray;
        }
        
        /// <summary>
        /// Outputs information to console
        /// </summary>
        /// <param name="msg"></param>
        public void Show(string msg)
        {
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;
            Console.Write(msg);
        }

        string alphabet = "abcdefgh";
        
        public void Visualize(string[,] gamefield, int CurrentPlayer)
        {
            Console.Clear();
            if (CurrentPlayer % 2 == 0)
            {
                Show("White's turn\n");

                for (int j = 0; j < 9; j++)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i == 0 && j == 0)
                        {
                            Show("  ");
                            continue;
                        }
                        else if (i == 0 && j > 0)
                        {
                            SetDefaultColors();
                            // row number
                            Show(9 - j + " ");
                        }
                        else if (j == 0 && i > 0)
                        {
                            SetDefaultColors();
                            // column letter
                            Show(alphabet[i - 1].ToString().ToUpper() + " ");
                        }
                        else
                        {
                            if ((i + j) % 2 == 0)
                            {
                                // White cell with black piece symbol
                                SetBackgroundColor(ConsoleColor.White);
                                SetForegroundColor(ConsoleColor.Black);
                            }
                            else
                            {
                                // Black cell with white piece symbol
                                SetBackgroundColor(ConsoleColor.Black);
                                SetForegroundColor(ConsoleColor.White);
                            }
                            Show(gamefield[i - 1, 8 - j] + " ");
                            SetDefaultColors();
                        }
                    }
                    Show("\n");
                }
            }
            else
            {
                Show("Black's turn\n");

                for (int j = 0; j < 9; j++)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i == 0 && j == 0)
                        {
                            Show("  ");
                            continue;
                        }
                        else if (i == 0 && j > 0)
                        {
                            SetDefaultColors();
                            Show(j + " ");
                        }
                        else if (j == 0 && i > 0)
                        {
                            SetDefaultColors();
                            Show(alphabet[7 - i + 1].ToString().ToUpper() + " ");
                        }
                        else
                        {
                            if ((i + j) % 2 == 0)
                            {
                                // White cell with black piece symbol
                                SetBackgroundColor(ConsoleColor.White);
                                SetForegroundColor(ConsoleColor.Black);
                            }
                            else
                            {
                                // Black cell with white piece symbol
                                SetBackgroundColor(ConsoleColor.Black);
                                SetForegroundColor(ConsoleColor.White);
                            }
                            Show(gamefield[8 - i, j - 1] + " ");
                            SetDefaultColors();
                        }
                    }
                    Show("\n");
                }
            }
        }

        /// <summary>
        /// Default constructor sets default colors
        /// </summary>
        public ConsoleView()
        {
            SetDefaultColors();
        }
        
        /// <summary>
        /// Sets custom colors
        /// </summary>
        /// <param name="backgroundColor"></param>
        /// <param name="foregroundColor"></param>
        public ConsoleView(ConsoleColor backgroundColor, ConsoleColor foregroundColor)
        {
            BackgroundColor = backgroundColor;
            ForegroundColor = foregroundColor;
        }
    }
}

