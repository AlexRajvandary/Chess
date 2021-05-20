using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLib
{
    /// <summary>
    /// Класс служит для визуализации игры в консоли
    /// </summary>
    public class ConsoleView : IView
    {
        /// <summary>
        /// Цвет фона
        /// </summary>
        public ConsoleColor BackgroundColor { get; private set; }

        /// <summary>
        /// Цвет символов
        /// </summary>
        public ConsoleColor ForegroundColor { get; private set; }
        /// <summary>
        /// Устанавливает цвет фона
        /// </summary>
        /// <param name="color"></param>
        public void SetBackgroundColor(ConsoleColor color)
        {
            BackgroundColor = color;
        }
        /// <summary>
        /// устанавливает цвет символов
        /// </summary>
        /// <param name="color"></param>
        public void SetForegroundColor(ConsoleColor color)
        {
            ForegroundColor = color;
        }
        /// <summary>
        /// Устанавливает цвета по умолчанию (Черный фон, серые символы)
        /// </summary>
        public void SetDefaultColors()
        {
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.Gray;
        }
        /// <summary>
        /// Выводит на консоль передаваемую информацию
        /// </summary>
        /// <param name="msg"></param>
        public void Show(string msg)
        {
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;

            Console.Write(msg);

           
        }
        /// <summary>
        ///  Выводит на консоль передаваемую информацию и переносит строку
        /// </summary>
        /// <param name="msg"></param>
        public void ShowLine(string msg)
        {
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;

            Console.WriteLine(msg);
        }
        string alphabet = "abcdefgh";
        public void Visualize(string[,] gamefield, int CurrentPlayer)
        {
            if (CurrentPlayer == 1)
            {
                Show("Ход белых");
                ShowLine("");
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
                            // номер поля по вертикали
                            Show(9 - j + " ");
                        }
                        else if (j == 0 && i > 0)
                        {
                            SetDefaultColors();
                            // буква поля по горизонтали
                            Show(alphabet[i - 1].ToString().ToUpper() + " ");
                        }
                        else
                        {
                            if ((i + j) % 2 == 0)
                            {
                                //Белая клетка и черным цветом красим обозначение фигуры
                                SetBackgroundColor(ConsoleColor.White);
                                SetForegroundColor(ConsoleColor.Black);
                            }
                            else
                            {
                                //Черная клетка и белым цветом красим обозначение фигуры
                                SetBackgroundColor(ConsoleColor.Black);
                                SetForegroundColor(ConsoleColor.White);
                            }
                            Show(gamefield[i - 1, 8 - j] + " ");
                            SetDefaultColors();
                        }
                    }
                    ShowLine("");
                }

            }
            else
            {
                Show("Ход черных");
                ShowLine("");
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
                                //Белая клетка и черным цветом красим обозначение фигуры
                                SetBackgroundColor(ConsoleColor.White);
                                SetForegroundColor(ConsoleColor.Black);
                            }
                            else
                            {
                                //Черная клетка и белым цветом красим обозначение фигуры
                                SetBackgroundColor(ConsoleColor.Black);
                                SetForegroundColor(ConsoleColor.White);
                            }
                            Show(gamefield[8 - i, j - 1] + " ");
                            SetDefaultColors();
                        }
                    }
                    ShowLine("");
                }
            }
        }


        /// <summary>
        /// Конструктор по умолчанию устанавливает дефолтные цвета
        /// </summary>
        public ConsoleView()
        {
            SetDefaultColors();
        }
        /// <summary>
        /// Устанавливает желаемые цвета
        /// </summary>
        /// <param name="backgroundColor"></param>
        /// <param name="foregroundColor"></param>
        public ConsoleView(ConsoleColor backgroundColor,ConsoleColor foregroundColor)
        {
            BackgroundColor = backgroundColor;
            ForegroundColor = foregroundColor;
        }
    }
}
