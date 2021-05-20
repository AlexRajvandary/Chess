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
